using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model.SharePoint.Core;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on adding data via REST or Microsoft Graph - used to test the core data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class AddTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task AddListViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "AddListViaRest";
                var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

                // Load the list again
                await context.Web.GetAsync(p => p.Lists);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "AddListViaBatchRest";
                var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync();

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

                // Load the list again
                context.Web.Get(p => p.Lists);
                await context.ExecuteAsync();

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaExplicitBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = context.Web.Get(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListViaExplicitBatchRest";
                var myList = web.Lists.Where(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                batch = context.BatchClient.EnsureBatch();
                myList = web.Lists.Add(batch, listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync(batch);

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

                // Load the list again
                batch = context.BatchClient.EnsureBatch();
                context.Web.Get(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);
            }
        }

        #endregion

        #region Tests that use Graph to hit SharePoint
        
        [TestMethod]
        public async Task AddTeamChannelViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Channels);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();
                if (channelFound == null)
                {
                    int channelCount = team.Channels.Count();
                    // Add a new channel
                    channelFound = await team.Channels.AddAsync(channelName, "Test channel, will be deleted in 21 days");

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }

        [TestMethod]
        public async Task AddTeamChannelViaBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var team = context.Team.Get(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();
                if (channelFound == null)
                {
                    int channelCount = team.Channels.Count();
                    // Add a new channel
                    batch = context.BatchClient.EnsureBatch();
                    channelFound = team.Channels.Add(batch, channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync(batch);

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }

        [TestMethod]
        public async Task AddTeamChannelViaExplicitBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(p => p.Channels);
                await context.ExecuteAsync();

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();
                if (channelFound == null)
                {
                    int channelCount = team.Channels.Count();
                    // Add a new channel
                    channelFound = team.Channels.Add(channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync();

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }
        #endregion

    }
}
