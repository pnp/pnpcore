using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Test.Common.Utilities;
using System;
using System.Linq;
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
        public void SetServicePrincipalRequestTest()
        {
            SetServicePrincipalRequest request = new() {Enabled = true};

            request.GetRequest(new IteratorIdProvider());
            
            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23408.12001\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"e5ad99a0-0053-6000-4c98-9977844229ad\"},2,{\"IsNull\":false},5,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipal\",\"AccountEnabled\":true,\"AppId\":\"40ed0677-9e6f-435c-b198-7634beba3874\",\"ReplyUrls\":[\"https://fluidpreview.office.net/spfxsinglesignon\",\"https://dev.fluidpreview.office.net/spfxsinglesignon\",\"https://tenant-admin.sharepoint.com/_forms/spfxsinglesignon.aspx\",\"https://tenant.sharepoint.com/_forms/spfxsinglesignon.aspx?redirect\",\"https://tenant.sharepoint.com/_forms/spfxsinglesignon.aspx\",\"https://tenant.sharepoint.com/\"]}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.IsTrue(request.Result.AccountEnabled);
        }
       
        [TestMethod]
        public async Task EnableDisableServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                try
                {
                    ServicePrincipal principal = new(context);
                    var result = principal.Disable();
                    Assert.IsFalse(result.AccountEnabled);
                    Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                    Assert.IsTrue(result.ReplyUrls.Any());
                }
                finally
                {
                    ServicePrincipal principal = new(context);
                    var result = principal.Enable();
                    Assert.IsTrue(result.AccountEnabled);
                    Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                    Assert.IsTrue(result.ReplyUrls.Any());
                }
            }
        }
    }
}
