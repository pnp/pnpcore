using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.Model.Teams;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on updating data via REST or Microsoft Graph - used to test the core data add logic of the engine.
    /// Specific domain model testing will be implemented in the domain model tests
    /// </summary>
    [TestClass]
    public class UpdateTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Tests that use REST to hit SharePoint

        [TestMethod]
        public async Task UpdatePropertyViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "Documents";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    var currentDescription = myList.Description;
                    var newDescription = $"Updated on UTC {DateTime.UtcNow}";

                    myList.Description = newDescription;
                    Assert.IsTrue(myList.HasChanged("Description"));

                    await myList.UpdateAsync();

                    // Verify model status after update
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // load again from server
                    await context.Web.GetAsync(p => p.Lists);

                    // and verify again
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // reset description back to original value
                    myList.Description = currentDescription;
                    await myList.UpdateAsync();
                }
                else
                {
                    Assert.Inconclusive("Default documents library was not available anymore...");
                }
            }
        }

        [TestMethod]
        public async Task UpdatePropertyViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "Documents";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    var currentDescription = myList.Description;
                    var newDescription = $"Updated on UTC {DateTime.UtcNow}";

                    myList.Description = newDescription;
                    Assert.IsTrue(myList.HasChanged("Description"));

                    await myList.UpdateBatchAsync();
                    await context.ExecuteAsync();

                    // Verify model status after update
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // load again from server
                    await context.Web.GetBatchAsync(p => p.Lists);
                    await context.ExecuteAsync();

                    // and verify again
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // reset description back to original value
                    myList.Description = currentDescription;
                    await myList.UpdateBatchAsync();
                    await context.ExecuteAsync();
                }
                else
                {
                    Assert.Inconclusive("Default documents library was not available anymore...");
                }
            }
        }

        [TestMethod]
        public async Task UpdatePropertyViaExplicitBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = await context.Web.GetBatchAsync(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "Documents";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    var currentDescription = myList.Description;
                    var newDescription = $"Updated on UTC {DateTime.UtcNow}";

                    myList.Description = newDescription;
                    Assert.IsTrue(myList.HasChanged("Description"));

                    batch = context.BatchClient.EnsureBatch();
                    await myList.UpdateBatchAsync(batch);
                    await context.ExecuteAsync(batch);

                    // Verify model status after update
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // load again from server
                    batch = context.BatchClient.EnsureBatch();
                    await context.Web.GetBatchAsync(batch, p => p.Lists);
                    await context.ExecuteAsync(batch);

                    // and verify again
                    Assert.IsTrue(myList.Description == newDescription);
                    Assert.IsFalse(myList.HasChanged("Description"));

                    // reset description back to original value
                    myList.Description = currentDescription;
                    batch = context.BatchClient.EnsureBatch();
                    await myList.UpdateBatchAsync(batch);
                    await context.ExecuteAsync(batch);
                }
                else
                {
                    Assert.Inconclusive("Default documents library was not available anymore...");
                }
            }
        }

        [TestMethod]
        public async Task UpdateNonLoadedProperty()
        {
            string updatedDescription = "UpdateNonLoadedProperty test";
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Force REST for this test
                context.GraphFirst = false;

                var web = await context.Web.GetAsync(p => p.Title);

                // description was not loaded
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Description));

                // Although description was not loaded we still want to be able to set a value to it and update
                web.Description = "temp";
                web.Description = updatedDescription;

                // Description should now be available after setting a value
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Description));

                await web.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context2.GraphFirst = false;

                    // load the web again with both properties
                    var web2 = await context.Web.GetAsync(p => p.Title, p => p.Description);

                    // description update should have happened
                    Assert.IsTrue(web2.Title == web.Title);
                    Assert.IsTrue(web2.Description == updatedDescription);
                }

                // set description back as empty
                web.Description = "";
                await web.UpdateAsync();
            }
        }
        #endregion

        #region Tests that use Microsoft Graph

        [TestMethod]
        public async Task UpdatePropertyViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Channels);

                // Find first updatable channel
                var channelToUpdate = team.Channels.FirstOrDefault(p => p.DisplayName != "General");

                if (channelToUpdate == null)
                {
                    channelToUpdate = await team.Channels.AddAsync($"Channel test {new Random().Next()}", "Test channel, will be deleted in 21 days");
                }

                string newChannelDescription = $"Updated on {DateTime.UtcNow}";
                channelToUpdate.Description = newChannelDescription;

                Assert.IsTrue(channelToUpdate.HasChanged("Description"));
                
                await channelToUpdate.UpdateAsync();

                // Verify model status after update
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

                // load again from server
                await context.Team.GetAsync(p => p.Channels);

                // and verify again
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

            }
        }

        [TestMethod]
        public async Task UpdatePropertyViaBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                // Find first updatable channel
                var channelToUpdate = team.Channels.FirstOrDefault(p => p.DisplayName != "General");

                if (channelToUpdate == null)
                {
                    channelToUpdate = await team.Channels.AddBatchAsync($"Channel test {new Random().Next()}", "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync();
                }

                string newChannelDescription = $"Updated on {DateTime.UtcNow}";
                channelToUpdate.Description = newChannelDescription;

                Assert.IsTrue(channelToUpdate.HasChanged("Description"));

                await channelToUpdate.UpdateBatchAsync();
                await context.ExecuteAsync();

                // Verify model status after update
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

                // load again from server
                await context.Team.GetBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                // and verify again
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

            }
        }

        [TestMethod]
        public async Task UpdatePropertyViaExplicitBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var team = await context.Team.GetBatchAsync(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // Find first updatable channel
                var channelToUpdate = team.Channels.FirstOrDefault(p => p.DisplayName != "General");

                if (channelToUpdate == null)
                {
                    batch = context.BatchClient.EnsureBatch();
                    channelToUpdate = await team.Channels.AddBatchAsync(batch, $"Channel test {new Random().Next()}", "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync(batch);
                }

                string newChannelDescription = $"Updated on {DateTime.UtcNow}";
                channelToUpdate.Description = newChannelDescription;

                Assert.IsTrue(channelToUpdate.HasChanged("Description"));

                batch = context.BatchClient.EnsureBatch();
                await channelToUpdate.UpdateBatchAsync(batch);
                await context.ExecuteAsync(batch);

                // Verify model status after update
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

                // load again from server
                batch = context.BatchClient.EnsureBatch();
                await context.Team.GetBatchAsync(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // and verify again
                Assert.IsTrue(channelToUpdate.Description == newChannelDescription);
                Assert.IsFalse(channelToUpdate.HasChanged("Description"));

            }
        }

        [TestMethod]
        public async Task UpdateComplexModelPropertyViaGraph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync();

                var currentAllowGiphy = team.FunSettings.AllowGiphy;
                var currentGiphyRating = team.FunSettings.GiphyContentRating;
                var currentAllowDeleteChannels = team.GuestSettings.AllowDeleteChannels;

                var newAllowGiphy = !currentAllowGiphy;
                var newGiphyRating = currentGiphyRating == TeamGiphyContentRating.Moderate ? TeamGiphyContentRating.Strict : TeamGiphyContentRating.Moderate;
                var newAllowDeleteChannels = !currentAllowDeleteChannels;

                team.FunSettings.AllowGiphy = newAllowGiphy;
                team.FunSettings.GiphyContentRating = newGiphyRating;
                team.GuestSettings.AllowDeleteChannels = newAllowDeleteChannels;

                Assert.IsTrue(team.HasChanged("FunSettings"));
                Assert.IsTrue(team.HasChanged("GuestSettings"));

                await team.UpdateAsync();

                Assert.IsTrue(team.FunSettings.AllowGiphy == newAllowGiphy);
                Assert.IsTrue(team.FunSettings.GiphyContentRating == newGiphyRating);
                Assert.IsTrue(team.GuestSettings.AllowDeleteChannels == newAllowDeleteChannels);
                Assert.IsFalse(team.HasChanged("FunSettings"));
                Assert.IsFalse(team.HasChanged("GuestSettings"));

                await context.Team.GetAsync();

                Assert.IsTrue(team.FunSettings.AllowGiphy == newAllowGiphy);
                Assert.IsTrue(team.FunSettings.GiphyContentRating == newGiphyRating);
                Assert.IsTrue(team.GuestSettings.AllowDeleteChannels == newAllowDeleteChannels);
                Assert.IsFalse(team.HasChanged("FunSettings"));
                Assert.IsFalse(team.HasChanged("GuestSettings"));

            }
        }

        #endregion
    }
}
