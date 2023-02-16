using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Test.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            ApprovePermissionRequest request = new ApprovePermissionRequest();
            List<ActionObjectPath> csom = request.GetRequest(new IteratorIdProvider());
        }

        [TestMethod]
        public void GetPermissionsRequestsRequest()
        {
            GetPermissionRequestsRequest request = new GetPermissionRequestsRequest();

            request.GetRequest(new IteratorIdProvider());

            string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23401.12003\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"23d996a0-5009-6000-2b64-14f4eaefefa8\"},2,{\"IsNull\":false},4,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequestCollection\",\"_Child_Items_\":[{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(4c9fb5af-b111-4b96-9ecd-efc3dbb53ac1)/\",\"IsDomainIsolated\":true,\"IsolatedDomainUrl\":\"microsoft.com\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"somesolution-ui-client-side-solution\",\"PackageVersion\":\"1.0.0.0\",\"Resource\":\"api://com.loitzl.test/somesolution\",\"ResourceId\":\"api://com.loitzl.test/somesolution\",\"Scope\":\"Config.Manage\",\"TimeRequested\":\"/Date(1636643974000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(9a3338df-8f97-4ba1-bdde-03fde282dfc0)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Workspaces\",\"PackageVersion\":\"2.0.0.0\",\"Resource\":\"Kylix Workspaces\",\"ResourceId\":\"Kylix Workspaces\",\"Scope\":\"user_impersonation\",\"TimeRequested\":\"/Date(1651759632000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(f669b2e6-cb54-47cc-b91a-e3f589fc984d)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Hub\",\"PackageVersion\":\"6.0.0.0\",\"Resource\":\"Office 365 Exchange Online\",\"ResourceId\":\"Office 365 Exchange Online\",\"Scope\":\"Tasks.Read\",\"TimeRequested\":\"/Date(1651760702000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(f929f6da-921f-4580-a9a2-6ff8817e5569)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Provisioner\",\"PackageVersion\":\"6.0.0.0\",\"Resource\":\"Microsoft Graph\",\"ResourceId\":\"Microsoft Graph\",\"Scope\":\"User.Read.All\",\"TimeRequested\":\"/Date(1651760702000)/\"}]}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.AreEqual(4, request.Result.Count);
        }

        [TestMethod]
        public async Task GetPermissionsRequest_Async()
        {
            TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                Uri url = context.GetSharePointAdmin().GetTenantPortalUri();

                using (PnPContext tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    ServicePrincipal principal = new ServicePrincipal(context);
                    List<IPermissionRequest> permissionRequests = await principal.GetPermissionRequests();
                    Assert.IsNotNull(permissionRequests);
                    Assert.IsTrue(permissionRequests.Count > 0);
                }
            }
        }
    }
}