using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Model.SharePoint.Core.Internal;
using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Test.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public void ApprovePermissionRequestTest()
        {
            ApprovePermissionRequest request = new();

            request.RequestId = "0000aaaa00000";
            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23401.12003\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"a12697a0-3075-6000-2b64-1556c644e788\"},2,{\"IsNull\":false},4,{\"IsNull\":false},6,{\"IsNull\":false},8,{\"IsNull\":false},9,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionGrant\",\"ClientId\":\"9b994571-2cf2-4fa4-acd0-9fe1f271410f\",\"ConsentType\":\"AllPrincipals\",\"IsDomainIsolated\":false,\"ObjectId\":\"cUWZm_IspE-s0J_h8nFBDyE1Bl7_7v5Jkj7RHjh3Yt0\",\"PackageName\":null,\"Resource\":\"Kylix  Provisioning\",\"ResourceId\":\"5e063521-eeff-49fe-923e-d11e387762dd\",\"Scope\":\"user_impersonation\"}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.AreEqual("Kylix  Provisioning", request.Result.Resource);
        }

        [TestMethod]
        public void DenyPermissionRequestTest()
        {
            DenyPermissionRequest request = new();

            request.RequestId = "0000aaaa00000";
            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23401.12003\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"e82697a0-1009-6000-2b64-1b9a0d83d5e2\"},2,{\"IsNull\":false},4,{\"IsNull\":false},6,{\"IsNull\":false}]";

            request.ProcessResponse(response);
            Assert.IsNotNull(request.Result);
        }


        [TestMethod]
        public void GetPermissionsRequestsRequestTest()
        {
            GetPermissionRequestsRequest request = new();

            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23401.12003\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"23d996a0-5009-6000-2b64-14f4eaefefa8\"},2,{\"IsNull\":false},4,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequestCollection\",\"_Child_Items_\":[{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(4c9fb5af-b111-4b96-9ecd-efc3dbb53ac1)/\",\"IsDomainIsolated\":true,\"IsolatedDomainUrl\":\"microsoft.com\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"somesolution-ui-client-side-solution\",\"PackageVersion\":\"1.0.0.0\",\"Resource\":\"api://com.loitzl.test/somesolution\",\"ResourceId\":\"api://com.loitzl.test/somesolution\",\"Scope\":\"Config.Manage\",\"TimeRequested\":\"/Date(1636643974000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(9a3338df-8f97-4ba1-bdde-03fde282dfc0)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Workspaces\",\"PackageVersion\":\"2.0.0.0\",\"Resource\":\"Kylix Workspaces\",\"ResourceId\":\"Kylix Workspaces\",\"Scope\":\"user_impersonation\",\"TimeRequested\":\"/Date(1651759632000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(f669b2e6-cb54-47cc-b91a-e3f589fc984d)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Hub\",\"PackageVersion\":\"6.0.0.0\",\"Resource\":\"Office 365 Exchange Online\",\"ResourceId\":\"Office 365 Exchange Online\",\"Scope\":\"Tasks.Read\",\"TimeRequested\":\"/Date(1651760702000)/\"},{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionRequest\",\"ClientComponentItemUniqueId\":\"\",\"Id\":\"/Guid(f929f6da-921f-4580-a9a2-6ff8817e5569)/\",\"IsDomainIsolated\":false,\"IsolatedDomainUrl\":\"\",\"MultiTenantAppId\":\"\",\"MultiTenantAppReplyUrl\":\"\",\"PackageApproverName\":\"John Doe\",\"PackageName\":\"Kylix Intranet Provisioner\",\"PackageVersion\":\"6.0.0.0\",\"Resource\":\"Microsoft Graph\",\"ResourceId\":\"Microsoft Graph\",\"Scope\":\"User.Read.All\",\"TimeRequested\":\"/Date(1651760702000)/\"}]}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.AreEqual(4, request.Result.Count);
        }

        [TestMethod]
        public async Task GetPermissionsRequestsTest_Async()
        {
            TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                Uri url = context.GetSharePointAdmin().GetTenantPortalUri();

                using (PnPContext tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    ServicePrincipal principal = new(context);
                    List<IPermissionRequest> permissionRequests = await principal.GetPermissionRequests();
                    Assert.IsNotNull(permissionRequests);
                    Assert.IsTrue(permissionRequests.Count > 0);
                }
            }
        }

        [TestMethod]
        public async Task ApprovePermissionsRequestTest_Async()
        {
            TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            ServicePrincipal principal = new(context);
            List<IPermissionRequest> permissionRequests = await principal.GetPermissionRequests();

            var result = await principal.ApprovePermissionRequest(permissionRequests.First().Id.ToString());
            
            Assert.IsNotNull(result);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.ObjectId));
        }

        [TestMethod]
        public async Task DenyPermissionsRequestTest_Async()
        {
            TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            Uri url = context.GetSharePointAdmin().GetTenantPortalUri();

            ServicePrincipal principal = new(context);
            List<IPermissionRequest> permissionRequests = await principal.GetPermissionRequests();

            await principal.DenyPermissionRequest(permissionRequests.First().Id.ToString());
        }
    }
}
