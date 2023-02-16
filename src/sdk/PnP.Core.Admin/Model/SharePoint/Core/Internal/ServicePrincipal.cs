using PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
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

        public Task<bool> ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            throw new NotImplementedException();
        }

        public async Task<List<IPermissionRequest>> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null)
        {
            using PnPContext tenantAdminContext = await _context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false);
            List<IPermissionRequest> result = new();
            GetPermissionRequestsRequest request = new();

            ApiCall getPermissionRequestsCall = new(
                new List<IRequest<object>> {request});

            ApiCallResponse csomResult = await ((Web)tenantAdminContext.Web)
                .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                .ConfigureAwait(false);

            return (List<IPermissionRequest>)csomResult.ApiCall.CSOMRequests[0].Result;
        }
    }

    internal sealed class PermissionRequest : IPermissionRequest
    {
        public Guid Id { get; set; }
        public bool IsDomainIsolated { get; set; }
        public string IsolatedDomainUrl { get; set; }
        public string MultiTenantAppId { get; set; }
        public string MultiTenantAppReplyUrl { get; set; }
        public string PackageApproverName { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string Scope { get; set; }
        public DateTime TimeRequested { get; set; }
    }
}