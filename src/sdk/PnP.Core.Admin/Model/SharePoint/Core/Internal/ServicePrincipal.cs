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

        protected enum UpdateGrantAction
        {
            Add,
            Remove
        }

        internal ServicePrincipal(PnPContext context)
        {
            this.context = context;
        }

        #region SPOWebAppServicePrincipal CSOM based grants API

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

            List<IPermissionRequest> permissionRequests =
                (List<IPermissionRequest>)csomResult.ApiCall.CSOMRequests[0].Result;

            string appPrincipalId =
                await GetAppPrincipalIdAsync(
                        "SharePoint Online Client Extensibility Web Application Principal",
                        vanityUrlOptions)
                    .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(appPrincipalId))
            {
                return permissionRequests;
            }

            var alreadyGranted =
                await ListGrants2Async(vanityUrlOptions).ConfigureAwait(false);

            var filtered = new List<IPermissionRequest>();

            foreach (IPermissionRequest permissionRequest in permissionRequests)
            {
                // Are scopes for this resource already granted?
                var alreadyGrantedCandidate =
                    alreadyGranted.FirstOrDefault(grant =>
                        grant.ResourceName.Equals(permissionRequest.Resource,
                            StringComparison.InvariantCultureIgnoreCase));

                if (alreadyGrantedCandidate == null)
                {
                    // No: add permissionRequest
                    filtered.Add(permissionRequest);
                    continue;
                }

                // Yes: check if the scope was already granted
                if (!alreadyGrantedCandidate.Scope.Contains(permissionRequest.Scope,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    // No: add permissionRequest
                    filtered.Add(permissionRequest);
                }
            }

            return filtered;
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

        #endregion

        #region Graph based grants API

        public async Task<IPermissionGrant2> AddGrant2Async(string resource, string scope,
            VanityUrlOptions vanityUrlOptions = null)
        {
            // get id for resource
            string resourcePrincipalId = await GetAppPrincipalIdAsync(resource, vanityUrlOptions).ConfigureAwait(false);

            // get existing grants for resource
            var grants = await ListGrants2Async(vanityUrlOptions).ConfigureAwait(false);

            IPermissionGrant2 result = null;
            var existingGrant = grants.FirstOrDefault(grant => grant.ResourceId.Equals(resourcePrincipalId));
            if (existingGrant != null)
            {
                // update    
                result = await UpdateGrantAsync(existingGrant, scope, UpdateGrantAction.Add, vanityUrlOptions)
                    .ConfigureAwait(false);
            }
            else
            {
                var sharePointAppPrincipalId =
                    await GetAppPrincipalIdAsync(
                        "SharePoint Online Client Extensibility Web Application Principal",
                        vanityUrlOptions).ConfigureAwait(false);
                // create
                result = await CreateGrantAsync(sharePointAppPrincipalId, resourcePrincipalId, scope, vanityUrlOptions)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public IPermissionGrant2 AddGrant2(string resource, string scope, VanityUrlOptions vanityUrlOptions = null)
        {
            return AddGrant2Async(resource, scope, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public Task<IServicePrincipalProperties> Disable2Async(VanityUrlOptions vanityUrlOptions = null)
        {
            return UpdateServicePrincipalAsync(false, vanityUrlOptions);
        }

        public IServicePrincipalProperties Disable2(VanityUrlOptions vanityUrlOptions = null)
        {
            return Disable2Async(vanityUrlOptions).GetAwaiter().GetResult();
        }
        
        public Task<IServicePrincipalProperties> Enable2Async(VanityUrlOptions vanityUrlOptions = null)
        {
            return UpdateServicePrincipalAsync(true, vanityUrlOptions);
        }

        public IServicePrincipalProperties Enable2(VanityUrlOptions vanityUrlOptions = null)
        {
            return Enable2Async(vanityUrlOptions).GetAwaiter().GetResult();
        }
        
        public IPermissionGrant2[] ListGrants2(VanityUrlOptions vanityUrlOptions = null)
        {
            return ListGrants2Async(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<IPermissionGrant2[]> ListGrants2Async(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            string sharePointAppPrincipalId =
                await GetAppPrincipalIdAsync(
                        "SharePoint Online Client Extensibility Web Application Principal",
                        vanityUrlOptions)
                    .ConfigureAwait(false);

            var getOAuth2PermissionGrantsCall =
                new ApiCall(
                    $"oAuth2PermissionGrants/?$filter=clientId eq '{sharePointAppPrincipalId}' and consentType eq 'AllPrincipals'",
                    ApiType.GraphBeta);

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
                return Array.Empty<IPermissionGrant2>();
            }

            var permissionGrants = root
                .EnumerateArray()
                .Select(element => new PermissionGrant2
                {
                    ClientId = element.GetProperty("clientId").GetString(),
                    ConsentType = element.GetProperty("consentType").GetString(),
                    Id = element.GetProperty("id").GetString(),
                    Scope = element.GetProperty("scope").GetString(),
                    ResourceId = element.GetProperty("resourceId").GetString(),
                    StartTime = element.GetProperty("startTime").GetDateTime(),
                    ExpiryTime = element.GetProperty("expiryTime").GetDateTime()
                });

            // enrich permissionGrants ResourceNames with appDisplayName
            var enriched = permissionGrants
                .Select(async grant =>
                    grant with
                    {
                        ResourceName =
                        await GetAppPrincipalNameAsync(grant.ResourceId, vanityUrlOptions).ConfigureAwait(false)
                    })
                .Select(t => t.Result);

            return enriched.Cast<IPermissionGrant2>().ToArray();
        }

        public async Task<IPermissionGrant2> RevokeGrant2Async(string grantId, string scope,
            VanityUrlOptions vanityUrlOptions = null)
        {
            // fetch grant
            IPermissionGrant2 grant = await GetGrantAsync(grantId, vanityUrlOptions).ConfigureAwait(false);
            // remove 'scope' from grant
            return await UpdateGrantAsync(grant, scope, UpdateGrantAction.Remove, vanityUrlOptions)
                .ConfigureAwait(false);
        }

        public IPermissionGrant2 RevokeGrant2(string grantId, string scope,
            VanityUrlOptions vanityUrlOptions = null)
        {
            return RevokeGrant2Async(grantId, scope, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public Task DeleteGrant2Async(
            string grantId,
            VanityUrlOptions vanityUrlOptions = null)
        {
            // delete grant
            return DeleteGrantAsync(grantId, vanityUrlOptions);
        }

        public void DeleteGrant2(
            string grantId,
            VanityUrlOptions vanityUrlOptions = null)
        {
            DeleteGrant2Async(grantId, vanityUrlOptions).GetAwaiter().GetResult();
        }

        #region Helpers

        private async Task<string> GetAppPrincipalIdAsync(string appPrincipalName,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            ApiCall getPrincipalIdCall =
                new ApiCall(
                    $"servicePrincipals/?$filter=displayName eq '{appPrincipalName}'",
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

        private async Task<string> GetAppPrincipalNameAsync(string appPrincipalId,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            ApiCall getPrincipalIdCall =
                new ApiCall(
                    $"servicePrincipals/{appPrincipalId}",
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

            JsonElement root = JsonDocument.Parse(result.Json).RootElement;

            return root.ValueKind != JsonValueKind.Object
                ? String.Empty
                : root.GetProperty("appDisplayName").ToString();
        }

        private async Task<IServicePrincipalProperties> UpdateServicePrincipalAsync(bool active, VanityUrlOptions vanityUrlOptions = null)
        {
            var sharePointAppPrincipalId =
                await GetAppPrincipalIdAsync(
                    "SharePoint Online Client Extensibility Web Application Principal",
                    vanityUrlOptions).ConfigureAwait(false);

            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);
            
            var json = new {accountEnabled = active};
            var body = JsonSerializer.Serialize(json);
            
            var patchServicePrincipalsCall = new ApiCall(
                $"servicePrincipals/{sharePointAppPrincipalId}",
                ApiType.Graph,
                body);
            
            // no response
            await (tenantAdminContext.Web as Web)
                .RawRequestAsync(patchServicePrincipalsCall, new HttpMethod("PATCH"))
                .ConfigureAwait(false);
            
            var getServicePrincipalsCall = new ApiCall(
                $"servicePrincipals/{sharePointAppPrincipalId}",
                ApiType.Graph,
                body);

            var result = await (tenantAdminContext.Web as Web)
                .RawRequestAsync(getServicePrincipalsCall, HttpMethod.Get)
                .ConfigureAwait(false);

            #region JsonResponse
            
            /*
                {
                 "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#servicePrincipals/$entity",
                 "id": "9b994571-2cf2-4fa4-acd0-9fe1f271410f",
                 "deletedDateTime": null,
                 "accountEnabled": false,
                 "alternativeNames": [],
                 "appDisplayName": "SharePoint Online Client Extensibility Web Application Principal",
                 "appDescription": null,
                 "appId": "40ed0677-9e6f-435c-b198-7634beba3874",
                 "applicationTemplateId": null,
                 "appOwnerOrganizationId": "60df2466-a102-404a-8d9d-95c950626730",
                 "appRoleAssignmentRequired": false,
                 "createdDateTime": "2021-05-12T10:22:37Z",
                 "description": null,
                 "disabledByMicrosoftStatus": null,
                 "displayName": "SharePoint Online Client Extensibility Web Application Principal",
                 "homepage": null,
                 "loginUrl": null,
                 "logoutUrl": null,
                 "notes": null,
                 "notificationEmailAddresses": [],
                 "preferredSingleSignOnMode": null,
                 "preferredTokenSigningKeyThumbprint": null,
                 "replyUrls": [
                   "https://loitzl2.sharepoint.com/",
                   "https://loitzl2-admin.sharepoint.com/_forms/spfxsinglesignon.aspx",
                   "https://dev.fluidpreview.office.net/spfxsinglesignon",
                   "https://fluidpreview.office.net/spfxsinglesignon",
                   "https://loitzl2.sharepoint.com/_forms/spfxmsalv3singlesignon.aspx",
                   "https://loitzl2.sharepoint.com/_forms/spfxsinglesignon.aspx?redirect",
                   "https://loitzl2.sharepoint.com/_forms/spfxsinglesignon.aspx",
                   "https://loitzl2.sharepoint.com"
                 ],
                 "servicePrincipalNames": [
                   "api://60df2466-a102-404a-8d9d-95c950626730/loitzl2.sharepoint.com",
                   "api://60df2466-a102-404a-8d9d-95c950626730/microsoft.spfx3rdparty.com",
                   "40ed0677-9e6f-435c-b198-7634beba3874"
                 ],
                 "servicePrincipalType": "Application",
                 "signInAudience": "AzureADMyOrg",
                 "tags": [],
                 "tokenEncryptionKeyId": null,
                 "samlSingleSignOnSettings": null,
                 "addIns": [],
                 "appRoles": [],
                 "info": {
                   "logoUrl": null,
                   "marketingUrl": null,
                   "privacyStatementUrl": null,
                   "supportUrl": null,
                   "termsOfServiceUrl": null
                 },
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
                 "verifiedPublisher": {
                   "displayName": null,
                   "verifiedPublisherId": null,
                   "addedDateTime": null
                 }
               }
            */            
            #endregion

            var root = JsonDocument.Parse(result.Json).RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                var servicePrincipalProperties = new ServicePrincipalProperties
                {
                    AccountEnabled = root.GetProperty("accountEnabled").GetBoolean(),
                    AppId = root.GetProperty("appId").GetString(),
                    ReplyUrls = root.GetProperty("replyUrls").EnumerateArray().Select( r => r.GetString())
                };
                return servicePrincipalProperties;
            }

            return null;
        }
        
        #endregion

        # region CRUD Operations
        
        private async Task<IPermissionGrant2> CreateGrantAsync(
            string sharePointAppPrincipalId,
            string resourceId,
            string scope,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            var json = new
            {
                clientId = sharePointAppPrincipalId, // SharePoint Online Client Extensibility Web Application Principal
                consentType = "AllPrincipals",
                startTime = DateTime.MinValue.ToString("o"),
                expiryTime = DateTime.MaxValue.ToString("o"),
                resourceId, // e.g. Microsoft Graph
                scope = string.Join(" ",
                    scope
                        .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct()
                        .ToArray())
            };

            var body = JsonSerializer.Serialize(json);

            var postOAuth2PermissionGrantCall = new ApiCall(
                $"oAuth2PermissionGrants",
                ApiType.GraphBeta,
                body);

            var result = await (tenantAdminContext.Web as Web)
                .RawRequestAsync(postOAuth2PermissionGrantCall, HttpMethod.Post)
                .ConfigureAwait(false);

            #region JsonResponse

            /*
               {
                 "@odata.context": "https://graph.microsoft.com/beta/$metadata#oauth2PermissionGrants/$entity",
                 "clientId": "9b994571-2cf2-4fa4-acd0-9fe1f271410f",
                 "consentType": "AllPrincipals",
                 "expiryTime": "9999-12-31T23:59:59.9999999Z",
                 "id": "cUWZm_IspE-s0J_h8nFBD2oqyUTxyRZHpEatDPcYwsQ",
                 "principalId": null,
                 "resourceId": "44c92a6a-c9f1-4716-a446-ad0cf718c2c4",
                 "scope": "access_as_user Scope2",
                 "startTime": null
               }
            */

            #endregion

            var root = JsonDocument.Parse(result.Json).RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                var resultGrant = new PermissionGrant2()
                {
                    ClientId = root.GetProperty("clientId").GetString(),
                    ConsentType = root.GetProperty("consentType").GetString(),
                    ExpiryTime = root.GetProperty("expiryTime").GetDateTime(),
                    Id = root.GetProperty("id").GetString(),
                    Scope = root.GetProperty("scope").GetString(),
                    StartTime = root.GetProperty("startTime").GetDateTime(),
                    ResourceId = root.GetProperty("resourceId").GetString()
                };

                return resultGrant with
                {
                    ResourceName = await GetAppPrincipalNameAsync(resultGrant.ResourceId, vanityUrlOptions)
                        .ConfigureAwait(false)
                };
            }

            return null;
        }

        private async Task<IPermissionGrant2> GetGrantAsync(string existingGrantId,
            VanityUrlOptions vanityUrlOptions)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            var getOAuth2PermissionGrantCall =
                new ApiCall(
                    $"oAuth2PermissionGrants/{existingGrantId}",
                    ApiType.GraphBeta);

            var result = await (tenantAdminContext.Web as Web)
                .RawRequestAsync(getOAuth2PermissionGrantCall, HttpMethod.Get)
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

            var root = JsonDocument.Parse(result.Json).RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                var resultGrant = new PermissionGrant2()
                {
                    ClientId = root.GetProperty("clientId").GetString(),
                    ConsentType = root.GetProperty("consentType").GetString(),
                    ExpiryTime = root.GetProperty("expiryTime").GetDateTime(),
                    Id = root.GetProperty("id").GetString(),
                    Scope = root.GetProperty("scope").GetString(),
                    StartTime = root.GetProperty("startTime").GetDateTime(),
                    ResourceId = root.GetProperty("resourceId").GetString()
                };


                return resultGrant with
                {
                    ResourceName = await GetAppPrincipalNameAsync(resultGrant.ResourceId, vanityUrlOptions)
                        .ConfigureAwait(false)
                };
            }

            return null;
        }

        private async Task<IPermissionGrant2> UpdateGrantAsync(
            IPermissionGrant2 existingGrant,
            string scope,
            UpdateGrantAction updateGrantAction,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            // join existingGrant.Scope with scope(s)
            var scopes = existingGrant.Scope.Split(' ').ToList();

            switch (updateGrantAction)
            {
                // scope can be a single scope, or a list of scopes separated by spaces
                case UpdateGrantAction.Add:
                    scopes.AddRange(
                        scope.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                    break;
                case UpdateGrantAction.Remove:
                    scopes = scopes
                        .Except(scope.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                        .ToList();
                    break;
            }

            if (scopes.Count == 0)
            {
                // no scopes left, delete the whole oAuth2PermissionGrants item
                await DeleteGrant2Async(existingGrant.Id, vanityUrlOptions).ConfigureAwait(false);
                return null;
            }

            // patch with new scopes
            var json = new {scope = string.Join(" ", scopes.Distinct().ToArray())};
            var body = JsonSerializer.Serialize(json);

            var patchOAuth2PermissionGrantCall = new ApiCall(
                $"oAuth2PermissionGrants/{existingGrant.Id}",
                ApiType.GraphBeta,
                body);

            // no response
            await (tenantAdminContext.Web as Web)
                .RawRequestAsync(patchOAuth2PermissionGrantCall, new HttpMethod("PATCH"))
                .ConfigureAwait(false);

            var getOAuth2PermissionGrantCall = new ApiCall(
                $"oAuth2PermissionGrants/{existingGrant.Id}",
                ApiType.GraphBeta,
                body);

            var result = await (tenantAdminContext.Web as Web)
                .RawRequestAsync(getOAuth2PermissionGrantCall, HttpMethod.Get)
                .ConfigureAwait(false);

            #region JsonResponse

            /*
               {
                 "@odata.context": "https://graph.microsoft.com/beta/$metadata#oauth2PermissionGrants/$entity",
                 "clientId": "9b994571-2cf2-4fa4-acd0-9fe1f271410f",
                 "consentType": "AllPrincipals",
                 "expiryTime": "9999-12-31T23:59:59.9999999Z",
                 "id": "cUWZm_IspE-s0J_h8nFBDzH0zhmfRphIjnLf0TBUs_U",
                 "principalId": null,
                 "resourceId": "19cef431-469f-4898-8e72-dfd13054b3f5",
                 "scope": "User.ReadBasic.All Calendars.Read Application.ReadWrite.All",
                 "startTime": "0001-01-01T00:00:00Z"
               }
            */

            #endregion

            var root = JsonDocument.Parse(result.Json).RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                var resultGrant = new PermissionGrant2()
                {
                    ClientId = root.GetProperty("clientId").GetString(),
                    ConsentType = root.GetProperty("consentType").GetString(),
                    ExpiryTime = root.GetProperty("expiryTime").GetDateTime(),
                    Id = root.GetProperty("id").GetString(),
                    Scope = root.GetProperty("scope").GetString(),
                    StartTime = root.GetProperty("startTime").GetDateTime(),
                    ResourceId = root.GetProperty("resourceId").GetString()
                };

                return resultGrant with
                {
                    ResourceName = await GetAppPrincipalNameAsync(resultGrant.ResourceId, vanityUrlOptions)
                        .ConfigureAwait(false)
                };
            }

            return null;
        }

        private async Task DeleteGrantAsync(string grantId,
            VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);

            var deleteOAuth2PermissionGrantCall = new ApiCall(
                $"oAuth2PermissionGrants/{grantId}",
                ApiType.Graph);

            // no response
            await (tenantAdminContext.Web as Web)
                .RawRequestAsync(deleteOAuth2PermissionGrantCall, HttpMethod.Delete)
                .ConfigureAwait(false);
        }

        #endregion
        
        #endregion
    }
}