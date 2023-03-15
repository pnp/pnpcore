using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class LegacyPrincipalTests
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
        public async Task GetACSPrincipals()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var principals = context.GetSiteCollectionManager().GetSiteCollectionACSPrincipals();

                Assert.IsTrue(principals.Any());
            }
        }

        [TestMethod]
        public async Task GetLegacyACSPrincipalsFromAAD()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var result = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                Assert.IsNotNull(result);   
                Assert.IsTrue(result.Any());    
            }
        }

        [TestMethod]
        public async Task GetAllACSPrincipals()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var legacyServicePrincipals = context.GetSiteCollectionManager().GetLegacyServicePrincipals();
                var principals = context.GetSiteCollectionManager().GetTenantAndSiteCollectionACSPrincipals(legacyServicePrincipals);

                Assert.IsTrue(principals.Any());
            }
        }

    }
}
