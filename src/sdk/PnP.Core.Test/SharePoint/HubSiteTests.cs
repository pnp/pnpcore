using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        [ExpectedException(typeof(ArgumentException))]
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

        //[TestMethod]
        //public async Task JoinHubSiteTest()
        //{
        //    throw new NotImplementedException();

        //    //TestCommon.Instance.Mocking = false;
        //    //using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    //{
        //    //    ISite site = await context.Site.GetAsync(
        //    //        p => p.HubSiteId,
        //    //        p => p.IsHubSite);

        //    //    Assert.IsNotNull(site);
        //    //    Assert.AreEqual(default, site.HubSiteId);
        //    //    Assert.IsFalse(site.IsHubSite);

        //    //}
        //}


    }
}
