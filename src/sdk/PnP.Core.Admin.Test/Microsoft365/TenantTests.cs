using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Test.Common;
using System.Linq;
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
                Assert.IsTrue(context.GetMicrosoft365Admin().AccessTokenHasScope("AllSites.FullControl"));
                Assert.IsFalse(context.GetMicrosoft365Admin().AccessTokenHasRole("Sites.FullControl.All"));
                Assert.IsFalse(context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissions());
            }
        }

        [TestMethod]
        public async Task GetSensitivityLabelsUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var labels = await SensitivityLabelManager.GetLabelsUsingDelegatedPermissionsAsync(context);
                    Assert.IsTrue(labels.Any());
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        [TestMethod]
        public async Task GetSensitivityLabelsUsingApplicationPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var labels = await SensitivityLabelManager.GetLabelsUsingApplicationPermissionsAsync(context);
                    Assert.IsTrue(labels.Any());
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }
    }
}
