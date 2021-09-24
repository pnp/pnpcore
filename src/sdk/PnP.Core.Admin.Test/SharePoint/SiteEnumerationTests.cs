using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System.Collections.Generic;
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
        public async Task EnumerateSitesWithApplicationPermissions()
        {
            TestCommon.Instance.UseApplicationPermissions = true;
            await EnumerateSitesImplementation("EnumerateSitesWithApplicationPermissions");
            TestCommon.Instance.UseApplicationPermissions = false;
        }

        [TestMethod]
        public async Task EnumerateSitesWithDelegatedPermissions()
        {
            TestCommon.Instance.UseApplicationPermissions = false;
            var sitesUsingSearch = await EnumerateSitesImplementation("EnumerateSitesWithDelegatedPermissionsSearch", true);
            var sitesUsingTenantAdmin = await EnumerateSitesImplementation("EnumerateSitesWithDelegatedPermissionsSites", false);

            //// Verify all sites returned via admin are also in sites returned via search
            //foreach(var site in sitesUsingTenantAdmin)
            //{
            //    if (sitesUsingSearch.FirstOrDefault(p => p.Id == site.Id) == null)
            //    {

            //    }
            //}
        }

        private static async Task<List<ISiteCollection>> EnumerateSitesImplementation(string testName, bool ignoreUserIsSharePointAdmin = false)
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, testName: testName))
            {
                var sites = context.GetSharePointAdmin().GetSiteCollections(ignoreUserIsSharePointAdmin);

                Assert.IsTrue(sites.Count > 0);

                return sites;
            }
        }
    }
}
