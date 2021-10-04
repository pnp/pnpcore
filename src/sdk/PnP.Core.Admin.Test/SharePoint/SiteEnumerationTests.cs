using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class SiteEnumerationTests
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
        public async Task EnumerateSitesWithApplicationPermissionsViaSitesApi()
        {
            //TestCommon.Instance.Mocking = false;
            try
            {
                TestCommon.Instance.UseApplicationPermissions = true;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var sites = await SiteCollectionEnumerator.GetViaGraphSitesApiAsync(context);

                    VerifySite(sites, context);
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task EnumerateSitesWithDelegatedPermissionsViaSearchApi()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var sites = await SiteCollectionEnumerator.GetViaGraphSearchApiAsync(context);

                VerifySite(sites, context);

            }
        }

        [TestMethod]
        public async Task EnumerateSitesWithDelegatedPermissionsViaSharePointAdminCenter()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var sites = await SiteCollectionEnumerator.GetViaTenantAdminHiddenListAsync(context);

                VerifySite(sites, context);
            }
        }

        private void VerifySite(List<ISiteCollection> sites, PnPContext context)
        {
            Assert.IsTrue(sites.Count > 0);
            var myTestSite = sites.FirstOrDefault(p => p.Id == context.Site.Id);                
            Assert.IsTrue(myTestSite != null);
            Assert.IsTrue(myTestSite.RootWebId == context.Web.Id);
            Assert.IsTrue(!string.IsNullOrEmpty(myTestSite.Name));
        }

        [TestMethod]
        public async Task EnumerateSitesWithDetails()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var sites = await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context);

                Assert.IsTrue(sites.Count > 0);
                var myTestSite = sites.FirstOrDefault(p => p.Id == context.Site.Id);
                Assert.IsTrue(myTestSite != null);
                Assert.IsTrue(myTestSite.RootWebId == context.Web.Id);
                Assert.IsTrue(!string.IsNullOrEmpty(myTestSite.Name));
                Assert.IsTrue(!string.IsNullOrEmpty(myTestSite.CreatedBy));
                Assert.IsTrue(myTestSite.TimeCreated > DateTime.MinValue);
                Assert.IsTrue(myTestSite.StorageQuota > 0);
                Assert.IsTrue(myTestSite.StorageUsed > 0);
                Assert.IsTrue(!string.IsNullOrEmpty(myTestSite.TemplateName));
            }
        }

    }
}
