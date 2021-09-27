using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
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

                    Assert.IsTrue(sites.Count > 0);
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

                Assert.IsTrue(sites.Count > 0);
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

                Assert.IsTrue(sites.Count > 0);
            }
        }

    }
}
