using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;

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
        public async Task AddListViaRestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "AddListViaRestAsync";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

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
        public async Task AddListViaRestAsyncPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.Lists);

                string listTitle = "AddListViaRestAsyncPropertiesTest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    await myList.DeleteAsync();
                }

                var listCount = web.Lists.Count();
                // Add a new list with opposite to defaults.
                myList = await web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);
                myList.Description = "TEST DESCRIPTION";
                myList.EnableAttachments = false;
                myList.EnableFolderCreation = true;
                myList.EnableMinorVersions = false;
                myList.EnableModeration = true;
                myList.EnableVersioning = true;
                myList.ForceCheckout = false;
                myList.Hidden = true;
                myList.MaxVersionLimit = 400;
                myList.MinorVersionLimit = 20;
                myList.ListExperience = ListExperience.ClassicExperience; //Eek
                await myList.UpdateAsync();


                var myListToCheck = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                // Was the list added
                Assert.IsTrue(myListToCheck.Requested);
                Assert.IsTrue(myListToCheck.Id != Guid.Empty);
                Assert.IsTrue(web.Lists.Count() == listCount + 1);
                Assert.IsFalse(myListToCheck.ContentTypesEnabled);
                Assert.AreEqual(myListToCheck.Direction, ListReadingDirection.None);
                Assert.IsNull(myListToCheck.DocumentTemplate);
                Assert.AreEqual(0, myListToCheck.DraftVersionVisibility);
                Assert.AreEqual("TEST DESCRIPTION", myListToCheck.Description);
                Assert.IsFalse(myListToCheck.EnableAttachments);
                Assert.IsTrue(myListToCheck.EnableFolderCreation);
                Assert.IsFalse(myListToCheck.EnableMinorVersions);
                Assert.IsTrue(myListToCheck.EnableModeration);
                Assert.IsTrue(myListToCheck.EnableVersioning);
                Assert.IsFalse(myListToCheck.ForceCheckout);
                Assert.IsTrue(myListToCheck.Hidden);
                Assert.IsTrue(myListToCheck.ImageUrl.Contains("itgen.png"));
                Assert.IsFalse(myListToCheck.IrmExpire);
                Assert.IsFalse(myListToCheck.IrmReject);
                Assert.IsFalse(myListToCheck.IsApplicationList);
                Assert.AreEqual(myListToCheck.ListExperience, ListExperience.ClassicExperience);
                Assert.AreEqual(20, myListToCheck.MinorVersionLimit);
                Assert.AreEqual(400, myListToCheck.MaxVersionLimit);
                Assert.AreEqual(new Guid("00bfea71-de22-43b2-a848-c05709900100"), myListToCheck.TemplateFeatureId);

                // Load the list again
                await context.Web.GetAsync(p => p.Lists);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

                // Clean up
                await myList.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddListViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);

                string listTitle = "AddListViaRest";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                myList = web.Lists.Add(listTitle, ListTemplateType.GenericList);
                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Lists.Count() == listCount + 1);

                // Load the list again
                context.Web.Get(p => p.Lists);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaRestException()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);

                string listTitle = "AddListViaRestException";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    myList = web.Lists.Add(null, ListTemplateType.GenericList);
                });

                Assert.ThrowsException<ArgumentException>(() =>
                {
                    myList = web.Lists.Add(listTitle, ListTemplateType.NoListTemplate);
                });
            }
        }

        [TestMethod]
        public async Task AddListViaRestBatchException()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web.Get(p => p.Lists);

                string listTitle = "AddListViaRestException";
                var myList = web.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Lists.Count();
                // Add a new list
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    myList = web.Lists.AddBatch(null, ListTemplateType.GenericList);
                });

                Assert.ThrowsException<ArgumentException>(() =>
                {
                    myList = web.Lists.AddBatch(listTitle, ListTemplateType.NoListTemplate);
                });
            }
        }

        [TestMethod]
        public async Task AddListViaBatchAsyncRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "AddListViaBatchAsyncRest";
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Result.Lists.Count();
                // Add a new list
                myList = await web.Result.Lists.AddBatchAsync(listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync();

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);

                // Load the list again
                await context.Web.GetBatchAsync(p => p.Lists);
                await context.ExecuteAsync();

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = context.Web.GetBatch(p => p.Lists);
                await context.ExecuteAsync();

                string listTitle = "AddListViaBatchRest";
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Result.Lists.Count();
                // Add a new list
                myList = web.Result.Lists.AddBatch(listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync();

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);

                // Load the list again
                context.Web.GetBatch(p => p.Lists);
                await context.ExecuteAsync();

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaExplicitBatchAsyncRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = await context.Web.GetBatchAsync(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListViaExplicitBatchAsyncRest";
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Result.Lists.Count();
                // Add a new list
                batch = context.BatchClient.EnsureBatch();
                myList = await web.Result.Lists.AddBatchAsync(batch, listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync(batch);

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);

                // Load the list again
                batch = context.BatchClient.EnsureBatch();
                await context.Web.GetBatchAsync(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);
            }
        }

        [TestMethod]
        public async Task AddListViaExplicitBatchRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var web = context.Web.GetBatch(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                string listTitle = "AddListViaExplicitBatchRest";
                var myList = web.Result.Lists.FirstOrDefault(p => p.Title.Equals(listTitle, StringComparison.InvariantCultureIgnoreCase));

                if (myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                var listCount = web.Result.Lists.Count();
                // Add a new list
                batch = context.BatchClient.EnsureBatch();
                myList = web.Result.Lists.AddBatch(batch, listTitle, ListTemplateType.GenericList);
                await context.ExecuteAsync(batch);

                // Was the list added
                Assert.IsTrue(myList.Requested);
                Assert.IsTrue(myList.Id != Guid.Empty);
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);

                // Load the list again
                batch = context.BatchClient.EnsureBatch();
                context.Web.GetBatch(batch, p => p.Lists);
                await context.ExecuteAsync(batch);

                // Check if we still have the same amount of lists
                Assert.IsTrue(web.Result.Lists.Count() == listCount + 1);
            }
        }

        #endregion

        #region Tests that use Graph to hit SharePoint

        [TestMethod]
        public async Task AddTeamChannelViaGraphAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Channels);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);
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
        public void AddTeamChannelViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(p => p.Channels);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelFound == null)
                {
                    int channelCount = team.Channels.Count();
                    // Add a new channel
                    channelFound = team.Channels.Add(channelName, "Test channel, will be deleted in 21 days");

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
        public async Task AddTeamChannelViaAsyncBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var team = await context.Team.GetBatchAsync(batch, p => p.Channels);
                await context.ExecuteAsync(batch);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Result.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelFound == null)
                {
                    int channelCount = team.Result.Channels.Count();
                    // Add a new channel
                    batch = context.BatchClient.EnsureBatch();
                    channelFound = await team.Result.Channels.AddBatchAsync(batch, channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync(batch);

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Result.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }

        [TestMethod]
        public async Task AddTeamChannelViaExplicitAsyncBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetBatchAsync(p => p.Channels);
                await context.ExecuteAsync();

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Result.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelFound == null)
                {
                    int channelCount = team.Result.Channels.Count();
                    // Add a new channel
                    channelFound = await team.Result.Channels.AddBatchAsync(channelName, "Test channel, will be deleted in 21 days");
                    await context.ExecuteAsync();

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Result.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }

        [TestMethod]
        public void AddTeamChannelViaBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.BatchClient.EnsureBatch();
                var team = context.Team.GetBatch(batch, p => p.Channels);
                context.Execute(batch);

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Result.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelFound == null)
                {
                    int channelCount = team.Result.Channels.Count();
                    // Add a new channel
                    batch = context.BatchClient.EnsureBatch();
                    channelFound = team.Result.Channels.AddBatch(batch, channelName, "Test channel, will be deleted in 21 days");
                    context.Execute(batch);

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Result.Channels.Count() == channelCount + 1);

                }
                else
                {
                    Assert.Inconclusive($"Channel {channelName} already exists...channels can't be immediately deleted");
                }
            }
        }

        [TestMethod]
        public void AddTeamChannelViaExplicitBatchGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.GetBatch(p => p.Channels);
                context.Execute();

                // Channel names have to be unique
                string channelName = $"Channel test {new Random().Next()}";
                // Check if the channel exists
                var channelFound = team.Result.Channels.FirstOrDefault(p => p.DisplayName == channelName);
                if (channelFound == null)
                {
                    int channelCount = team.Result.Channels.Count();
                    // Add a new channel
                    channelFound = team.Result.Channels.AddBatch(channelName, "Test channel, will be deleted in 21 days");
                    context.Execute();

                    Assert.IsNotNull(channelFound);
                    Assert.IsTrue(channelFound.Requested);
                    Assert.IsTrue(!string.IsNullOrEmpty(channelFound.Id));
                    Assert.IsTrue(team.Result.Channels.Count() == channelCount + 1);

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
