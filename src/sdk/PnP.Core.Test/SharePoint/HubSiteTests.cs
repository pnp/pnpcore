using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class HubSiteTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);
                
            }
        }

        [TestMethod]
        public async Task GetDirectHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.IsTrue(site.IsPropertyAvailable(s => s.IsHubSite));

                if (!site.IsHubSite) { 
                    var hub = await site.RegisterHubSiteAsync();
                    Assert.IsNotNull(hub);
                }

                site = await context.Site.GetAsync(p => p.HubSiteId); // refresh the hubsite id

                // Seperate Get Operation
                IHubSite hubSite = new HubSite()
                {
                    PnPContext = context,
                    Id = site.HubSiteId
                   
                };

                await hubSite.LoadAsync();

                Assert.AreEqual(hubSite.Id, site.HubSiteId);
                Assert.IsTrue(!string.IsNullOrEmpty(hubSite.Title));

                // Refresh and clean up
                site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                await site.UnregisterHubSiteAsync();
            }
        }

        [TestMethod]
        public async Task GetAnyHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This tests retrieving another hubsite not associated with the current context
                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, id: 1))
                {

                    ISite site = await context2.Site.GetAsync(
                        p => p.HubSiteId,
                        p => p.IsHubSite);

                    Assert.IsNotNull(site);
                    Assert.IsTrue(site.IsPropertyAvailable(s => s.IsHubSite));

                    if (!site.IsHubSite)
                    {
                        var hub = await site.RegisterHubSiteAsync();
                        Assert.IsNotNull(hub);
                    }

                    site = await context2.Site.GetAsync(p => p.HubSiteId); // refresh the hubsite id

                    // Seperate Get Operation
                    IHubSite hubSite = new HubSite()
                    {
                        PnPContext = context,
                        Id = site.HubSiteId

                    };

                    var hubResult = await hubSite.GetAsync();

                    Assert.AreEqual(hubResult.Id, site.HubSiteId);
                    Assert.IsTrue(!string.IsNullOrEmpty(hubResult.Title));
                    Assert.IsTrue(!string.IsNullOrEmpty(hubResult.SiteUrl));

                    //// Refresh and clean up
                    site = await context2.Site.GetAsync(
                        p => p.HubSiteId,
                        p => p.IsHubSite);

                    await site.UnregisterHubSiteAsync();

                }
            }
        }

        [TestMethod]
        public async Task RegisterHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);

                var result = await site.RegisterHubSiteAsync();
                Assert.IsNotNull(result);

                // Refresh
                site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                await site.UnregisterHubSiteAsync();

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task UnRegisterHubSiteExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);

                await site.UnregisterHubSiteAsync();
            }
        }

        [TestMethod]
        public async Task AlreadyRegisteredHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                   p => p.HubSiteId,
                   p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId, "Site shouldnt be a hub site already");
                Assert.IsFalse(site.IsHubSite);

                var result = await site.RegisterHubSiteAsync();
                Assert.IsNotNull(result);

                await Assert.ThrowsExceptionAsync<ClientException>( async () => {

                    // Check this is updated
                    site = await context.Site.GetAsync(
                           p => p.HubSiteId,
                           p => p.IsHubSite);

                    await site.RegisterHubSiteAsync();
                });

                // Cleanup
                site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                await site.UnregisterHubSiteAsync();
            }
        }

        [TestMethod]
        public async Task JoinUnJoinHubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var contextPrimaryHub = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 1))
            {
                ISite site = await contextPrimaryHub.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);

                var result = await site.RegisterHubSiteAsync();
                Assert.IsNotNull(result);

                // Refresh
                site = await contextPrimaryHub.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite);

                // Associate group site to the hub
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
                {
                    ISite assocSite = await context.Site.GetAsync(
                        p => p.HubSiteId,
                        p => p.IsHubSite);

                    Assert.IsNotNull(assocSite);
                    Assert.AreEqual(default, assocSite.HubSiteId);
                    Assert.IsFalse(assocSite.IsHubSite);

                    Assert.AreNotEqual(default, site.HubSiteId);

                    var resultJoin = await assocSite.JoinHubSiteAsync(site.HubSiteId);
                    Assert.IsTrue(resultJoin);

                    var resultUnJoin = await assocSite.UnJoinHubSiteAsync();
                    Assert.IsTrue(resultUnJoin);
                }

                // Clean up
                await site.UnregisterHubSiteAsync();
            }
        }


    }
}
