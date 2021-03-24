using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamChannelTabTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetChannelTabPropertiesTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                var tabs = channel.Tabs.AsRequested();
                var firstTab = tabs.First();
                await firstTab.LoadAsync(o => o.WebUrl, o => o.SortOrderIndex);

                Assert.IsNotNull(firstTab.WebUrl);
                Assert.IsNotNull(firstTab.SortOrderIndex); //This is not found on the graph V1.0
            }
        }

        [TestMethod]
        public async Task GetChannelTabConfigurationTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                var tabs = channel.Tabs.AsRequested();
                var firstTab = tabs.First();
                await firstTab.LoadAsync(o => o.Configuration);
                var config = firstTab.Configuration;

                Assert.IsTrue(config.IsPropertyAvailable(o => o.WebsiteUrl));
                Assert.IsTrue(config.IsPropertyAvailable(o => o.RemoveUrl));
                Assert.IsTrue(config.IsPropertyAvailable(o => o.HasContent));
                Assert.IsFalse(config.HasContent); // This could fail if developer adds content in the wiki
                Assert.IsNull(config.WebsiteUrl);
                Assert.IsNull(config.RemoveUrl);
                Assert.IsNotNull(config.WikiDefaultTab);
                Assert.IsNotNull(config.WikiTabId);
            }
        }

        [TestMethod]
        public async Task AddChannelDocumentLibTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                await channel.LoadAsync(o => o.Tabs);
                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = await channel.Tabs.AddDocumentLibraryTabAsync(testTabName, new Uri(testSiteLib));

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
            }
        }

        [TestMethod]
        public void AddChannelDocumentLibTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel.Load(o => o.Tabs);
                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = channel.Tabs.AddDocumentLibraryTab(testTabName, new Uri(testSiteLib));

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.Delete();
            }
        }

        [TestMethod]
        public void AddChannelDocumentLibBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.GetBatch(o => o.Channels);
                context.Execute();
                Assert.IsTrue(team.Result.Channels.Length > 0);

                var channelQuery = team.Result.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channelQuery);

                var channel = channelQuery.GetBatch(o => o.Tabs);
                context.Execute();

                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = channel.Result.Tabs.AddDocumentLibraryTabBatch(testTabName, new Uri(testSiteLib));

                context.Execute();

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch();
                context.Execute();
            }
        }

        [TestMethod]
        public void UpdateChannelTabTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);

                // Get the Channel "General" 
                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

                // Load the channel tab collection
                channel = channel.Get(o => o.Tabs);

                var siteDocLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var tabName = "Important Documents";
                var newDocTab = channel.Tabs.AddDocumentLibraryTab(tabName, new Uri(siteDocLib));

                channel = channel.Get(o => o.Tabs);
                var tab = channel.Tabs.AsRequested().FirstOrDefault(i => i.DisplayName == tabName);
                tab.DisplayName = "Most Important Documents";
                tab.Update();

                // Cleanup
                tab.Delete();
            }
        }

        [TestMethod]
        public void AddChannelDocumentLibSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                var team = context.Team.GetBatch(batch, o => o.Channels);
                context.Execute(batch);

                var channelQuery = team.Result.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                var channel = channelQuery.GetBatch(batch, o => o.Tabs);

                context.Execute(batch);
                Assert.IsTrue(team.Result.Channels.Length > 0);
                Assert.IsNotNull(channel);

                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = channel.Result.Tabs.AddDocumentLibraryTabBatch(batch, testTabName, new Uri(testSiteLib));

                context.Execute(batch);
                
                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch(batch);
                context.Execute(batch);
            }
        }

        [TestMethod]
        public async Task AddWikiTabAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                
                var testTabName = "WikiTestTab";
                var result = await channel.Tabs.AddWikiTabAsync(testTabName);

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
            }
        }

        [TestMethod]
        public void AddWikiTabTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var testTabName = "WikiTestTab";
                var result = channel.Tabs.AddWikiTab(testTabName);

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.Delete();
            }
        }

        [TestMethod]
        public void AddWikiTabBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.GetBatch(o => o.Channels);
                context.Execute();
                Assert.IsTrue(team.Result.Channels.Length > 0);

                var channelQuery = team.Result.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channelQuery);

                var channel = channelQuery.GetBatch(o => o.Tabs);
                context.Execute();

                var testTabName = "WikiTestTab";
                var result = channel.Result.Tabs.AddWikiTabBatch(testTabName);
                context.Execute();

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch();
                context.Execute();
            }
        }

        [TestMethod]
        public void AddWikiTabSpecificBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var batch = context.NewBatch();
                var team = context.Team.GetBatch(batch, o => o.Channels);
                context.Execute(batch);
                Assert.IsTrue(team.Result.Channels.Length > 0);

                var firstChannel = team.Result.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(firstChannel);

                var channel = firstChannel.GetBatch(batch, o => o.Tabs);
                context.Execute(batch);

                var testTabName = "WikiTestTab";
                var result = channel.Result.Tabs.AddWikiTabBatch(batch,testTabName);
                context.Execute(batch);

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch(batch);
                context.Execute(batch);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWikiTabExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);
                
                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWikiTab(string.Empty);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWikiTabBatchExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWikiTabBatch(string.Empty);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDocLibraryTabBatchExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddDocumentLibraryTabBatch(string.Empty, new Uri($"{context.Uri.OriginalString}/Shared%20Documents"));
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDocLibraryTabBatchNullUriExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddDocumentLibraryTabBatch("DocLibTab", null);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDocLibraryTabExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddDocumentLibraryTab(string.Empty, new Uri($"{context.Uri.OriginalString}/Shared%20Documents"));
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDocLibraryTabNullUriExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddDocumentLibraryTab("DocLibTab", null);
                Assert.IsNull(result);
            }
        }
    }
}
