using System;

namespace PnP.Core.Admin.Model.SharePoint
{
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
