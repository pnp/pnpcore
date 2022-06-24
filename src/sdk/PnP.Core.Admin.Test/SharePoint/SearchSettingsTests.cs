using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class SearchSettingsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        #region Get Search configuration

        [TestMethod]
        public async Task GetWebSearchConfigurationTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var searchConfigXml = context.GetSharePointAdmin().GetTenantSearchConfigurationXml();

                Assert.IsNotNull(searchConfigXml);
                Assert.IsTrue(!string.IsNullOrEmpty(searchConfigXml));
            }
        }

        [TestMethod]
        public async Task GetWebSearchManagedPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var mps = context.GetSharePointAdmin().GetTenantSearchConfigurationManagedProperties();

                Assert.IsNotNull(mps);
                Assert.IsTrue(mps.Count > 0);
            }
        }
        #endregion
    }
}
