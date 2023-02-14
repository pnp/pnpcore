using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Admin.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class PermissionRequestsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void ApprovePermissionRequest()
        {
            var request = new ApprovePermissionRequest();
            var csom = request.GetRequest(new IteratorIdProvider());

        }
        [TestMethod]
        public async Task GetPermissionsRequest_Async()
        {

            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantPortalUri();

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var principal = new ServicePrincipal(context);
                    var permissionRequests = await principal.GetPermissionRequests();

                }
            }
        }

    }
}