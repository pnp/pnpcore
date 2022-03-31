using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.IO;
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

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);
                var tabs = channel.Tabs.AsRequested();
                var firstTab = tabs.First();
                await firstTab.LoadAsync(o => o.WebUrl/*, o => o.SortOrderIndex*/);

                Assert.IsNotNull(firstTab.WebUrl);
                //Assert.IsNotNull(firstTab.SortOrderIndex); //This is not found on the graph V1.0
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
        public async Task AddWebsiteTabAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var testTabName = "PnP GitHub";
                var testSite = $"https://pnp.github.io/";
                var result = await channel.Tabs.AddWebsiteTabAsync(testTabName, new Uri(testSite));

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
            }
        }

        [TestMethod]
        public void AddWebsiteTabBatchTest()
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

                var testTabName = "PnP GitHub";
                var testSite = $"https://pnp.github.io/";
                var result = channel.Result.Tabs.AddWebsiteTabBatch(testTabName, new Uri(testSite));
                context.Execute();

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                result.DeleteBatch();
                context.Execute();
            }
        }

        [TestMethod]
        public void AddWebsiteTabSpecificBatchTest()
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

                var testTabName = "PnP GitHub";
                var testSite = $"https://pnp.github.io/";
                var result = channel.Result.Tabs.AddWebsiteTabBatch(batch, testTabName, new Uri(testSite));
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
        public void AddWebsiteTabNullUriExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWebsiteTab("WebsiteTab", null);
                Assert.IsNull(result);
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWebsiteTabNullNameExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWebsiteTab(null, new Uri("https://pnp.github.io"));
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWebsiteTabBatchExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWebsiteTabBatch(string.Empty, new Uri("https://pnp.github.io"));
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWebsiteTabBatchNullUriExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = channel.Get(o => o.Tabs);

                var result = channel.Tabs.AddWebsiteTabBatch("WebsiteTab", null);
                Assert.IsNull(result);
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
                var result = channel.Result.Tabs.AddWikiTabBatch(batch, testTabName);
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

        [TestMethod]
        public async Task AddWordTabAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var siteAssetsList = await context.Web.Lists.GetByTitleAsync("Site Assets");
                var newFile = await siteAssetsList.RootFolder.Files.AddAsync(WordFileName, new MemoryStream(Convert.FromBase64String(WordBase64Content)), true);

                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var testTabName = "Word file test";
                var result = await channel.Tabs.AddWordTabAsync(testTabName, new Uri($"{context.Uri.AbsoluteUri}/SiteAssets/{WordFileName}"), newFile.UniqueId);

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
                await newFile.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWordTabBatchAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var batch = context.NewBatch();

                var siteAssetsList = await context.Web.Lists.GetByTitleAsync("Site Assets");
                var newFile = await siteAssetsList.RootFolder.Files.AddAsync(WordFileName, new MemoryStream(Convert.FromBase64String(WordBase64Content)), true);

                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var testTabName = "Word file test";
                var result = channel.Tabs.AddWordTabBatch(batch, testTabName, new Uri($"{context.Uri.AbsoluteUri}/SiteAssets/{WordFileName}"), newFile.UniqueId);
                
                context.Execute(batch);

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
                await newFile.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task AddWordTabCurrentBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var siteAssetsList = await context.Web.Lists.GetByTitleAsync("Site Assets");
                var newFile = await siteAssetsList.RootFolder.Files.AddAsync(WordFileName, new MemoryStream(Convert.FromBase64String(WordBase64Content)), true);

                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var testTabName = "Word file test";
                var result = channel.Tabs.AddWordTabBatch(testTabName, new Uri($"{context.Uri.AbsoluteUri}/SiteAssets/{WordFileName}"), newFile.UniqueId);

                context.Execute();

                Assert.IsNotNull(result);
                Assert.AreEqual(testTabName, result.DisplayName);

                //Clean up tab
                await result.DeleteAsync();
                await newFile.DeleteAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddWordTabCurrentBatchNullUriExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var testTabName = "Word file test";
                var result = channel.Tabs.AddWordTabBatch(testTabName, null, Guid.NewGuid());

                context.Execute();

                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddWordTabAsyncNullTitleExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var result = await channel.Tabs.AddWordTabAsync(null, new Uri("https://google.com"), Guid.NewGuid());

                Assert.IsNull(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddWordTabNullFileIdExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");
                Assert.IsNotNull(channel);

                channel = await channel.GetAsync(o => o.Tabs);

                var result = channel.Tabs.AddWordTabBatch("Test tab title", new Uri("https://google.com"), Guid.Empty);

                context.Execute();

                Assert.IsNull(result);
            }
        }

        #region Dummy Word File
        private string WordBase64Content = "UEsDBBQABgAIAAAAIQBncygvlwEAACgJAAATAAgCW0NvbnRlbnRfVHlwZXNdLnhtbCCiBAIooAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADElstqwzAQRfeF/oPRtthKUiilxMmij2UbaPoBijSOTW1JSJPX33ccO6GUJDZNTDYGeWbuuTMylobjdZEHS3A+Mzpm/ajHAtDSqEzPY/Y1fQsfWeBRaCVyoyFmG/BsPLq9GU43FnxA1drHLEW0T5x7mUIhfGQsaIokxhUCaenm3Ar5LebAB73eA5dGI2gMsdRgo+ELJGKRY/C6pteVEwe5Z8FzlViyYiaszTMpkOJ8qdUfSlgTIqrc5vg0s/6OEhg/SCgjxwF13QeNxmUKgolw+C4KyuIr4xRXRi4KqoxOyxzwaZIkk7CvL9WsMxK8p5kXebSPFCLTO/9HfXjc5OAv76LSbcYDIhV0YaBWbrSwgtlnZy5+iTcaSYxBbbCL3dhLN5oArTrysFNutJCCUOD6l3dQCbfkD67GLzerk/4r4Zb8Dvpvya/GdH/l+XfAbz1/4olZDl04qKUbTSCdxFA9z/8StzKnkJQ5ccZ6OtndP9reHd1ldUgNW3CYnf7T7IkkfXZ/UN4KFKgDbL6954x+AAAA//8DAFBLAwQUAAYACAAAACEAHpEat+8AAABOAgAACwAIAl9yZWxzLy5yZWxzIKIEAiigAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAKySwWrDMAxA74P9g9G9UdrBGKNOL2PQ2xjZBwhbSUwT29hq1/79PNjYAl3pYUfL0tOT0HpznEZ14JRd8BqWVQ2KvQnW+V7DW/u8eACVhbylMXjWcOIMm+b2Zv3KI0kpyoOLWRWKzxoGkfiImM3AE+UqRPblpwtpIinP1GMks6OecVXX95h+M6CZMdXWakhbeweqPUW+hh26zhl+CmY/sZczLZCPwt6yXcRU6pO4Mo1qKfUsGmwwLyWckWKsChrwvNHqeqO/p8WJhSwJoQmJL/t8ZlwSWv7niuYZPzbvIVm0X+FvG5xdQfMBAAD//wMAUEsDBBQABgAIAAAAIQBHQ9HZFQMAAPoLAAARAAAAd29yZC9kb2N1bWVudC54bWyklttu2zAMQN8H7B8Cv7eyk9RxjCZFm6xFHwYUbfcBiizbQi1LkJTbvn6Ur9ncBY77IutCHlEUSev27sCz0Y4qzUS+cLxr1xnRnIiI5cnC+fX+eBU4I21wHuFM5HThHKl27pbfv93uw0iQLae5GQEi1+FekoWTGiNDhDRJKcf6mjOihBaxuSaCIxHHjFC0FypCY9dzi55UglCtYb8VzndYOxWOHPrRIoX3oGyBU0RSrAw9tAzvYsgNmqOgCxoPAMEJx14XNbkY5SNrVQc0HQQCqzqkm2GkTw7nDyONu6TZMNKkSwqGkTrhxLsBLiTNYTEWimMDQ5UgjtXHVl4BWGLDNixj5ghM168xmOUfAywCrYbAJ9HFhBniIqLZJKopYuFsVR5W+leNvjU9LPWrT62h+py/VFlXxaE4OVI0A1+IXKdMNhnOh9JgMa0hu3OH2PGslttLr2e6/K88rUtXtsA+5lf+51lp+Xmi5/a4EYtoNPqY8PeetSUcorDdeJBrTpzr9SwgNWDcAfiE9iz4NSOoGIi0GWo5rGdq1JzyViyHtY71etaxf405AejIROlFlHHtV2R1scEp1k2gWyK9zKibBnfkJz6SydcS4UmJrWxp7Gu057as7e0D4wJWlVCnSa6/ZsxbiiVUO07C5yQXCm8ysAjSYwQRPipuwLYQKPZTdOmhmLd3PbI1xlnCy2gjoqP9SlibhhIr/AxB6Y6DH8G9Cy8sOwv/FWNnp4E7Xa2COcyG8AqLXkHQfXB97+GxmVrTGG8zY1fu/cn6flXsomxjlu9Um1tke7ZVRSvtkqbEvKhPsHYxpTii6pXGVMF7D44VmqOEw9IdhbqgQptF6jnyHXROOioNO1GYlQqxEKYHPjgv3cXPz9sTM6VPxT33/AYdea+Ul8nbbxCC+up5c/vnhi2h7weTymKZ/MTWtUbAb8CberPirliSmna4EcYI3o4zGp+slkdYODM3sMPSwmaYbE0xrOwnItMwqyUmtJQppuEF/qRsMIYZy+kLMwSsnPiFEqoDoOiWEYnaR/vyDwAAAP//AwBQSwMEFAAGAAgAAAAhAMWqsptAAQAAPQcAABwACAF3b3JkL19yZWxzL2RvY3VtZW50LnhtbC5yZWxzIKIEASigAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAtJXLboMwEEX3lfoPyPtiIG36UEw2VaVsW/oBDh4eKtjInj74+1pBIaRFVhZmORd57mHuYDbbn7YJvkCbWklG4jAiAchciVqWjLxnLzcPJDDIpeCNksBID4Zs0+urzSs0HO0hU9WdCWwXaRipELsnSk1eQctNqDqQ9kmhdMvRlrqkHc8/eAk0iaI11dMeJD3rGewEI3onrH/Wd3BJb1UUdQ7PKv9sQeKMBS2UQtC2I9clICNDHYe2EaHz/vHKJwDas3DyP5SD6ITwyvAN+zdAtAmbE8lEdIHc+wSpgItpGkOdONNI/K6DxIzvm0kio+Si8Aph/mVxVJyD8IqAfQNTgEPtsl8vvwfur9Lr+89fCysXwJ1Pf5BCWsdJAkfFOYNo+RCcM7j1ncGfIYySC+Jx+UUYbyR69tNLfwEAAP//AwBQSwMEFAAGAAgAAAAhAFMc2vSzAgAAkQsAABIAAAB3b3JkL2Zvb3Rub3Rlcy54bWzUlslu2zAQQO8F+g+C7g612JYjxA7SGClyC5L2AxiKsoiIC0jKsv++pFY3cg1ZOVUHLUPO48xwZsS7+wPNnT2WinC2dv0bz3UwQzwhbLd2f/96mq1cR2nIEphzhtfuESv3fvP9210Zp5xrxjVWjmEwFZcCrd1MaxEDoFCGKVQ3lCDJFU/1DeIU8DQlCIOSywQEnu9Vb0JyhJUyCz5CtofKbXDoMI6WSFgaZQucA5RBqfGhZ/hXQxbgFqyGoGACyHgY+ENUeDVqCaxVA9B8EshYNSAtppHOOLecRgqGpGgaKRySVtNIg3SiwwTnAjMzmHJJoTafcgcolB+FmBmwgJq8k5zoo2F6yxYDCfuYYJHR6gg0TK4mRIDyBOdh0lL42i0kixv9WadvTY9r/ebRasgx/tcqW44KipmuPAcS5yYWnKmMiK7C6VSaGcxayP6SE3uat/NK4Y8sl3+1p20dyh44xvwm/jSvLb9M9L0RO2IRncYYE/5es7WEmizsF54UmpPg+iMbSAsIBoAlwiMbfstYNQyA+gq1HDKyNFpOvSuWQ/rA+iP72GdjTgAq0Ul2FSVo4wqsLtQwg6pLdEvE1xm16HBHehIjsftaIfyUvBA9jXyN9ty3tdKeMK5gNQV1WuTqa8a8ZVCYbkdR/LxjXML33FhkysMxGe5UO2DvJlHso3rFh0pu99qxPcbdnByNnDLWR2EQCgsooebSNSKboDO/miiM8jy2Y89G6Hnbx8B/unUrqfnxaCuNmsuqmnNa8monrhZRNLcTa9EWp7DI9XDkxYoeluH24bFe8EXahxIQGXfNJJhqbNq6ZxVyYjcgmHcfr4X1Hxaau2BzBzr1mtH6VA/JekJ1b/0/GwvEmSasqP4Hb5/j4p0JSxiF8x9R6P8fYTnr3qUQnXyozR8AAAD//wMAUEsDBBQABgAIAAAAIQCeIEdPsgIAAIsLAAARAAAAd29yZC9lbmRub3Rlcy54bWzUlslu2zAQhu8F+g4C7w61eYkQOwjstsgtSNIHYCjaIiIuICkvb19Sqxu5gayc6oMlkZyPMz9nRrq7P7Lc2xOlqeBLENz4wCMci5Ty3RL8fv05WQBPG8RTlAtOluBENLhfff92d0gIT7kwRHsWwXVykHgJMmNkAqHGGWFI3zCKldBia26wYFBstxQTeBAqhaEf+OWdVAITre1+a8T3SIMah4/DaKlCB2vsgDHEGVKGHDtGcDVkCm/hog8KR4BshGHQR0VXo2bQedUDxaNA1qseaTqOdCG42ThS2CfNx5GiPmkxjtRLJ9ZPcCEJt5NboRgy9lHtIEPqvZATC5bI0DeaU3OyTH/WYBDl7yM8slYtgUXp1YQ5ZCIleZQ2FLEEheJJbT9p7Z3rSWVfXxoLNST+ymQjcMEIN2XkUJHcaiG4zqhsK5yNpdnJrIHsPwtiz/Jm3UEGA8vlX+1pU0nZAYe4X+vP8srzz4mBP+BEHKK1GOLC33s2njCbhd3Go6Q5EzcY2EAaQNgDzDAZ2PAbxqJmQNxVqOPQgaXRcKpTcRzaCRsM7GMfnTkD6NSk2VWUsNEVOltkUIZ0m+iOSK5zatriTuxMI7n7WiH8UqKQHY1+jfbYtbWD+8C4glUX1HmR668585Ihabsdw8njjguF3nLrkS0Pz2a4V56A+7eJ4i7lLTmW4+6sPddjwKr7MvIOiTlJS9BEIoWMUMAOufycBOU6aW3jxM092sHoIVoHsR+BctS+d4wbndc/Z2q/0tLnJfD9xXQ+j2/boQ3ZoiI3/ZknN/QwizYP62rDJ+UuWiJso7WL0NYQ29V9Z5BTp38Ytw/PhQsfFUYAuLqDrXnFaGKqplS1oPyvw7+kBBbcUF6UL4OXj6r4F0SZzde3myD+8X+IcjG8TwTq7vXqDwAAAP//AwBQSwMEFAAGAAgAAAAhACvIBadhAgAA5QkAABAAAAB3b3JkL2hlYWRlcjEueG1spJbbjpswEIbvK/UdkO8TAzlsikJW201bRb1ZddsH8BonoOCDbOf09h0TDmlpV0BygYnt+fx7PDN4+XjmuXdk2mRSxCgY+8hjgsokE7sY/fr5dbRAnrFEJCSXgsXowgx6XH38sDxFaaI9sBYmOikao9RaFWFsaMo4MWOeUS2N3NoxlRzL7TajDJ+kTnDoB37xprSkzBhY6pmIIzGoxNFzN1qiyQmMHXCKaUq0ZeeGEfSGzPAnvGiDwgEg2GEYtFGT3qg5dqpaoOkgEKhqkWbDSP/Y3HwYKWyTHoaRJm3SYhipFU68HeBSMQGDW6k5sfBX7zAnen9QIwArYrO3LM/sBZj+vMKQTOwHKAKrmsAnSW/CA+YyYfkkqSgyRgctotJ+VNs76dHVvmwqC91l/1eTtaQHzoQtdo41y8EXUpg0U3WG86E0GEwryPG9TRx5Xs07qaBjuvyvPK2vrmyAXeSX/uf5Vfn7xMDvcCIOUVt0kfDnmpUSDlHYLDzINTfODToWkAoQtgBzyjoW/IqxKBmYNhnqOFnH1Kg411NxnKxxbNCxjv0t5gZgEpukvShh5VfsbIklKTF1oDsi6ydqVuMu/MZHandfInzT8qAaWnYfbdOUtZO7W/RglQl1m+TmPjGvKVFQ7TiNNjshNXnLQRGkhwcR7hUn4J4QKK4pXtm56Hdn7bkag1ZwKVLQN40U0WQDwQgH4fvhl8+o6IXviXW9D+UPeiO4eCU/YKL/NJ+sn57rrjXbkkNub0YK+osumld7yUFPdCR5jL5LZdneWIRXS1zOcW3xhEva6jcAAAD//wMAUEsDBBQABgAIAAAAIQB2aRBYYgIAAOUJAAAQAAAAd29yZC9oZWFkZXIyLnhtbKSW246bMBCG7yv1HZDvEwM5LgpZ7W62VdSbqts+gNc4AQUfZDunt++YcEhLuyIkF5gMzOff45nBi8cTz70D0yaTIkbB0EceE1QmmdjG6NfPL4M58owlIiG5FCxGZ2bQ4/Lzp8UxShPtgbcw0VHRGKXWqghjQ1PGiRnyjGpp5MYOqeRYbjYZZfgodYJDP/CLO6UlZcbAVC9EHIhBJY6eutESTY7g7IBjTFOiLTs1jOBmyAQ/4HkbFPYAwQrDoI0a3YyaYqeqBRr3AoGqFmnSj/SPxU37kcI2adaPNGqT5v1IrXTi7QSXigl4uJGaEwt/9RZzond7NQCwIjZ7z/LMnoHpTysMycSuhyLwqgl8lNxMmGEuE5aPkooiY7TXIir9B7W/kx5d/Muh8tBd1n9xWUm650zYYuVYsxxiIYVJM1VXOO9Lg4dpBTl8tIgDz6v3jiroWC7/a0+rSygbYBf5Zfx5flH+MTHwO+yIQ9QeXST8OWelhEMWNhP3Cs1VcIOODaQChC3AlLKODb9izEsGpk2FOk7WsTQqzmVXHCdrAht07GN/i7kCmMQm6U2UsIordr7EkpSYOtEdkd0malLjzvwqRmp7XyF81XKvGlp2H23dtLWjO1vcwCoL6rrIzX1i3lKioNtxGq23QmrynoMiKA8PMtwrdsBdIVHcUNyyU2F3e+25HoOWcChSYBtHimiyhmQMX4PXh+fnCSqs8D2xzjorf2CN4OCV/IiR7z9NR6unl9q0Yhuyz+3Vk4L+XRfDmz3noCc6kDxG36SybGcswssFLt9xY3GFQ9ryNwAAAP//AwBQSwMEFAAGAAgAAAAhANgQmIJgAgAA5gkAABAAAAB3b3JkL2Zvb3RlcjEueG1spJZLj5swEMfvlfodkO+JIU+KQlZRola5Vd1t717jBBT8kO28vn3HhEda2hUhOWAyMD//PZ4ZvHi58Nw7MW0yKWIUDH3kMUFlkol9jH6+fR2EyDOWiITkUrAYXZlBL8vPnxbnaGe1B97CRGdFY5RaqyKMDU0ZJ2bIM6qlkTs7pJJjudtllOGz1Ake+YFf3CktKTMGploTcSIGlTh66UZLNDmDswNOME2JtuzSMIKHIVP8BYdt0KgHCFY4Ctqo8cOoGXaqWqBJLxCoapGm/Uj/WNysH2nUJs37kcZtUtiP1Eon3k5wqZiAhzupObHwV+8xJ/pwVAMAK2Kz9yzP7BWY/qzCkEwceigCr5rAx8nDhDnmMmH5OKkoMkZHLaLSf1D7O+nRzb8cKg/dZf03l42kR86ELVaONcshFlKYNFN1hfO+NHiYVpDTR4s48bx676yCjuXyv/a0uYWyAXaRX8af5zflHxMDv8OOOETt0UXCn3NWSjhkYTNxr9DcBTfo2EAqwKgFmFHWseFXjLBkYNpUqONkHUuj4tx2xXGyJrBBxz72t5g7gElskj5EGVVxxc6XWJISUye6I7LHRE1r3JXfxUjtnyuEb1oeVUPLnqNtm7Z2dmeLB1hlQd0XuXlOzGtKFHQ7TqPtXkhN3nNQBOXhQYZ7xQ64KySKG4pbdinsbq8912PQEg5FCmyTSBFNtpCM03AVbtabMSqs8D2xzjovf2CN4OCV/IiR769m481qXZs2bEeOub17UtC/62J4tdcc9EQnksfol2TWsoOxCC8XuHzJjcUVTmnL3wAAAP//AwBQSwMEFAAGAAgAAAAhAKUNQwFfAgAA5gkAABAAAAB3b3JkL2Zvb3RlcjIueG1spJbLrtowEIb3lfoOkffgJFwbEY4oqBW7qqft3scxJCK+yDa3t+845EKb9igEFnGwPZ9/j2cmXrxceO6dmDaZFDEKhj7ymKAyycQ+Rj9/fBnMkWcsEQnJpWAxujKDXpYfPyzO0c5qD6yFic6Kxii1VkUYG5oyTsyQZ1RLI3d2SCXHcrfLKMNnqRMc+oFfvCktKTMGlloTcSIGlTh66UZLNDmDsQOOMU2JtuzSMIKHIRP8Cc/boLAHCHYYBm3U6GHUFDtVLdC4FwhUtUiTfqR/bG7ajxS2SbN+pFGbNO9HaoUTbwe4VEzA4E5qTiz81XvMiT4c1QDAitjsLcszewWmP60wJBOHHorAqibwUfIwYYa5TFg+SiqKjNFRi6i0H9T2Tnp0sy+bykJ32f/NZCPpkTNhi51jzXLwhRQmzVSd4bwvDQbTCnJ6bxMnnlfzziromC7/K0+bmysbYBf5pf95flP+PjHwO5yIQ9QWXST8uWalhEMUNgv3cs2dc4OOBaQChC3AlLKOBb9izEsGpk2GOk7WMTUqzu1UHCdrHBt0rGN/i7kDmMQm6UOUsPIrdrbEkpSYOtAdkT0malLjrvzOR2r/XCJ81fKoGlr2HG3blLWzu1s8wCoT6j7JzXNiXlOioNpxGm33QmryloMiSA8PItwrTsA9IVBcU7yyS9HvztpzNQYt4VKkoG8cKaLJFoLR90fB59V6hope+J5Y1zsrf9AbwcUr+e4mrqajzWpdd23YjhxzezdS0L/ponm11xz0RCeSx+iXZNayg7EILxe4nOTa4gm3tOVvAAAA//8DAFBLAwQUAAYACAAAACEA072SyGICAADlCQAAEAAAAHdvcmQvaGVhZGVyMy54bWykltuOmzAQhu8r9R2Q7xNDTpugkNVqk1ZRb6pu+wBe4wQUfJDtnN6+Y8IhLe0KSC4wGZjPv8czg5fPF555J6ZNKkWEgqGPPCaojFOxj9Cvn18Gc+QZS0RMMilYhK7MoOfV50/Lc5jE2gNvYcKzohFKrFUhxoYmjBMz5CnV0sidHVLJsdztUsrwWeoYj/zAz++UlpQZA1O9EnEiBhU4emlHizU5g7MDTjBNiLbsUjOCzpApXuB5EzTqAYIVjoImatwZNcNOVQM06QUCVQ3StB/pH4ub9SONmqSnfqRxkzTvR2qkE28muFRMwMOd1JxY+Kv3mBN9OKoBgBWx6XuapfYKTH9WYkgqDj0UgVdF4OO4M+EJcxmzbByXFBmhoxZh4T+o/J308OZfDKWHbrP+m8ta0iNnwuYrx5plEAspTJKqqsJ5Xxo8TErI6aNFnHhWvndWQcty+V97Wt9CWQPbyC/iz7Ob8o+Jgd9iRxyi8mgj4c85SyUcsrCeuFdo7oIbtGwgJWDUAMwoa9nwS8a8YGBaV6jjpC1Lo+TcdsVx0jqwQcs+9reYO4CJbZx0oozKuGLnSyxJiKkS3RFZN1HTCnfldzFS+8cK4auWR1XT0sdo27qtnd3ZogOrKKj7IjePiXlLiIJux2m43QupyXsGiqA8PMhwL98Bd4VEcUN+yy653e2153oMWsGhSIFtEiqiyRaScbZYbKbTzQblVvieWGd9Kn5gDeHgFf+IkO+/zMbrl9fKtGY7cszs3ZOc/l3nw5u9ZqAnPJEsQt+ksuxgLMKrJS7ecWN+hUPa6jcAAAD//wMAUEsDBBQABgAIAAAAIQBU0CWrXwIAAOYJAAAQAAAAd29yZC9mb290ZXIzLnhtbKSWS4+bMBDH75X6HZDviYE8i0JW241a5VZ12969xgko+CHbeX37jgmPtLQrIDlgMjA//z2eGbx6uvDcOzFtMiliFIx95DFBZZKJfYx+/vgyWiLPWCISkkvBYnRlBj2tP35YnaOd1R54CxOdFY1Raq2KMDY0ZZyYMc+olkbu7JhKjuVul1GGz1InOPQDv7hTWlJmDEz1QsSJGFTi6KUbLdHkDM4OOMU0JdqyS8MIekNm+BNetkHhABCsMAzaqElv1Bw7VS3QdBAIVLVIs2GkfyxuPowUtkmLYaRJm7QcRmqlE28nuFRMwMOd1JxY+Kv3mBN9OKoRgBWx2VuWZ/YKTH9eYUgmDgMUgVdN4JOkN2GBuUxYPkkqiozRUYuo9B/V/k56dPMvh8pDd1n/zWUj6ZEzYYuVY81yiIUUJs1UXeF8KA0ephXk9N4iTjyv3juroGO5/K89bW6hbIBd5Jfx5/lN+fvEwO+wIw5Re3SR8OeclRIOWdhMPCg0d8ENOjaQChC2AHPKOjb8irEsGZg2Feo4WcfSqDi3XXGcrAls0LGP/S3mDmASm6S9KGEVV+x8iSUpMXWiOyLrJ2pW4678LkZq/1ghfNXyqBpa9hht27S1sztb9GCVBXVf5OYxMa8pUdDtOI22eyE1ectBEZSHBxnuFTvgrpAobihu2aWwu732XI9BazgUKbBNI0U02UIyLsJJOAs+T1Fhhe+JLazlD6wRHLyS7zHy/ef5ZPP8Ups2bEeOub17UtC/6WJ4tdcc9EQnksfol2TWsoOxCK9XuHzJjcUVTmnr3wAAAP//AwBQSwMEFAAGAAgAAAAhALb0Z5jSBgAAySAAABUAAAB3b3JkL3RoZW1lL3RoZW1lMS54bWzsWUuLG0cQvgfyH4a5y3rN6GGsNdJI8mvXNt61g4+9UmumrZ5p0d3atTCGYJ9yCQSckEMMueUQQgwxxOSSH2OwSZwfkeoeSTMt9cSPXYMJu4JVP76q/rqquro0c+Hi/Zg6R5gLwpKOWz1XcR2cjNiYJGHHvX0wLLVcR0iUjBFlCe64Cyzcizuff3YBnZcRjrED8ok4jzpuJOXsfLksRjCMxDk2wwnMTRiPkYQuD8tjjo5Bb0zLtUqlUY4RSVwnQTGovTGZkBF2DpRKd2elfEDhXyKFGhhRvq9UY0NCY8fTqvoSCxFQ7hwh2nFhnTE7PsD3petQJCRMdNyK/nPLOxfKayEqC2RzckP9t5RbCoynNS3Hw8O1oOf5XqO71q8BVG7jBs1BY9BY69MANBrBTlMups5mLfCW2BwobVp095v9etXA5/TXt/BdX30MvAalTW8LPxwGmQ1zoLTpb+H9XrvXN/VrUNpsbOGblW7faxp4DYooSaZb6IrfqAer3a4hE0YvW+Ft3xs2a0t4hirnoiuVT2RRrMXoHuNDAGjnIkkSRy5meIJGgAsQJYecOLskjCDwZihhAoYrtcqwUof/6uPplvYoOo9RTjodGomtIcXHESNOZrLjXgWtbg7y6sWLl4+ev3z0+8vHj18++nW59rbcZZSEebk3P33zz9Mvnb9/+/HNk2/teJHHv/7lq9d//Plf6qVB67tnr58/e/X913/9/MQC73J0mIcfkBgL5zo+dm6xGDZoWQAf8veTOIgQyUt0k1CgBCkZC3ogIwN9fYEosuB62LTjHQ7pwga8NL9nEN6P+FwSC/BaFBvAPcZoj3Hrnq6ptfJWmCehfXE+z+NuIXRkWzvY8PJgPoO4JzaVQYQNmjcpuByFOMHSUXNsirFF7C4hhl33yIgzwSbSuUucHiJWkxyQQyOaMqHLJAa/LGwEwd+GbfbuOD1Gber7+MhEwtlA1KYSU8OMl9BcotjKGMU0j9xFMrKR3F/wkWFwIcHTIabMGYyxEDaZG3xh0L0Gacbu9j26iE0kl2RqQ+4ixvLIPpsGEYpnVs4kifLYK2IKIYqcm0xaSTDzhKg++AElhe6+Q7Dh7ref7duQhuwBombm3HYkMDPP44JOELYp7/LYSLFdTqzR0ZuHRmjvYkzRMRpj7Ny+YsOzmWHzjPTVCLLKZWyzzVVkxqrqJ1hAraSKG4tjiTBCdh+HrIDP3mIj8SxQEiNepPn61AyZAVx1sTVe6WhqpFLC1aG1k7ghYmN/hVpvRsgIK9UX9nhdcMN/73LGQObeB8jg95aBxP7OtjlA1FggC5gDBFWGLd2CiOH+TEQdJy02t8pNzEObuaG8UfTEJHlrBbRR+/gfr/aBCuPVD08t2NOpd+zAk1Q6Rclks74pwm1WNQHjY/LpFzV9NE9uYrhHLNCzmuaspvnf1zRF5/mskjmrZM4qGbvIR6hksuJFPwJaPejRWuLCpz4TQum+XFC8K3TZI+Dsj4cwqDtaaP2QaRZBc7mcgQs50m2HM/kFkdF+hGawTFWvEIql6lA4MyagcNLDVt1qgs7jPTZOR6vV1XNNEEAyG4fCazUOZZpMRxvN7AHeWr3uhfpB64qAkn0fErnFTBJ1C4nmavAtJPTOToVF28KipdQXstBfS6/A5eQg9Ujc91JGEG4Q0mPlp1R+5d1T93SRMc1t1yzbayuup+Npg0Qu3EwSuTCM4PLYHD5lX7czlxr0lCm2aTRbH8PXKols5AaamD3nGM5c3Qc1IzTruBP4yQTNeAb6hMpUiIZJxx3JpaE/JLPMuJB9JKIUpqfS/cdEYu5QEkOs591Ak4xbtdZUe/xEybUrn57l9FfeyXgywSNZMJJ1YS5VYp09IVh12BxI70fjY+eQzvktBIbym1VlwDERcm3NMeG54M6suJGulkfReN+SHVFEZxFa3ij5ZJ7CdXtNJ7cPzXRzV2Z/uZnDUDnpxLfu24XURC5pFlwg6ta054+Pd8nnWGV532CVpu7NXNde5bqiW+LkF0KOWraYQU0xtlDLRk1qp1gQ5JZbh2bRHXHat8Fm1KoLYlVX6t7Wi212eA8ivw/V6pxKoanCrxaOgtUryTQT6NFVdrkvnTknHfdBxe96Qc0PSpWWPyh5da9Savndeqnr+/XqwK9W+r3aQzCKjOKqn649hB/7dLF8b6/Ht97dx6tS+9yIxWWm6+CyFtbv7qu14nf3DgHLPGjUhu16u9cotevdYcnr91qldtDolfqNoNkf9gO/1R4+dJ0jDfa69cBrDFqlRjUISl6joui32qWmV6t1vWa3NfC6D5e2hp2vvlfm1bx2/gUAAP//AwBQSwMEFAAGAAgAAAAhAJ2cWv08BAAAZwwAABEAAAB3b3JkL3NldHRpbmdzLnhtbLRX227jNhB9L9B/MPRcx5Ijy15hnYXsrJss4m6xdlGgb5RI2UR4EUjKjrfov3dIiZadpIskRV5iai5nRsMzM8rHTw+c9XZEaSrFNIguwqBHRCExFZtp8Md60Z8EPW2QwIhJQabBgejg09XPP33cp5oYA2a6BxBCp7yYBltjqnQw0MWWcKQvZEUEKEupODLwqDYDjtR9XfULyStkaE4ZNYfBMAyToIWR06BWIm0h+pwWSmpZGuuSyrKkBWl/vId6SdzG5VoWNSfCuIgDRRjkIIXe0kp7NP5WNFBuPcjuRy+x48zb7aPwBa+7lwofPV6SnnWolCyI1nBBnPkEqegCx0+AjrEvIHb7ig4K3KPQnU4zH70OYPgEICnIw+swJi3GADxPcSh+HU5yxKFdYaPkbcmcAGhs8PZVKENf14H1RQZtkT6yyCKS1yU1OsIdeFcjzV7CmkZ1R3OFVNOTLWV4kd5uhFQoZ5AOUKcHt99z2dm/UET7447kwcltHYIrmBHfpeS9fVoRVUCjwIAJw2BgFZiUqGZmjfKVkRWY7BAkOQ4njXp7qLZEuO78C+aO18fDUaMvtkihwhC1qlABHJ9LYZRk3g7L36SZw4xR0AKNRymlEdKQ39XpEzhY8vSjc6NW7HIdPPYlAj95eIRzLvUwZ47NBOxOq2aagotAHMp8NiGXEsO426e1oi/ng3Vw1Yh80Z4NJGH6K4rJ2l7vyhwYWUAxV/Q7yQT+UmtDAdHdxP/I4EcJwD1D5K9AyPWhIguCTA3X9k7BHDMWjFZLqpRUtwIDL98tGC1LoiAARYYsge5Uyb2r8w1BGJbuO8WtNfkTjGEeXK6hTe5n0hjJb7qeentcz+WOvvDpgLU/fINOOZqGWXJ5nc2bTK2200xG43H84TnNf/vMwiSaLdr4bVSe2rVrO6o5Wer2eOMxRzxXFPWWdjEPrEWu7mdUeH1OYPqRU82qzr2y328UmiPGFlBEr3AF4CmmurompTuzJVKbDre1UM9KYe59OWLZmUjUr0rWVaPdK1Q1lPQmURy3nlSYO8q9XNf5ynsJmNcnqlrgrzvl6tSVZ58auGLX2nfIUcXZCtaffW6pxNTK0oAsUVU1bMo30TRgdLM1kSWAgScM32/uId8MW93Q6YaNzj2gwr4ZWLeHTjb0shO7Sy+77GSxl8WdbORlo06WeFliZbA1iGJU3AOx/dHKS8mY3BN80+mfiJoi6C2qyHWzm4BeshG0y0r3dil5gC1GMDXwWVxRzNGDXWrDxLq31gwdZG3ObK3OGlfnCHbht608OHN2FH+Ui92ZBQU6rg4871bdL03ijGoYAxVsRSPVuS6KUyyLW7uk45aL8SSbR+Nxox65bWrcpIB7/0bKGdIEtzrvOmpc/46zcJaNk6SfZfO4H2eLeX+WJWF/Now+jD/Hi1k2Gf/TNqn/D+HqXwAAAP//AwBQSwMEFAAGAAgAAAAhAI+JEztQDAAA5XcAAA8AAAB3b3JkL3N0eWxlcy54bWzsnU1z2zgShu9btf+BpdPuwZE/5SQ1zpTjJGPXxIknciZniIQsjEFCS1KxPb9+AZCUIDdBscFen/aSWCL7AYi33yZAUdQvvz6mMvrJ80Ko7Gx08Gp/FPEsVonI7s5G328/7b0eRUXJsoRJlfGz0RMvRr++++c/fnl4W5RPkheRBmTF2zQ+Gy3Kcvl2PC7iBU9Z8UoteaY3zlWeslK/zO/GKcvvV8u9WKVLVoqZkKJ8Gh/u709GNSbvQ1HzuYj5BxWvUp6VNn6cc6mJKisWYlk0tIc+tAeVJ8tcxbwo9EGnsuKlTGRrzMExAKUizlWh5uUrfTB1jyxKhx/s279SuQGc4ACHADCJ+SOO8bpmjHWkyxEJjjNZc0TicMI64wCKpEwWKMphM65jE8tKtmDFwiVyXKdO1rin1IxRGr+9ustUzmZSk7TqkRYusmDzrz5+85/9kz/a980hjN5pLyQq/sDnbCXLwrzMb/L6Zf3K/vdJZWURPbxlRSzEre6gbiUVusHL86wQI72Fs6I8LwRr3bgwf7RuiYvSefu9SMRobFos/tYbfzJ5Njo8bN65MD3Yek+y7K55L5N77z+6PTkb8Wzv+9S8NdPcsxHL96bnJnBcH1j1v3O4y+evbMNLFgvbDpuXXNv8YLJvoFKYqnJ48qZ58W1lBp+tSlU3YgHV/2vsGIy4dr+uBdOqJOmtfP5Zxfc8mZZ6w9nItqXf/H51kwuV67JzNnpj29RvTnkqLkWS8MzZMVuIhP9Y8Ox7wZPN+398sqWjfiNWq0z/fXQ6sVkgi+TjY8yXphDprRkzmnwxAdLsvRKbxm34fxrYQa1EW/yCM1ONo4PnCNt9FOLQRBTO0bYzV8+O3e6FaujopRo6fqmGTl6qoclLNXT6Ug29fqmGLOZ/2ZDIEl347f6wGUDdxfG4Ec3xmA3N8XgJzfFYBc3xOAHN8SQ6muPJYzTHk6YITqliXxY6yX7kyfZu7u5zRBh39ykhjLv7DBDG3V3ww7i763sYd3c5D+Purt5h3N3FGs+tplrRlbZZVg522VypMlMlj0r+OJzGMs2yS1Qanjnp8ZzkIAkwVWWrT8SDaTGzr3dniDVp+Pm8NCu9SM2jubhb5bwY3HGe/eRSLXnEkkTzCIE5L1e5Z0RCcjrnc57zLOaUiU0HNSvBKFulM4LcXLI7MhbPEuLha4gkRWGd0Hr9vDAmEQRJnbI4V8O7phhZffgsiuFjZSDR+5WUnIj1hSbFLGv42sBihi8NLGb4ysBihi8MHM2ohqimEY1UTSMasJpGNG5VflKNW00jGreaRjRuNW34uN2KUtoS7846Dvpfu7uQynyoMLgfU3GXMT0BGH66qa+ZRjcsZ3c5Wy4ic1W6HeseM7ad9yp5im4pzmlrEtW83qbIhT5qka2GD+gWjcpcax6RvdY8IoOtecMtdq2nyWaCdkmznpmuZmWraS2pl2mnTK6qCe1wt7FyeIZtDPBJ5AWZDdqxBBn8xUxnjZwUlW/Ty+Ed27CG2+p5VSLtXo0k6KVU8T1NGb58WvJcL8vuB5M+KSnVA0/oiNMyV1WuuZY/tJL0svzHdLlghbBrpS1E/1N9cztCdM2Wgw/oRjKR0ej2cS9lQkZ0M4jL2+vP0a1ammWmGRga4HtVliolY9ZXAv/1g8/+TdPBc70Izp6Ijvac6PKQhV0IgpNMRVIJEUlPM0UmSM6hlvc7f5oplic0tJucV3cAlZyIOGXpspp0EHhL18UHXX8IZkOW9yfLhbkuRGWqWxKYc9mwWM3+4vHwUvdFRSRXhr6uSnv90U51bTQdbvg0YQs3fIpg1dSnB5O/BAe7hRt+sFs4qoO9kKwohPcj1GAe1eE2POrjHb74q3lKqny+knQD2ADJRrABkg2hkqs0KyiP2PIID9jyqI+XMGUsj+CSnOX9louETAwLo1LCwqhksDAqDSyMVIDhd+g4sOG36Tiw4ffqVDCiKYADo8oz0tM/0ac8DowqzyyMKs8sjCrPLIwqz44+RHw+15NgulOMg6TKOQdJd6LJSp4uVc7yJyLkR8nvGMEF0op2k6u5+WqIyqqbuAmQ5hq1JJxsVzgqkX/wGVnXDIuyXwRXRJmUShFdW9uccGzk9r1ru8LsNzkGd+FGspgvlEx47jkmf6xeL0+rr2U8777tRq/Lnp/F3aKMpov11X4XM9nfGdks2LfCdjfYNuaT5vssbWHXPBGrtOko/DLF5Kh/sM3oreDj3cGbmcRW5EnPSNjmZHfkZpa8FXnaMxK2+bpnpPXpVmSXHz6w/L41EU678me9xvMk32lXFq2DW5vtSqR1ZFsKnnZl0ZZVovM4Np8WQHX6ecYf3888/niMi/wUjJ38lN6+8iO6DPaN/xTmzI4pmra99d0ToO7bSXSvyvnHSlXX7bc+cOr/pa4rPXHKCh61co76f3C1VWX849i73PgRveuOH9G7APkRvSqRNxxVkvyU3rXJj+hdpPwIdLWCZwRctYLxuGoF40OqFaSEVKsBswA/ovd0wI9AGxUi0EYdMFPwI1BGBeFBRoUUtFEhAm1UiEAbFU7AcEaF8TijwvgQo0JKiFEhBW1UiEAbFSLQRoUItFEhAm3UwLm9NzzIqJCCNipEoI0KEWij2vniAKPCeJxRYXyIUSElxKiQgjYqRKCNChFoo0IE2qgQgTYqRKCMCsKDjAopaKNCBNqoEIE2avVVw3CjwnicUWF8iFEhJcSokII2KkSgjQoRaKNCBNqoEIE2KkSgjArCg4wKKWijQgTaqBCBNqr9sHCAUWE8zqgwPsSokBJiVEhBGxUi0EaFCLRRIQJtVIhAGxUiUEYF4UFGhRS0USECbVSI6MrP+iNK3232B/irnt479vt/dFV36pv7VW4XddQf1fTKz+r/XYT3St1HrV88PLLrjX4QMZNC2UvUno/VXa69JQL1wefXi+5v+Lj0gQ9dqr8LYT8zBfDjvpHgmspxV8q7kWCRd9yV6W4kmHUed1VfNxKcBo+7iq71ZXNTij4dgeCuMuMEH3jCu6q1Ew6HuKtGO4FwhLsqsxMIB7irHjuBJ5Epzs+jT3qO02R9fykgdKWjQzj1E7rSEmrVlGNojL6i+Ql91fMT+sroJ6D09GLwwvpRaIX9qDCpoc2wUocb1U/ASg0JQVIDTLjUEBUsNUSFSQ0LI1ZqSMBKHV6c/YQgqQEmXGqICpYaosKkhqcyrNSQgJUaErBSDzwhezHhUkNUsNQQFSY1nNxhpYYErNSQgJUaEoKkBphwqSEqWGqICpMarJLRUkMCVmpIwEoNCUFSA0y41BAVLDVEdUltr6JsSY1S2AnHTcKcQNwJ2QnEFWcnMGC15EQHrpYcQuBqCWrVaI5bLbmi+Ql91fMT+sroJ6D09GLwwvpRaIX9qDCpcaulNqnDjeonYKXGrZa8UuNWS51S41ZLnVLjVkt+qXGrpTapcaulNqnDi7OfECQ1brXUKTVutdQpNW615Jcat1pqkxq3WmqTGrdaapN64AnZiwmXGrda6pQat1ryS41bLbVJjVsttUmNWy21SY1bLXmlxq2WOqXGrZY6pcatlvxS41ZLbVLjVkttUuNWS21S41ZLXqlxq6VOqXGrpU6pcaulax0iCB4BNU1ZXkZ0z4u7ZMWiZMMfTvg9y3mh5E+eRLSH+hl1lOOHrZ+/Mmz723x6/1KPmXkCuvN1paR6AmwNtDteJeYZeuZH/swjtky86UxU/yZY/QNWts/1J7ZVozYWthYvdHNx/fiqXa0x8ywjtid5qQNMPGje88Ra251NJjZ712O7Gbhqv61h6+x9aTK/T8/1jlx6Bqvyj6+Pb+qCsKuTukszWf18mv7jKks04KH+6bCqs8kjq1B6+wWX8ppVe6ulf1fJ52W19WDfPr7g2fZZ9SQ+b3xuS7YXMN7uTPWy/gk3z5BXz+av7yXwDPtvnGdS/FWULSNu720ZOtj+7m25Z92h39Wy5Pct/al/mKMaS6bhX4277aaNxyoltMubTQ3uQptn16G0JEpeCJMddrf9/fPJ0Yfzi2rn+vf3dLbawqD/b/YzBb0y6VIV+jR1clSfbp19rNjrXd7sVzcKGVFrHvhdP/dX/Y7XL7y/6tenhsSrQmekLW7P02Jr2J4r0WyMNoPqEwSWIa9Cu9TxSYFNsD+V6UxbhtW/2YLMsDXv/ymGSrHtcXuuxXorTZJtRB+cZc1fxbv/AgAA//8DAFBLAwQUAAYACAAAACEA7wopTk4BAAB+AwAAFAAAAHdvcmQvd2ViU2V0dGluZ3MueG1snNNfa8IwEADw98G+Q8m7psoUKVZhDMdexmDbB4jp1YYluZKLq+7T79qpc/hi95L/9+MuIfPlztnkEwIZ9LkYDVORgNdYGL/JxfvbajATCUXlC2XRQy72QGK5uL2ZN1kD61eIkU9SwoqnzOlcVDHWmZSkK3CKhliD580Sg1ORp2EjnQof23qg0dUqmrWxJu7lOE2n4sCEaxQsS6PhAfXWgY9dvAxgWURPlanpqDXXaA2Gog6ogYjrcfbHc8r4EzO6u4Cc0QEJyzjkYg4ZdRSHj9Ju5OwvMOkHjC+AqYZdP2N2MCRHnjum6OdMT44pzpz/JXMGUBGLqpcyPt6rbGNVVJWi6lyEfklNTtzetXfkdPa08RjU2rLEr57wwyUd3LZcf9t1Q9h1620JYsEfAutonPmCFYb7gA1BkO2yshabl+dHnsg/v2bxDQAA//8DAFBLAwQUAAYACAAAACEAvy/Xf+8BAAB6BgAAEgAAAHdvcmQvZm9udFRhYmxlLnhtbNyTwY6bMBCG75X6Dsj3DYaEbIqWrNR2I1Wqeqi2D+AYA9ZiG3mckLx9x4awkaKVlh56WA7G/sfzeebHPDyeVBsdhQVpdEGSBSWR0NyUUtcF+fO8u9uQCBzTJWuNFgU5CyCP28+fHvq8MtpBhPkacsUL0jjX5XEMvBGKwcJ0QmOwMlYxh0tbx4rZl0N3x43qmJN72Up3jlNK12TE2PdQTFVJLr4bflBCu5AfW9Ei0WhoZAcXWv8eWm9s2VnDBQD2rNqBp5jUEyZZ3YCU5NaAqdwCmxkrCihMT2iYqfYVkM0DpDeANReneYzNyIgx85ojy3mc9cSR5RXn34q5AkDpymYWJb34Gvtc5ljDoLkminlFZRPurLxHiuc/am0s27dIwq8e4YeLAtiP2L9/hak4Bd23QLbjrxD1uWYKM7+xVu6tDIGOaQMiwdiRtQXBHnY0o76XlK7o0o8k9ht5wywIDxk20kGumJLt+aJCLwGGQCcdby76kVnpqx5CIGsMHGBPC/K0ojR92u3IoCRYHUVldf91VFJ/Vni+jMpyUqhXeOCEZTJweOBMe/DMeHDgxolnqQREv0Qf/TaK6TccSekancjQD+/McpYjNnBnOeL7v3HkfpP9F0fGuxH9lHXj3rwh/l580BsyTmD7FwAA//8DAFBLAwQUAAYACAAAACEAouGx8nEBAAD3AgAAEQAIAWRvY1Byb3BzL2NvcmUueG1sIKIEASigAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAnJJNT8MwDIbvSPyHKvc2/UAIqq6TAO3EJCTGh7hlibdla9Mo8Vb670nbraNiJ252/Pi18ybZ9LssvAMYKys1IVEQEg8Ur4RU6wl5W8z8O+JZZEqwolIwIQ1YMs2vrzKuU14ZeDGVBoMSrOeUlE25npANok4ptXwDJbOBI5QrripTMnSpWVPN+I6tgcZheEtLQCYYMtoK+npQJEdJwQdJvTdFJyA4hQJKUGhpFET0zCKY0l5s6Cq/yFJio+EieioO9LeVA1jXdVAnHer2j+jn/Pm1u6ovVesVB5JngqcosYA8o+fQRXa/3ALH/nhIXMwNMKxMPme4kVvrvYNZAvBdB56Kre07aOrKCOskRpnDBFhupEb3mP2A0YGjC2Zx7l53JUE8NBdm/WXaNgMH2f6QPOqIIc2Odvf7gfCcTWlv6qnykTw+LWYkj8M49sPET6JFeJ/eJGkYfrUrjvrPguVxgX8rngR6l8ZfNf8BAAD//wMAUEsDBBQABgAIAAAAIQCzZ6uKbQEAAMUCAAAQAAgBZG9jUHJvcHMvYXBwLnhtbCCiBAEooAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJxSy07DMBC8I/EPUe6tU4QqhLauUCvEgUelBnq27E1i4diW7Vbt37NpIAS4kdPu7O5oZmJYHluTHTBE7ewin02LPEMrndK2XuSv5f3kJs9iElYJ4ywu8hPGfMkvL2ATnMeQNMaMKGxc5E1K/paxKBtsRZzS2NKkcqEVidpQM1dVWuLayX2LNrGropgzPCa0CtXED4R5z3h7SP8lVU52+uJbefLEx6HE1huRkD93l2aqXGqBDSiULglT6hZ5QfDQwEbUGPkMWF/AzgUVu52+gFUjgpCJ8uPXwEYd3HlvtBSJcuVPWgYXXZWyl7PYrLsGNl4BMrBFuQ86nTr+cQuP2vYq+oJUBVEH4ZtPaUMHWykMrsg6r4SJCOwbgJVrvbBEx4aK+N7jqy/dukvh8+QnOLK406nZeiHxl9kRDltCUZH6QcAAwAP9jGA6drq1Naqvnb+DLr63/lXy2Xxa0HfO6wsj18Nz4R8AAAD//wMAUEsBAi0AFAAGAAgAAAAhAGdzKC+XAQAAKAkAABMAAAAAAAAAAAAAAAAAAAAAAFtDb250ZW50X1R5cGVzXS54bWxQSwECLQAUAAYACAAAACEAHpEat+8AAABOAgAACwAAAAAAAAAAAAAAAADQAwAAX3JlbHMvLnJlbHNQSwECLQAUAAYACAAAACEAR0PR2RUDAAD6CwAAEQAAAAAAAAAAAAAAAADwBgAAd29yZC9kb2N1bWVudC54bWxQSwECLQAUAAYACAAAACEAxaqym0ABAAA9BwAAHAAAAAAAAAAAAAAAAAA0CgAAd29yZC9fcmVscy9kb2N1bWVudC54bWwucmVsc1BLAQItABQABgAIAAAAIQBTHNr0swIAAJELAAASAAAAAAAAAAAAAAAAALYMAAB3b3JkL2Zvb3Rub3Rlcy54bWxQSwECLQAUAAYACAAAACEAniBHT7ICAACLCwAAEQAAAAAAAAAAAAAAAACZDwAAd29yZC9lbmRub3Rlcy54bWxQSwECLQAUAAYACAAAACEAK8gFp2ECAADlCQAAEAAAAAAAAAAAAAAAAAB6EgAAd29yZC9oZWFkZXIxLnhtbFBLAQItABQABgAIAAAAIQB2aRBYYgIAAOUJAAAQAAAAAAAAAAAAAAAAAAkVAAB3b3JkL2hlYWRlcjIueG1sUEsBAi0AFAAGAAgAAAAhANgQmIJgAgAA5gkAABAAAAAAAAAAAAAAAAAAmRcAAHdvcmQvZm9vdGVyMS54bWxQSwECLQAUAAYACAAAACEApQ1DAV8CAADmCQAAEAAAAAAAAAAAAAAAAAAnGgAAd29yZC9mb290ZXIyLnhtbFBLAQItABQABgAIAAAAIQDTvZLIYgIAAOUJAAAQAAAAAAAAAAAAAAAAALQcAAB3b3JkL2hlYWRlcjMueG1sUEsBAi0AFAAGAAgAAAAhAFTQJatfAgAA5gkAABAAAAAAAAAAAAAAAAAARB8AAHdvcmQvZm9vdGVyMy54bWxQSwECLQAUAAYACAAAACEAtvRnmNIGAADJIAAAFQAAAAAAAAAAAAAAAADRIQAAd29yZC90aGVtZS90aGVtZTEueG1sUEsBAi0AFAAGAAgAAAAhAJ2cWv08BAAAZwwAABEAAAAAAAAAAAAAAAAA1igAAHdvcmQvc2V0dGluZ3MueG1sUEsBAi0AFAAGAAgAAAAhAI+JEztQDAAA5XcAAA8AAAAAAAAAAAAAAAAAQS0AAHdvcmQvc3R5bGVzLnhtbFBLAQItABQABgAIAAAAIQDvCilOTgEAAH4DAAAUAAAAAAAAAAAAAAAAAL45AAB3b3JkL3dlYlNldHRpbmdzLnhtbFBLAQItABQABgAIAAAAIQC/L9d/7wEAAHoGAAASAAAAAAAAAAAAAAAAAD47AAB3b3JkL2ZvbnRUYWJsZS54bWxQSwECLQAUAAYACAAAACEAouGx8nEBAAD3AgAAEQAAAAAAAAAAAAAAAABdPQAAZG9jUHJvcHMvY29yZS54bWxQSwECLQAUAAYACAAAACEAs2erim0BAADFAgAAEAAAAAAAAAAAAAAAAAAFQAAAZG9jUHJvcHMvYXBwLnhtbFBLBQYAAAAAEwATALQEAACoQgAAAAA=";
        private string WordFileName = Guid.NewGuid().ToString() + ".docx";
        #endregion
    }
}
