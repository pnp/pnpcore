using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on deleting data via REST or Microsoft Graph - used to test the core data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class DeleteTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST

        [TestMethod]
        public async Task DeleteListViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                string listTitle = "DeleteListViaRest";
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                // Delete the list
                await myList.WithSPResponseHeaders((responseHeaders) => { 
                    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"])); 
                }).DeleteAsync();
                // Verify that the list was removed from the model collection as well
                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);

                // Was the list added
                bool exceptionThrown = false;
                try
                {
                    var deletedListDescription = myList.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the lists again
                await context.Web.LoadAsync(p => p.Lists);

                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                string listTitle = "DeleteListViaBatchRest";
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddBatchAsync(listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                // Delete the list
                var batch = context.NewBatch();

                // Shows how to get the response headers from the batch for the SharePoint REST and CSOM requests
                await myList.WithSPResponseHeaders((responseHeaders) => {
                    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"]));
                }).DeleteBatchAsync(batch);
                await context.ExecuteAsync(batch);

                // Check that the batch response was OK
                Assert.IsTrue(batch.Requests[0].ResponseHttpStatusCode == HttpStatusCode.OK);

                // Shortcut to just get the SPRequestGuid from the batch response
                Assert.IsTrue(batch.Requests[0].SPRequestGuid != null);

                // Was the list added
                bool exceptionThrown = false;
                try
                {
                    var deletedListDescription = myList.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the lists again
                await context.Web.LoadBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaExplicitBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                Core.Services.Batch batch = null;
                string listTitle = "DeleteListViaExplicitBatchRest";
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    batch = context.BatchClient.EnsureBatch();
                    myList = await context.Web.Lists.AddBatchAsync(batch, listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync(batch);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                // Delete the list
                batch = context.BatchClient.EnsureBatch();
                await myList.DeleteBatchAsync(batch);
                await context.ExecuteAsync(batch);

                // Was the list added
                bool exceptionThrown = false;
                try
                {
                    var deletedListDescription = myList.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the lists again
                batch = context.BatchClient.EnsureBatch();
                await context.Web.LoadBatchAsync(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaRestId()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                string listTitle = TestCommon.GetPnPSdkTestAssetName("DeleteListViaRestId");
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.Lists.DeleteByIdAsync(myList.Id);

                    // Get the lists again
                    await context2.Web.LoadAsync(p => p.Lists);

                    Assert.IsTrue(context2.Web.Lists.Length == listCount - 1);
                }
            }
        }

        [TestMethod]
        public async Task DeleteListViaRestIdExistsInModel()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                string listTitle = TestCommon.GetPnPSdkTestAssetName("DeleteListViaRestIdExistsInModel");
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                // Delete the list
                await context.Web.Lists.DeleteByIdAsync(myList.Id);

                // Verify the list was removed from the model collection
                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);

                // Using a reference to a removed list should result in a exception
                bool exceptionThrown = false;
                try
                {
                    var deletedListDescription = myList.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the lists again
                await context.Web.LoadAsync(p => p.Lists);

                // and check if the list was deleted
                Assert.IsTrue(context.Web.Lists.Length == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaRestIdBatch()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the lists
                await context.Web.LoadAsync(p => p.Lists);

                string listTitle = TestCommon.GetPnPSdkTestAssetName("DeleteListViaRestIdBatch");
                var myList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == listTitle);

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = context.Web.Lists.Length;

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    // Add the delete to the current batch
                    await context2.Web.Lists.DeleteByIdBatchAsync(myList.Id);

                    // Execute the current batch
                    await context2.ExecuteAsync();

                    // Get the lists again
                    await context2.Web.LoadAsync(p => p.Lists);

                    Assert.IsTrue(context2.Web.Lists.Length == listCount - 1);
                }
            }
        }
        #endregion

        #region Tests that use Graph

        [TestMethod]
        public async Task DeleteChannelViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Initial load of channels
                await context.Team.LoadAsync(p => p.Channels);

                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = context.Team.Channels.AsRequested().FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    channelToDelete = await context.Team.Channels.AddAsync(channelName, "Test channel, will be deleted in 21 days");
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = context.Team.Channels.Length;

                // Delete channel
                await channelToDelete.DeleteAsync();

                // Was the channel deleted?
                Assert.ThrowsException<ClientException>(() =>
                {
                    var deletedChannelDescription = channelToDelete.Description;
                });

                // Get the channel again
                await context.Team.LoadAsync(p => p.Channels);

                // We should have one channel less
                Assert.IsTrue(context.Team.Channels.Length == channelCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteChannelViaBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Initial load of channels
                await context.Team.LoadBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = context.Team.Channels.AsRequested().FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    channelToDelete = await context.Team.Channels.AddBatchAsync(channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = context.Team.Channels.Length;

                // Delete channel
                await channelToDelete.DeleteBatchAsync();
                await context.ExecuteAsync();

                // Was the channel deleted?
                Assert.ThrowsException<ClientException>(() =>
                {
                    var deletedChannelDescription = channelToDelete.Description;
                });

                // Get the channel again
                await context.Team.LoadBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                // We should have one channel less
                Assert.IsTrue(context.Team.Channels.Length == channelCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteChannelViaExplicitBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Initial load of channels
                await context.Team.LoadBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                Core.Services.Batch batch = null;
                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = context.Team.Channels.AsRequested().FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    batch = context.BatchClient.EnsureBatch();
                    channelToDelete = await context.Team.Channels.AddBatchAsync(batch, channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync(batch);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = context.Team.Channels.Length;

                // Delete channel
                batch = context.BatchClient.EnsureBatch();
                await channelToDelete.DeleteBatchAsync(batch);
                await context.ExecuteAsync(batch);

                // Was the channel deleted?
                Assert.ThrowsException<ClientException>(() =>
                {
                    var deletedChannelDescription = channelToDelete.Description;
                });

                // Get the channel again
                batch = context.BatchClient.EnsureBatch();
                await context.Team.LoadBatchAsync(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // We should have one channel less
                Assert.IsTrue(context.Team.Channels.Length == channelCount - 1);
            }
        }
        #endregion
    }
}
