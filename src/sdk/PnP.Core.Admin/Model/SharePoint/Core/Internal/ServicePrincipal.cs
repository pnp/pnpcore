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

        public Task<bool> ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            throw new System.NotImplementedException();
        }

        public Task DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<IPermissionRequest>> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null)
        {
            using (var tenantAdminContext = await _context
                .GetSharePointAdmin()
                .GetTenantAdminCenterContextAsync(vanityUrlOptions)
                .ConfigureAwait(false))
            {

                var result = new List<IPermissionRequest>();
                var request = new GetPermissionRequestsRequest();

                ApiCall getPermissionRequestsCall = new ApiCall(
                    new List<IRequest<object>>()
                    {
                    request
                    });

                var csomResult = await (tenantAdminContext.Web as Web)
                        .RawRequestAsync(getPermissionRequestsCall, HttpMethod.Post)
                        .ConfigureAwait(false);
            }

            throw new System.NotImplementedException();
        }
    }
}