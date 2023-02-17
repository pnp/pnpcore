using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    public interface IServicePrincipal
    {
        Task<List<IPermissionRequest>> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null);
        Task<IPermissionGrant> ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);
        Task DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);
    }
}
