using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test
{
    [TestClass]
    public class TenantTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTenantAdminCenterUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                Assert.IsTrue(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                    Assert.IsTrue(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public async Task GetTenantAdminCenterContext()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                using (var tenantContext = context.GetSharePointAdmin().GetTenantAdminCenterContext())
                {
                    var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                    Assert.IsTrue(tenantContext != null);
                    Assert.IsTrue(tenantContext.Web.Requested);
                    Assert.IsTrue(tenantContext.Web.IsPropertyAvailable(p => p.Id));
                    Assert.IsTrue(tenantContext.Uri == url);
                }
            }
        }

        [TestMethod]
        public async Task GetTenantAppCatalogUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantAppCatalogUri();
                Assert.IsTrue(url != null);
            }
        }

        [TestMethod]
        public async Task EnsureTenantAppCatalogUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppCatalogCreated = context.GetSharePointAdmin().EnsureTenantAppCatalog();
                Assert.IsFalse(tenantAppCatalogCreated);
            }
        }
        
    }
}
