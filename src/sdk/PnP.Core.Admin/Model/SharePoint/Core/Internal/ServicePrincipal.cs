using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint.Core.Internal
{
    internal sealed class ServicePrincipal : IServicePrincipal
    {
        private readonly PnPContext _context;

        internal ServicePrincipal(PnPContext context)
        {
            _context = context;
        }

        public async Task<IPermissionGrant> ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
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

        public async Task DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await _context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);
           
            DenyPermissionRequest request = new() {RequestId = id};

            ApiCall getPermissionRequestsCall = new(
                new List<IRequest<object>> {request});
            
            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);
        }

        public async Task<List<IPermissionRequest>> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null)
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
    }
}
