using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class TenantTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod]
        public void GetTenantPortalUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
        }

        [TestMethod]
        public async Task GetTenantPortalUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantPortalUri();
                Assert.IsFalse(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsFalse(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantPortalUri();
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void GetTenantMySiteHostUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
        }

        [TestMethod]
        public async Task GetTenantMySiteHostUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantMySiteHostUri();
                Assert.IsFalse(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantMySiteHostUri();
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                    Assert.IsTrue(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void GetTenantAdminCenterUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
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
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
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
        public async Task GetTenantAdmins()
        {
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.UseApplicationPermissions = true;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var admins = context.GetSharePointAdmin().GetTenantAdmins();
                Assert.IsTrue(admins != null);
                Assert.IsTrue(admins.Any());
            }
        }

        [TestMethod]
        public async Task IsCurrentUserSharePointAdmin()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsTrue(context.GetSharePointAdmin().IsCurrentUserTenantAdmin());
            }
        }

        [TestMethod]
        public async Task CurrentUserIsNotSharePointAdmin()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {
                    return "{ \"responses\": [ { \"id\": \"1\", \"status\": 200, \"headers\": { \"Cache-Control\": \"no-cache\", \"x-ms-resource-unit\": \"2\", \"OData-Version\": \"4.0\", \"Content-Type\": \"application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false;charset=utf-8\" }, \"body\": { \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#directoryObjects\", \"@odata.count\": 0, \"value\": [] } } ] }";
                };

                Assert.IsFalse(context.GetSharePointAdmin().IsCurrentUserTenantAdmin());
            }
        }


        [TestMethod]
        public async Task GetTenantAppCatalogUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetAppManager().GetTenantAppCatalogUri();
                Assert.IsTrue(url != null);
            }
        }

        [TestMethod]
        public async Task EnsureTenantAppCatalogUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppCatalogCreated = context.GetAppManager().EnsureTenantAppCatalog();
                Assert.IsFalse(tenantAppCatalogCreated);
            }
        }

    }
}
