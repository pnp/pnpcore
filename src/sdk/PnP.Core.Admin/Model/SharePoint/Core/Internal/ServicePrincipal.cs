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
        private readonly PnPContext _context;

        internal ServicePrincipal(PnPContext context)
        {
            _context = context;
        }
        
        public IPermissionGrant ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            return ApprovePermissionRequestAsync(id, vanityUrlOptions).GetAwaiter().GetResult();
        }
        
        public async Task<IPermissionGrant> ApprovePermissionRequestAsync(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await _context
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
            using PnPContext tenantAdminContext = await _context
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
            using PnPContext tenantAdminContext = await _context
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

        public async Task<IServicePrincipalProperties> Enable(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await _context
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

        public async Task<IServicePrincipalProperties> Disable(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await _context
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
    }
}
