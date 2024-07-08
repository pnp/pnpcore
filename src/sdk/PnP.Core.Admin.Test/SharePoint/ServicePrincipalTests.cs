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

        #region SPOWebAppServicePrincipal CSOM based grants API
        
        [TestMethod]
        public void ListGrantsRequestTest()
        {
            ListGrantsRequest request = new();
            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23422.12001\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"d99b9da0-40da-6000-2b64-1a864c9568f9\"},2,{\"IsNull\":false},4,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionGrantCollection\",\"_Child_Items_\":[{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionGrant\",\"ClientId\":\"9b994571-2cf2-4fa4-acd0-9fe1f271410f\",\"ConsentType\":\"AllPrincipals\",\"IsDomainIsolated\":false,\"ObjectId\":\"cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U\",\"PackageName\":null,\"Resource\":\"Microsoft Graph\",\"ResourceId\":\"19cef431-469f-4898-8e72-dfd13054b3f5\",\"Scope\":\"Calendars.Read\"}]}]";

            request.ProcessResponse(response);

            Assert.IsTrue(request.Result.Any());
            Assert.AreEqual("9b994571-2cf2-4fa4-acd0-9fe1f271410f", request.Result.First().ClientId);
            Assert.AreEqual("AllPrincipals", request.Result.First().ConsentType);
            Assert.IsFalse(request.Result.First().IsDomainIsolated);
            Assert.AreEqual("cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U", request.Result.First().ObjectId);
            Assert.IsNull(request.Result.First().PackageName);
            Assert.AreEqual("Microsoft Graph", request.Result.First().Resource);
            Assert.AreEqual("19cef431-469f-4898-8e72-dfd13054b3f5", request.Result.First().ResourceId);
            Assert.AreEqual("Calendars.Read", request.Result.First().Scope);
        }

        [TestMethod]
        public void AddGrantRequestTest()
        {
            AddGrantRequest request = new() {Scope = "Calendars.ReadWrite", Resource = "Microsoft Graph"};

            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23422.12001\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"54d79da0-10c5-6000-4c98-92b0429a428f\"},2,{\"IsNull\":false},4,{\"IsNull\":false},6,{\"IsNull\":false},7,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionGrant\",\"ClientId\":\"9b994571-2cf2-4fa4-acd0-9fe1f271410f\",\"ConsentType\":\"AllPrincipals\",\"IsDomainIsolated\":false,\"ObjectId\":\"cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U\",\"PackageName\":null,\"Resource\":\"Microsoft Graph\",\"ResourceId\":\"19cef431-469f-4898-8e72-dfd13054b3f5\",\"Scope\":\"Calendars.ReadWrite.Shared\"}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.AreEqual("9b994571-2cf2-4fa4-acd0-9fe1f271410f", request.Result.ClientId);
            Assert.AreEqual("AllPrincipals", request.Result.ConsentType);
            Assert.IsFalse(request.Result.IsDomainIsolated);
            Assert.AreEqual("cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U", request.Result.ObjectId);
            Assert.IsNull(request.Result.PackageName);
            Assert.AreEqual("Microsoft Graph", request.Result.Resource);
            Assert.AreEqual("19cef431-469f-4898-8e72-dfd13054b3f5", request.Result.ResourceId);
            Assert.AreEqual("Calendars.ReadWrite.Shared", request.Result.Scope);
        }

        [TestMethod]
        public async Task AddListRevokeGrantTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using (PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite))
            {
                ServicePrincipal servicePrincipal = new(context);

                IPermissionGrant addedGrant =
                    servicePrincipal.AddGrant("Microsoft Graph", "Calendars.ReadWrite.Shared");

                Assert.IsNotNull(addedGrant);
                Assert.AreEqual("Microsoft Graph", addedGrant.Resource);
                Assert.AreEqual("Calendars.ReadWrite.Shared", addedGrant.Scope);

                var grants =
                    servicePrincipal.ListGrants();

                Assert.IsTrue(grants.Any(g => g.ObjectId.Equals(addedGrant.ObjectId)));

                IPermissionGrant revokedGrant =
                    servicePrincipal.RevokeGrant(addedGrant.ObjectId);

                Assert.IsNotNull(revokedGrant);
                Assert.AreEqual("Calendars.ReadWrite.Shared", addedGrant.Scope);
            }
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
                    IServicePrincipalProperties result = principal.Disable();
                    Assert.IsFalse(result.AccountEnabled);
                    Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                    Assert.IsTrue(result.ReplyUrls.Any());
                }
                finally
                {
                    ServicePrincipal principal = new(context);
                    IServicePrincipalProperties result = principal.Enable();
                    Assert.IsTrue(result.AccountEnabled);
                    Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                    Assert.IsTrue(result.ReplyUrls.Any());
                }
            }
        }

        [TestMethod]
        public void RevokeGrantRequestTest()
        {
            RevokeGrantRequest request = new() {ObjectId = "cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U"};

            request.GetRequest(new IteratorIdProvider());

            const string response =
                "[{\"SchemaVersion\":\"15.0.0.0\",\"LibraryVersion\":\"16.0.23422.12001\",\"ErrorInfo\":null,\"TraceCorrelationId\":\"51d69da0-a08e-6000-4c98-9d42be881c3b\"},2,{\"IsNull\":false},4,{\"IsNull\":false},6,{\"IsNull\":false},8,{\"_ObjectType_\":\"Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipalPermissionGrant\",\"ClientId\":\"9b994571-2cf2-4fa4-acd0-9fe1f271410f\",\"ConsentType\":\"AllPrincipals\",\"IsDomainIsolated\":false,\"ObjectId\":\"cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U\",\"PackageName\":null,\"Resource\":null,\"ResourceId\":\"19cef431-469f-4898-8e72-dfd13054b3f5\",\"Scope\":\"Calendars.Read Calendars.ReadWrite Calendars.ReadWrite.Shared\"}]";

            request.ProcessResponse(response);

            Assert.IsNotNull(request.Result);
            Assert.AreEqual("9b994571-2cf2-4fa4-acd0-9fe1f271410f", request.Result.ClientId);
            Assert.AreEqual("AllPrincipals", request.Result.ConsentType);
            Assert.IsFalse(request.Result.IsDomainIsolated);
            Assert.AreEqual("cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U", request.Result.ObjectId);
            Assert.IsNull(request.Result.PackageName);
            Assert.IsNull(request.Result.Resource);
            Assert.AreEqual("19cef431-469f-4898-8e72-dfd13054b3f5", request.Result.ResourceId);
            Assert.AreEqual("Calendars.Read Calendars.ReadWrite Calendars.ReadWrite.Shared", request.Result.Scope);
        }
        
        #endregion
        
        #region Graph based grants API
        
        [TestMethod]
        public async Task Enable2Disable2ServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            try
            {
                ServicePrincipal principal = new(context);
                IServicePrincipalProperties result = principal.Disable2();
                Assert.IsFalse(result.AccountEnabled);
                Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                Assert.IsTrue(result.ReplyUrls.Any());
            }
            finally
            {
                ServicePrincipal principal = new(context);
                IServicePrincipalProperties result = principal.Enable2();
                Assert.IsTrue(result.AccountEnabled);
                Assert.IsTrue(!string.IsNullOrEmpty(result.AppId));
                Assert.IsTrue(result.ReplyUrls.Any());
            }
        }

        [TestMethod]
        public async Task Add2Revoke2ServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            ServicePrincipal servicePrincipal = new(context);
            
            IPermissionGrant2 addedGrant =
                servicePrincipal.AddGrant2("Microsoft Graph", "Calendars.ReadWrite.Shared");
            
            Assert.IsNotNull(addedGrant);
            Assert.AreEqual("Microsoft Graph", addedGrant.ResourceName);
            Assert.AreEqual("AllPrincipals", addedGrant.ConsentType);
            Assert.IsTrue(
                addedGrant
                    .Scope
                    .Contains("Calendars.ReadWrite.Shared", StringComparison.InvariantCultureIgnoreCase));

            var revokedGrant = servicePrincipal.RevokeGrant2(addedGrant.Id, "Calendars.ReadWrite.Shared");
            
            Assert.IsNotNull(revokedGrant);
            Assert.IsFalse(revokedGrant.Scope.Contains("Calendars.ReadWrite.Shared", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public async Task Add2Delete2ServicePrincipalTest_Async()
        {
            //TestCommon.Instance.Mocking = false;
            using PnPContext context = await TestCommon.Instance.GetContextAsync(TestCommonBase.TestSite);
            ServicePrincipal servicePrincipal = new(context);
            
            IPermissionGrant2 addedGrant =
                servicePrincipal.AddGrant2("Azure DevOps", "vso.agentpools");
            Assert.IsNotNull(addedGrant);
            Assert.AreEqual("Azure DevOps", addedGrant.ResourceName);
            
            servicePrincipal.DeleteGrant2(addedGrant.Id);
            
            var grants = servicePrincipal.ListGrants2();
            Assert.IsFalse(grants.Any( g => g.ResourceName.Equals("Azure DevOps", StringComparison.InvariantCultureIgnoreCase)));
        }
        
        #endregion
    }
}
