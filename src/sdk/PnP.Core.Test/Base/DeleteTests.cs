using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
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

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task DeleteListViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "DeleteListViaRest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList == null)
                {
                    myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();

                // Delete the list
                await myList.DeleteAsync();
                // Verify that the list was removed from the model collection as well
                Assert.IsTrue(web.Lists.Count() == listCount - 1);

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
                await context.Web.GetAsync(p => p.Lists);

                Assert.IsTrue(web.Lists.Count() == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "DeleteListViaBatchRest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList == null)
                {
                    myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();

                // Delete the list
                myList.Delete();
                await context.ExecuteAsync();

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
                context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                Assert.IsTrue(web.Lists.Count() == listCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteListViaExplicitBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = context.Web.Get(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "DeleteListViaExplicitBatchRest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList == null)
                {
                    batch = context.BatchClient.EnsureBatch();
                    myList = web.Lists.Add(batch, listTitle, ListTemplateType.GenericList);
                    await context.ExecuteAsync(batch);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();

                // Delete the list
                batch = context.BatchClient.EnsureBatch();
                myList.Delete(batch);
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
                context.Web.Get(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                Assert.IsTrue(web.Lists.Count() == listCount - 1);
            }
        }
        #endregion

        #region Tests that use REST to hit Graph

        [TestMethod]
        public async Task DeleteChannelViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Channels);

                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    channelToDelete = await team.Channels.AddAsync(channelName, "Test channel, will be deleted in 21 days");
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = team.Channels.Count();

                // Delete channel
                await channelToDelete.DeleteAsync();

                // Was the channel added
                bool exceptionThrown = false;
                try
                {
                    var deletedChannelDescription = channelToDelete.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the channel again
                await context.Team.GetAsync(p => p.Channels);

                // We should have one channel less
                Assert.IsTrue(team.Channels.Count() == channelCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteChannelViaBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(p => p.Channels);
                await context.ExecuteAsync();

                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    channelToDelete = team.Channels.Add(channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = team.Channels.Count();

                // Delete channel
                channelToDelete.Delete();
                await context.ExecuteAsync();

                // Was the channel added
                bool exceptionThrown = false;
                try
                {
                    var deletedChannelDescription = channelToDelete.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the channel again
                context.Team.Get(p => p.Channels);
                await context.ExecuteAsync();

                // We should have one channel less
                Assert.IsTrue(team.Channels.Count() == channelCount - 1);
            }
        }

        [TestMethod]
        public async Task DeleteChannelViaExplicitBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var team = context.Team.Get(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                string channelName = $"Channel test {new Random().Next()}";

                // Find first updatable channel
                var channelToDelete = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);

                if (channelToDelete == null)
                {
                    batch = context.BatchClient.EnsureBatch();
                    channelToDelete = team.Channels.Add(batch, channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync(batch);
                }
                else
                {
                    Assert.Inconclusive("Test data set should be setup to not have the channel available.");
                }

                var channelCount = team.Channels.Count();

                // Delete channel
                batch = context.BatchClient.EnsureBatch();
                channelToDelete.Delete(batch);
                await context.ExecuteAsync(batch);

                // Was the channel added
                bool exceptionThrown = false;
                try
                {
                    var deletedChannelDescription = channelToDelete.Description;
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                Assert.IsTrue(exceptionThrown);

                // Get the channel again
                batch = context.BatchClient.EnsureBatch();
                context.Team.Get(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // We should have one channel less
                Assert.IsTrue(team.Channels.Count() == channelCount - 1);
            }
        }
        #endregion

    }
}
