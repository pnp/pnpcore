using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
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
    }
}
