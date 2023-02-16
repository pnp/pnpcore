using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{

    public interface IServicePrincipal
    {
        Task<List<IPermissionRequest>> GetPermissionRequests(VanityUrlOptions vanityUrlOptions = null);
        Task<bool> ApprovePermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);
        Task DenyPermissionRequest(string id, VanityUrlOptions vanityUrlOptions = null);

    }

    public interface IPermissionRequest
    {
        Guid Id { get; set; }
        bool IsDomainIsolated { get; set; }
        string IsolatedDomainUrl { get; set; }
        string MultiTenantAppId { get; set; }
        string MultiTenantAppReplyUrl { get; set; }
        string PackageApproverName { get; set; }
        string PackageName { get; set; }
        string PackageVersion { get; set; }
        string Resource { get; set; }
        string ResourceId { get; set; }
        string Scope { get; set; }
        DateTime TimeRequested { get; set; }
    }


}
