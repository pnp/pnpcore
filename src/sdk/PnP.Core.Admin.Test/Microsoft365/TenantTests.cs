using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Test.Common;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.Microsoft365
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
        public async Task IsMultiGeoTenant()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var multiGeoTenant = context.GetMicrosoft365Admin().IsMultiGeoTenant();
                Assert.IsFalse(multiGeoTenant);
            }
        }

        [TestMethod]
        public async Task GetMultiGeoDataLocations()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var locations = context.GetMicrosoft365Admin().GetMultiGeoLocations();
                Assert.IsNull(locations);
            }
        }

        [TestMethod]
        public async Task AccessTokenAnalysis()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsTrue(context.GetMicrosoft365Admin().AccessTokenHasScope(Constants.DelegatedAccessToken, "AllSites.FullControl"));
                Assert.IsTrue(context.GetMicrosoft365Admin().AccessTokenHasRole(Constants.ApplicationAccessToken, "Sites.FullControl.All"));
                Assert.IsTrue(context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissions(Constants.ApplicationAccessToken));
            }
        }

        [TestMethod]
        public async Task LiveAccessTokenAnalysis()
        {
            TestCommon.PnPCoreSDKTestUserSetup();

            using (var context = await TestCommon.Instance.GetLiveContextAsync())
            {
                Assert.IsTrue(await context.GetMicrosoft365Admin().AccessTokenHasScopeAsync("AllSites.FullControl"));
                Assert.IsFalse(await context.GetMicrosoft365Admin().AccessTokenHasRoleAsync("Sites.FullControl.All"));
                Assert.IsFalse(await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync());
            }
        }

    }
}
