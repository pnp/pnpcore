using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using System.Threading.Tasks;
using System;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on PnPContext specifics
    /// </summary>
    [TestClass]
    public class PnPContextTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task PropertiesInitialization()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsNotNull(context.Web);
                Assert.IsNotNull(context.Site);
                Assert.IsNotNull(context.Team);
                Assert.IsNotNull(context.Uri);
                Assert.IsNotNull(context.RestClient);
                Assert.IsNotNull(context.GraphClient);
                Assert.IsNotNull(context.Logger);
                Assert.IsNotNull(context.BatchClient);
                Assert.IsNotNull(context.CurrentBatch);
            }
        }

        [TestMethod]
        public async Task PendingRequests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.HasPendingRequests);

                await context.Web.GetBatchAsync(p => p.Title);

                Assert.IsTrue(context.HasPendingRequests);

                await context.ExecuteAsync();

                Assert.IsFalse(context.HasPendingRequests);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForRootContextViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.GetAsync();

                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id == context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForRootContextViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync();

                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id == context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForSubSiteContextViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                context.GraphFirst = false;

                await context.Web.GetAsync();

                // Since we've not loaded the actual web that's the rootweb, this means that the site rootweb property is not yet loaded
                Assert.IsFalse(context.Site.IsPropertyAvailable(p => p.RootWeb));

                // Load site rootweb and check again
                await context.Site.GetAsync(p => p.RootWeb);
                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id != context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForSubSiteContextViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.GetAsync();

                // Since we've not loaded the actual web that's the rootweb, this means that the site rootweb property is not yet loaded
                Assert.IsFalse(context.Site.IsPropertyAvailable(p => p.RootWeb));

                // Load site rootweb and check again
                await context.Site.GetAsync(p => p.RootWeb);
                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id != context.Web.Id);
            }
        }

        [TestMethod]
        public async Task SkipLoadingTeamForNoGroupSitesViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var team = await context.Team.GetAsync();
                
                // Requested stays false as there's no group connected to this site, so also no team
                Assert.IsFalse(context.Team.Requested);
            }
        }

        [TestMethod]
        public async Task CreateContextFromGroupId()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.EnsurePropertiesAsync(p => p.GroupId);
                // Group id should be loaded
                Assert.IsTrue(context.Site.GroupId != Guid.Empty);

                // Create a new context using this group id
                using (var context2 = await TestCommon.Instance.GetContextAsync(context.Site.GroupId, 1))
                {
                    Assert.IsTrue(context2.Group.Requested == true);
                    Assert.IsTrue(context2.Group.IsPropertyAvailable(p => p.WebUrl) == true);
                    Assert.IsTrue(context2.Uri != null);

                    // Try to get SharePoint and Teams information from a context created via a group id
                    var web = await context2.Web.GetAsync(p => p.Title);

                    Assert.IsTrue(web.Requested);
                    Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));

                    var site = await context2.Site.GetAsync(p => p.GroupId);

                    Assert.IsTrue(site.Requested);
                    Assert.IsTrue(site.IsPropertyAvailable(p => p.GroupId));
                    Assert.IsTrue(site.GroupId == context.Site.GroupId);

                    var team = await context2.Team.GetAsync(p=>p.DisplayName);

                    Assert.IsTrue(team.Requested);
                    Assert.IsTrue(team.IsPropertyAvailable(p => p.DisplayName));
                }
            }
        }

        [TestMethod]
        public async Task ContextCloningForSameSite()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(p => p.Title);

                using (var clonedContext = context.Clone())
                {
                    Assert.AreEqual(context.Uri, clonedContext.Uri);
                    Assert.AreEqual(context.AuthenticationProvider, clonedContext.AuthenticationProvider);

                    Assert.AreEqual(context.GraphAlwaysUseBeta, clonedContext.GraphAlwaysUseBeta);
                    Assert.AreEqual(context.GraphCanUseBeta, clonedContext.GraphCanUseBeta);
                    Assert.AreEqual(context.GraphFirst, clonedContext.GraphFirst);

                    Assert.AreEqual(context.RestClient, clonedContext.RestClient);
                    Assert.AreEqual(context.GraphClient, clonedContext.GraphClient);
                    Assert.AreEqual(context.Logger, clonedContext.Logger);

                    Assert.AreNotEqual(context.Id, clonedContext.Id);
                }

                // Since test cases work with mocking data we need to use a custom Clone method, this one will use
                // the PnPContext.Clone method and additionally will copy of the "test" settings
                using (var clonedContext = TestCommon.Instance.Clone(context, null, 1))
                {
                    await clonedContext.Web.GetAsync(p => p.Title);

                    Assert.AreEqual(context.Web.Title, clonedContext.Web.Title);
                }
            }
        }

        [TestMethod]
        public async Task ContextCloningForOtherSite()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(p => p.Title);

                var otherSite = TestCommon.Instance.TestUris[TestCommon.TestSubSite];

                using (var clonedContext = context.Clone(otherSite))
                {
                    Assert.AreNotEqual(context.Uri, clonedContext.Uri);
                    Assert.AreEqual(context.AuthenticationProvider, clonedContext.AuthenticationProvider);

                    Assert.AreEqual(context.GraphAlwaysUseBeta, clonedContext.GraphAlwaysUseBeta);
                    Assert.AreEqual(context.GraphCanUseBeta, clonedContext.GraphCanUseBeta);
                    Assert.AreEqual(context.GraphFirst, clonedContext.GraphFirst);

                    Assert.AreEqual(context.RestClient, clonedContext.RestClient);
                    Assert.AreEqual(context.GraphClient, clonedContext.GraphClient);
                    Assert.AreEqual(context.Logger, clonedContext.Logger);

                    Assert.AreNotEqual(context.Id, clonedContext.Id);

                }

                // Since test cases work with mocking data we need to use a custom Clone method, this one will use
                // the PnPContext.Clone method and additionally will copy of the "test" settings
                using (var clonedContext = TestCommon.Instance.Clone(context, otherSite, 1))
                {
                    await clonedContext.Web.GetAsync(p => p.Title);

                    Assert.AreNotEqual(context.Web.Title, clonedContext.Web.Title);
                }
            }
        }

    }
}
