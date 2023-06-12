using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class ServicePrincipal : IServicePrincipal
    {
        private readonly PnPContext context;

        internal ServicePrincipal(PnPContext context)
        {
            this.context = context;
        }

        public IPermissionGrant ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            return ApprovePermissionRequestAsync(id, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IPermissionGrant> ApprovePermissionRequestAsync(string id,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            ApprovePermissionRequest request = new() {RequestId = id};

            ApiCall getPermissionRequestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IPermissionGrant)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public void DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            DenyPermissionRequestAsync(id, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task DenyPermissionRequestAsync(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            DenyPermissionRequest request = new() {RequestId = id};

            ApiCall getPermissionRequestsCall = new(
                new List<IRequest<object>> {request});

            await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);
        }

        public List<IPermissionRequest> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetPermissionRequestsAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IPermissionRequest>> GetPermissionRequestsAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            GetPermissionRequestsRequest request = new();

            ApiCall getPermissionRequestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await (tenantAdminContext.Web as Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            string appPrincipalId = await GetSharePointAppPrincipalId().ConfigureAwait(false);
            
            List<IPermissionRequest> permissionRequests =  (List<IPermissionRequest>) csomResult.ApiCall.CSOMRequests[0].Result;

            if (string.IsNullOrWhiteSpace(appPrincipalId))
            {
                return permissionRequests;
            }
            
            List<string> alreadyGranted =  await GetSharePointAppPrincipalPermissionGrants(appPrincipalId).ConfigureAwait(false);
            
            return permissionRequests
                .Where(permissionRequest => !alreadyGranted.Contains(permissionRequest.Scope))
                .ToList();

            async Task<string> GetSharePointAppPrincipalId()
            {
                ApiCall getPrincipalIdCall =
                    new ApiCall(
                        "servicePrincipals/?$filter=displayName eq 'SharePoint Online Client Extensibility Web Application Principal'",
                        ApiType.Graph);

                ApiCallResponse result = await (tenantAdminContext.Web as Web)
                    .RawRequestAsync(getPrincipalIdCall, HttpMethod.Get)
                    .ConfigureAwait(false);

                #region JsonResponse

                /*
                 {
                  "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#servicePrincipals",
                  "value": [
                    {
                      "id": "9b994571-2cf2-4fa4-acd0-9fe1f271410f",
                      "accountEnabled": false,
                      "alternativeNames": [],
                      "appDisplayName": "SharePoint Online Client Extensibility Web Application Principal",
                      "appId": "40ed0677-9e6f-435c-b198-7634beba3874",
                      "appOwnerOrganizationId": "60df2466-a102-404a-8d9d-95c950626730",
                      "appRoleAssignmentRequired": false,
                      "createdDateTime": "2021-05-12T10:22:37Z",
                      "displayName": "SharePoint Online Client Extensibility Web Application Principal",
                      "notificationEmailAddresses": [],
                      "replyUrls": [
                        "https://fluidpreview.office.net/spfxsinglesignon",
                        "https://dev.fluidpreview.office.net/spfxsinglesignon",
                        "https://loitzl2-admin.sharepoint.com/_forms/spfxsinglesignon.aspx",
                        "https://loitzl2.sharepoint.com/",
                        "https://loitzl2.sharepoint.com/_forms/spfxsinglesignon.aspx",
                        "https://loitzl2.sharepoint.com/_forms/spfxsinglesignon.aspx?redirect"
                      ],
                      "servicePrincipalNames": [
                        "api://60df2466-a102-404a-8d9d-95c950626730/microsoft.spfx3rdparty.com",
                        "40ed0677-9e6f-435c-b198-7634beba3874"
                      ],
                      "servicePrincipalType": "Application",
                      "signInAudience": "AzureADMyOrg",
                      "tags": [],
                      "addIns": [],
                      "appRoles": [],
                      "info": {},
                      "keyCredentials": [],
                      "oauth2PermissionScopes": [
                        {
                          "adminConsentDescription": "Allow the application to access SharePoint Online Client Extensibility Web Application Principal on behalf of the signed-in user.",
                          "adminConsentDisplayName": "Access SharePoint Online Client Extensibility Web Application Principal",
                          "id": "80908136-1ead-4acd-acdb-c006ba6072bd",
                          "isEnabled": true,
                          "type": "User",
                          "userConsentDescription": "Allow the application to access SharePoint Online Client Extensibility Web Application Principal on your behalf.",
                          "userConsentDisplayName": "Access SharePoint Online Client Extensibility Web Application Principal",
                          "value": "user_impersonation"
                        }
                      ],
                      "passwordCredentials": [],
                      "resourceSpecificApplicationPermissions": [],
                      "verifiedPublisher": {}
                    }
                  ]
                }
                */

                #endregion

                JsonElement root = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");

                if (root.ValueKind != JsonValueKind.Array)
                {
                    return String.Empty;
                }

                return root
                    .EnumerateArray()
                    .Select(principal => principal.GetProperty("id").GetString())
                    .FirstOrDefault();
            }

            async Task<List<string>> GetSharePointAppPrincipalPermissionGrants(string id)
            {
                var getOAuth2PermissionGrantsCall =
                    new ApiCall(
                        $"oAuth2PermissionGrants/?$filter=clientId eq '{id}' and consentType eq 'AllPrincipals'",
                        ApiType.Graph);

                var result = await (tenantAdminContext.Web as Web)
                    .RawRequestAsync(getOAuth2PermissionGrantsCall, HttpMethod.Get)
                    .ConfigureAwait(false);

                #region JsonResponse

                /*
                {
                  "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#oauth2PermissionGrants",
                  "value": [
                    {
                      "clientId": "9b994571-2cf2-4fa4-acd0-9fe1f271410f",
                      "consentType": "AllPrincipals",
                      "id": "cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U",
                      "resourceId": "19cef431-469f-4898-8e72-dfd13054b3f5",
                      "scope": "User.ReadBasic.All"
                    }
                  ]
                } 
                */

                #endregion

                var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");

                if (root.ValueKind != JsonValueKind.Array)
                {
                    return new List<string>();
                }
                
                return root
                    .EnumerateArray()
                    .Select(element => element.GetProperty("scope").GetString())
                    .ToList();
            }
        }

        public async Task<IServicePrincipalProperties> EnableAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            SetServicePrincipalRequest request = new() {Enabled = true};

            ApiCall requestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(requestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IServicePrincipalProperties)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public IServicePrincipalProperties Enable(VanityUrlOptions vanityUrlOptions = null)
        {
            return EnableAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IServicePrincipalProperties> DisableAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            SetServicePrincipalRequest request = new() {Enabled = false};

            ApiCall requestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(requestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IServicePrincipalProperties)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public IServicePrincipalProperties Disable(VanityUrlOptions vanityUrlOptions = null)
        {
            return DisableAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<IPermissionGrant>> ListGrantsAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            ListGrantsRequest request = new();

            ApiCall requestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(requestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IEnumerable<IPermissionGrant>)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public IEnumerable<IPermissionGrant> ListGrants(VanityUrlOptions vanityUrlOptions = null)
        {
            return ListGrantsAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IPermissionGrant> AddGrantAsync(string resource, string scope,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            AddGrantRequest request = new() {Resource = resource, Scope = scope};

            ApiCall requestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(requestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IPermissionGrant)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public IPermissionGrant AddGrant(string resource, string scope, VanityUrlOptions vanityUrlOptions = null)
        {
            return AddGrantAsync(resource, scope, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IPermissionGrant> RevokeGrantAsync(string objectId, VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            RevokeGrantRequest request = new() {ObjectId = objectId};

            ApiCall requestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(requestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (IPermissionGrant)csomResult.ApiCall.CSOMRequests[0].Result;
        }

        public IPermissionGrant RevokeGrant(string objectId, VanityUrlOptions vanityUrlOptions = null)
        {
            return RevokeGrantAsync(objectId, vanityUrlOptions).GetAwaiter().GetResult();
        }
    }
}
