using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Test cases for the BatchClient class
    /// </summary>
    [TestClass]
    public class BatchClientTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.GenerateMockingDebugFiles = true;
        }

        [TestMethod]
        public void PropertiesInitialization()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Assert.IsNotNull(context.BatchClient.PnPContext == context);
                Assert.IsNotNull(context.BatchClient.EnsureBatch());
            }
        }

        [TestMethod]
        public async Task RemoveProcessedExplicitBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();

                context.Web.Get(batch);

                Assert.IsFalse(batch.Executed);
                Assert.IsTrue(context.BatchClient.ContainsBatch(batch.Id));

                await context.ExecuteAsync(batch);

                Assert.IsTrue(batch.Executed);

                // Trigger a second call into the batch client, this should have removed the previous batch
                await context.Web.GetAsync();

                Assert.IsFalse(context.BatchClient.ContainsBatch(batch.Id));
            }
        }

        [TestMethod]
        public async Task RemoveProcessedBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.Web.Get();

                Assert.IsFalse(context.CurrentBatch.Executed);
                var implicitBatchId = context.CurrentBatch.Id;
                Assert.IsTrue(context.BatchClient.ContainsBatch(implicitBatchId));

                await context.ExecuteAsync();

                // CurrentBatch is repopulated when the previous batch was executed, hence we should see false here and id should be different
                Assert.IsFalse(context.CurrentBatch.Executed);
                Assert.IsTrue(context.CurrentBatch.Id != implicitBatchId);

                // Trigger a second call into the batch client, this should have removed the previous batch
                await context.Web.GetAsync();

                Assert.IsFalse(context.BatchClient.ContainsBatch(implicitBatchId));
            }
        }

        [TestMethod]
        public async Task DedupBatchRequests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // first get
                var web = context.Web.Get(p => p.Title);
                // second get
                var web2 = context.Web.Get(p => p.Title);
                // third get
                var web3 = context.Web.Get(p => p.Title);

                // Grab the id of the current batch so we can later on find it back
                Guid currentBatchId = context.CurrentBatch.Id;

                // We added three requests to the the current batch
                Assert.IsTrue(context.CurrentBatch.Requests.Count == 3);

                // The model was not yet requested
                Assert.IsFalse(web.Requested);
                Assert.IsFalse(web2.Requested);
                Assert.IsFalse(web3.Requested);

                await context.ExecuteAsync();

                // all variables should point to the same model, so all should be requested
                Assert.IsTrue(web.Requested);
                Assert.IsTrue(web2.Requested);
                Assert.IsTrue(web3.Requested);

                var executedBatch = context.BatchClient.GetBatchById(currentBatchId);
                // Batch should be marked as executed
                Assert.IsTrue(executedBatch.Executed);
                // The 2 duplicate requests should have been removed
                Assert.IsTrue(executedBatch.Requests.Count == 1);

            }
        }

        [TestMethod]
        public async Task MixedGraphSharePointBatchRunsAsSharePointBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                if (context.GraphFirst)
                {
                    // Get web title : this can be done using rest and graph and since we're graphfirst this 
                    context.Web.Get(p => p.Title);
                    // Get the web searchscope: this will require a rest call
                    context.Web.Get(p => p.SearchScope);

                    // Grab the id of the current batch so we can later on find it back
                    Guid currentBatchId = context.CurrentBatch.Id;
                    var batchToExecute = context.BatchClient.GetBatchById(currentBatchId);
                    // We added three requests to the the current batch
                    Assert.IsTrue(context.CurrentBatch.Requests.Count == 2);
                    // The first call in the batch is a graph call
                    Assert.IsTrue(context.CurrentBatch.Requests[0].ApiCall.Type == ApiType.Graph);
                    // The second one is rest
                    Assert.IsTrue(context.CurrentBatch.Requests[1].ApiCall.Type == ApiType.SPORest);
                    // For the first one there's a backup rest call available
                    Assert.IsTrue(context.CurrentBatch.Requests[0].BackupApiCall.Type == ApiType.SPORest);
                    // Execute the batch
                    await context.ExecuteAsync();

                    // Check if batchclient did transform the graph calls into rest calls so that only one batch request to the server was needed
                    Assert.IsTrue(batchToExecute.Requests[0].ApiCall.Type == ApiType.SPORest);
                    Assert.IsTrue(batchToExecute.Requests[0].ApiCall.Equals(batchToExecute.Requests[0].BackupApiCall));

                    // Check that the original batch was marked as executed                    
                    Assert.IsTrue(batchToExecute.Executed);
                }
                else
                {
                    Assert.Inconclusive("Context was not by default set to GraphFirst, running this test makes no sense");
                }
            }
        }

        [TestMethod]
        public async Task SplitSharePointAndGraphRequestsInTwoBatches()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Graph only request (there's no option to fallback to a SharePoint REST call)
                context.Team.Get();
                // SharePoint REST request
                context.Web.Get(p => p.SearchScope, p => p.Title);

                // Grab the id of the current batch so we can later on find it back
                Guid currentBatchId = context.CurrentBatch.Id;
                var batchToExecute = context.BatchClient.GetBatchById(currentBatchId);
                // We added 2 requests to the the current batch, but since we're also loading the Teams channels by default we have 3 requests in our queue
                Assert.IsTrue(batchToExecute.Requests.Count == 3);

                // The first (and second due to Teams Channels load) call in the batch are graph calls
                Assert.IsTrue(batchToExecute.Requests[0].ApiCall.Type == ApiType.Graph);
                // The third one is rest
                Assert.IsTrue(batchToExecute.Requests[2].ApiCall.Type == ApiType.SPORest);
                // For the first one there's no backup rest call available
                Assert.IsTrue(batchToExecute.Requests[0].BackupApiCall.Equals(default(ApiCall)));
                // Execute the batch, this will result in 2 individual batches being executed, one rest and one graph
                await context.ExecuteAsync();

                // verify data was loaded 
                Assert.IsTrue(context.Team.Requested);
                Assert.IsTrue(context.Team.IsPropertyAvailable(p => p.Id));
                Assert.IsTrue(context.Web.Requested);
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task MergeGetBatchResults()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list1 = context.Web.Lists.BatchGetByTitle("Site Assets", p => p.Title, p => p.NoCrawl);
                var list1Again = context.Web.Lists.BatchGetByTitle("Site Assets", p => p.Title, p => p.EnableVersioning, p => p.Items);
                await context.ExecuteAsync();

                var siteAssetsCount = context.Web.Lists.Where(p => p.Title == "Site Assets");

                // The 2 individual loads should have been merged to a single loaded list
                Assert.IsTrue(siteAssetsCount.Count() == 1);
                // The properties from both loads should available on the first loaded model
                Assert.IsTrue(list1.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(list1.IsPropertyAvailable(p => p.NoCrawl));
                
                Assert.IsTrue(list1.IsPropertyAvailable(p => p.EnableVersioning));
                Assert.IsTrue(list1.IsPropertyAvailable(p => p.Items));
                // Site Assets should have items
                Assert.IsTrue(list1.Items.Count() > 0);
            }
        }

        [TestMethod]
        public async Task TestBatchRequestIdPopulation()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Get web title : this can be done using rest and graph and since we're graphfirst this will go via Graph
                await context.Web.GetAsync(p => p.Title);

                // The batch request id should be populated
                Guid batchRequestId = (context.Web as Web).BatchRequestId;
                Assert.IsTrue(batchRequestId != Guid.Empty);

                // Get the web searchscope: this will require a rest call and update the loaded web object
                await context.Web.GetAsync(p => p.SearchScope);

                // Since the web object was reloaded, the batch request must have changed
                Assert.IsTrue((context.Web as Web).BatchRequestId != Guid.Empty);
                Assert.IsTrue((context.Web as Web).BatchRequestId != batchRequestId);
            }
        }

        // Since graph batching with Teams is not reliable whenever the batch size grows these tests are commented out
        // TODO: rewrite using other Graph entities
        //[TestMethod]
        public async Task HandleMaxRequestsInGraphBatch2()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Get the primary channel without expression...should default to v1.0 graph
                var team = await context.Team.GetAsync(p => p.PrimaryChannel);

                // Are the expected properties populated
                Assert.IsTrue(team.IsPropertyAvailable(p => p.PrimaryChannel));
                Assert.IsTrue(team.PrimaryChannel.IsPropertyAvailable(p => p.Id));

                // Add 21 messages to the Channel..this triggers the batch splitting
                for (int i = 1; i <= 21; i++)
                {
                    team.PrimaryChannel.Messages.Add($"Message{i}");
                }
                await context.ExecuteAsync();

                // Try to get the beta property messages, this is a collection
                await team.PrimaryChannel.GetAsync(p => p.Messages);

                for (int i = 1; i <= 21; i++)
                {
                    var message = team.PrimaryChannel.Messages.FirstOrDefault(p => p.Body.Content == $"Message{i}");
                    if (message == null)
                    {
                        Assert.Fail("Message was not found");
                    }
                }

            }
        }


        //[TestMethod]
        public async Task HandleMaxRequestsInGraphBatch1()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Channels);

                // Find first updatable channel
                var generalChannel = team.Channels.Where(p => p.DisplayName == "General").FirstOrDefault();

                if (generalChannel != null)
                {
                    // Load tabs
                    await generalChannel.GetAsync(p => p.Tabs);

                    //await generalChannel.Tabs.AddDocumentLibraryTabAsync("Tab1", new Uri($"{context.Uri}/Shared Documents"));
                    //await generalChannel.Tabs.AddWikiTabAsync("Tab1");

                    // Batch up 21 tab creations...this triggers the batch splitting
                    for (int i = 1; i <= 21; i++)
                    {
                        generalChannel.Tabs.AddWikiTab($"Tab{i}");
                    }

                    // Send batch to the server
                    await context.ExecuteAsync();


                    // Cleanup created tabs
                    await generalChannel.GetAsync(p => p.Tabs);

                    foreach (var tab in generalChannel.Tabs)
                    {
                        if (tab.DisplayName.StartsWith("Tab"))
                        {
                            // Batch up delete
                            tab.Delete();
                        }
                    }

                    // Send batch to the server
                    await context.ExecuteAsync();
                }
            }
        }
        
    }
}
