using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.QueryModel;
using System.Net.Http;
using System.Threading;

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
        public async Task PropertiesInitialization()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsNotNull(context.BatchClient.PnPContext == context);
                Assert.IsNotNull(context.BatchClient.EnsureBatch());
            }
        }

        [TestMethod]
        public async Task RemoveProcessedExplicitBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();

                await context.Web.GetBatchAsync(batch);

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetBatchAsync();

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // first get
                var web = await context.Web.GetBatchAsync(p => p.Title);
                // second get
                var web2 = await context.Web.GetBatchAsync(p => p.Title);
                // third get
                var web3 = await context.Web.GetBatchAsync(p => p.Title);

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                if (context.GraphFirst)
                {
                    // Get web title : this can be done using rest and graph and since we're graphfirst this 
                    await context.Web.GetBatchAsync(p => p.Title);
                    // Get the web searchscope: this will require a rest call
                    await context.Web.GetBatchAsync(p => p.SearchScope);

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Graph only request (there's no option to fallback to a SharePoint REST call)
                await context.Team.GetBatchAsync();
                // SharePoint REST request
                await context.Web.GetBatchAsync(p => p.SearchScope, p => p.Title);

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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list1 = await context.Web.Lists.BatchGetByTitleAsync("Site Assets", p => p.Title, p => p.NoCrawl);
                var list1Again = await context.Web.Lists.BatchGetByTitleAsync("Site Assets", p => p.Title, p => p.EnableVersioning, p => p.Items);
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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
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

        [TestMethod]
        public async Task HandleMaxRequestsInGraphBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string newGroupName = SharePoint.TaxonomyTests.GetGroupName(context);

                // Add new group
                var group = await context.TermStore.Groups.AddAsync(newGroupName);

                Assert.IsNotNull(group);
                Assert.IsTrue(group.Requested);
                Assert.IsTrue(group.Name == newGroupName);

                var termSet = await group.Sets.AddAsync("PnPSet1", "Set description");

                for (int i = 1; i <= 30; i++)
                {
                    await termSet.Terms.AddBatchAsync($"T{i}");
                }
                // This batch of 30 requests in split into 2 batches when being sent to Microsoft 365
                await context.ExecuteAsync();

                // Load the terms again and verify that the batch splitting resulted in all terms being created
                await termSet.GetAsync(p => p.Terms);
                for (int i = 1; i <= 30; i++)
                {
                    var term = termSet.Terms.FirstOrDefault(p => p.Labels.FirstOrDefault(p => p.Name == $"T{i}") != null);
                    if (term == null)
                    {
                        Assert.Fail($"Term T{i} was not found");
                    }
                }

                // Delete term set 
                await termSet.DeleteAsync();

                // Add a delay for live testing...seems that immediately deleting the group after the termsets are deleted does 
                // not always work (getting error about deleting non empty term group)
                if (!TestCommon.Instance.Mocking)
                {
                    Thread.Sleep(4000);
                }

                // Delete the group again
                await group.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UnresolvedToken()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var api = new ApiCall($"_api/web?$select={{GraphId}}%2cWelcomePage", ApiType.SPORest)
                {
                    Interactive = true
                };

                bool exceptionThrown = false;
                try
                {
                    var apiResponse = await (context.Web as Web).RequestAsync(api, HttpMethod.Get);
                }
                catch(Exception ex)
                {
                    if (ex is ClientException && (ex as ClientException).Error.Type == ErrorType.UnresolvedTokens)
                    {
                        exceptionThrown = true;
                    }
                }

                Assert.IsTrue(exceptionThrown);
            }
        }

        #region Interactive request tests

        [TestMethod]
        public async Task InteractiveGetRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var api = new ApiCall($"_api/web?$select=Id%2cWelcomePage", ApiType.SPORest)
                {
                    Interactive = true
                };

                var apiResponse = await (context.Web as Web).RequestAsync(api, HttpMethod.Get);

                Assert.IsTrue(apiResponse.Executed);
                Assert.IsTrue(!string.IsNullOrEmpty(apiResponse.Requests.First().Value.ResponseJson));
                Assert.IsTrue(apiResponse.Requests.First().Value.ResponseHttpStatusCode == System.Net.HttpStatusCode.OK);
                Assert.IsTrue(!string.IsNullOrEmpty(context.Web.WelcomePage));
            }
        }

        [TestMethod]
        public async Task InteractivePostRequest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "InteractivePostRequest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();

                // Add a new list via interactive request
                string body = $"{{\"__metadata\":{{\"type\":\"SP.List\"}},\"BaseTemplate\":100,\"Title\":\"{listTitle}\"}}";

                var api = new ApiCall($"_api/web/lists", ApiType.SPORest, body)
                {
                    Interactive = true
                };

                var apiResponse = await (context.Web as Web).RequestAsync(api, HttpMethod.Post);

                Assert.IsTrue(apiResponse.Executed);
                Assert.IsTrue((int)apiResponse.Requests.First().Value.ResponseHttpStatusCode < 400);

                // Load the list again
                await context.Web.GetAsync(p => p.Lists);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

            }
        }

        #endregion

    }
}
