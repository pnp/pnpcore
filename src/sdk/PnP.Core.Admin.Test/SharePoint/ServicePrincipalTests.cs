using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Test.Common.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class ServicePrincipalTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task EnableServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                ServicePrincipal principal = new(context);
                var result = await principal.Enable();
                Assert.IsTrue(result.AccountEnabled);
            }
        }

        
        
        [TestMethod]
        public async Task DisableServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                ServicePrincipal principal = new(context);
                var result = await principal.Disable();
                Assert.IsFalse(result.AccountEnabled);
            }
        }
    }
}