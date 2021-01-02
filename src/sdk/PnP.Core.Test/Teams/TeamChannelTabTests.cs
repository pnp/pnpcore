using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                var tabs = channel.Tabs;
                var firstTab = tabs.First();
                await firstTab.GetAsync(o => o.WebUrl, o => o.SortOrderIndex);

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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                var tabs = channel.Tabs;
                var firstTab = tabs.First();
                await firstTab.GetAsync(o => o.Configuration);
                var config = firstTab.Configuration;

                Assert.IsTrue(config.IsPropertyAvailable(o => o.WebsiteUrl));
                Assert.IsTrue(config.IsPropertyAvailable(o => o.RemoveUrl));
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);
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
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                context.Execute();
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(o => o.Tabs);
                context.Execute();

                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = channel.Tabs.AddDocumentLibraryTabBatch(testTabName, new Uri(testSiteLib));

                context.Execute();

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch();
                context.Execute();
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                channel = channel.GetBatch(batch, o => o.Tabs);

                context.Execute(batch);
                Assert.IsTrue(team.Channels.Length > 0);
                Assert.IsNotNull(channel);

                var testSiteLib = $"{context.Uri.OriginalString}/Shared%20Documents";
                var testTabName = "DocLibTab";
                var result = channel.Tabs.AddDocumentLibraryTabBatch(batch, testTabName, new Uri(testSiteLib));

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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(o => o.Tabs);
                context.Execute();

                var testTabName = "WikiTestTab";
                var result = channel.Tabs.AddWikiTabBatch(testTabName);
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
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.GetBatch(batch, o => o.Tabs);
                context.Execute(batch);

                var testTabName = "WikiTestTab";
                var result = channel.Tabs.AddWikiTabBatch(batch,testTabName);
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
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

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddDocumentLibraryTab("DocLibTab", null);
                Assert.IsNull(result);
            }
        }
    }
}
