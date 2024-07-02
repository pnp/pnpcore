using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Test.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class PermissionRequestsTests
    {
        private const string packageName = "apicalltest.sppkg";
        private string packagePath = $"TestAssets/{packageName}";

        private const string packageName1 = "scopes-app-1.sppkg";
        private string packagePath1 = $"TestAssets/{packageName1}";

        private const string packageName2 = "scopes-app-2.sppkg";
        private string packagePath2 = $"TestAssets/{packageName2}";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void ApprovePermissionRequestTest()
        {
            ApprovePermissionRequest request = new() {RequestId = "0000aaaa00000"};

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
            DenyPermissionRequest request = new() {RequestId = "0000aaaa00000"};

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

        #region IApp.ParsePermissionRequests

        [TestMethod]
        public void IApp_ParsePermissionRequests_ErroneousInput3()
        {
            var result = TenantAppManager.ParsePermissionRequests(null);
            
            Assert.AreEqual(0, result.Count);
        }
        
        [TestMethod]
        public void IApp_ParsePermissionRequests_ErroneousInput2()
        {
            var result = TenantAppManager.ParsePermissionRequests("Lorem Ipsum");
        
            Assert.AreEqual(0, result.Count);
        }
        
        [TestMethod]
        public void IApp_ParsePermissionRequests_ErroneousInput()
        {
            var result = TenantAppManager.ParsePermissionRequests("Lorem Ipsum; dolor");
        
            Assert.AreEqual(0, result.Count);
        }
        
        [TestMethod]
        public void IApp_ParsePermissionRequests_Succeeds()
        {
            var result = 
                TenantAppManager.ParsePermissionRequests("Microsoft Graph, User.ReadBasic.All; Microsoft Graph, Calendars.Read; Microsoft Graph, Application.ReadWrite.All; App1, access_as_user; App1, Scope2");
        
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Microsoft Graph", result.ElementAt(0).Key);
            Assert.AreEqual("User.ReadBasic.All Calendars.Read Application.ReadWrite.All", result.ElementAt(0).Value);
            Assert.AreEqual("App1", result.ElementAt(1).Key);
            Assert.AreEqual("access_as_user Scope2", result.ElementAt(1).Value);
        }

        #endregion

        [TestMethod]
        public async Task ApprovePermissionRequestsTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            
            ITenantApp app1 = null;
            ITenantApp app2 = null;
            try
            {
                // App1 contains permission request: Microsoft Graph (Sites.Selected)   
                var appManager = context.GetTenantAppManager();
                app1 = await appManager.AddAsync(packagePath1, true);
                var deployResult1 = await app1.DeployAsync(false);

                Assert.IsTrue(deployResult1);

                IPermissionGrant2[] approvedPermissionGrants1
                    = await app1.ApprovePermissionRequestsAsync();

                Assert.AreEqual(1, approvedPermissionGrants1.Length);
                
                // App2 contains permission request: Office 365 SharePoint Online (Sites.Selected)
                app2 = await appManager.AddAsync(packagePath2, true);
                var deployResult2 = await app2.DeployAsync(false);

                Assert.IsTrue(deployResult2);

                IPermissionGrant2[] approvedPermissionGrants2 = await app2.ApprovePermissionRequestsAsync();
                Assert.AreEqual(2, approvedPermissionGrants2.Length);
            }
            finally
            {
                if (app1 != null)
                {
                    await app1.RetractAsync();
                    await app1.RemoveAsync();
                }
                if (app2 != null)
                {
                    await app2.RetractAsync();
                    await app2.RemoveAsync();
                }
            }
        }

        [TestMethod]
        public async Task DenyPermissionsRequestTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                ITenantApp app = null;
                try
                {
                    var appManager = context.GetTenantAppManager();
                    app = appManager.Add(packagePath, true);
                    var deployResult = app.Deploy(false);

                    Assert.IsTrue(deployResult);

                    List<IPermissionRequest> permissionRequests =
                        await appManager.ServicePrincipal.GetPermissionRequestsAsync();

                    await appManager.ServicePrincipal.DenyPermissionRequestAsync(permissionRequests.First().Id
                        .ToString());
                }
                finally
                {
                    var retractResult = app.Retract();
                    app.Remove();
                }
            }
        }
    }
}
