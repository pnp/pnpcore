using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System.Collections.Generic;
using System.Net.Http;
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

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (List<IPermissionRequest>)csomResult.ApiCall.CSOMRequests[0].Result;
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
