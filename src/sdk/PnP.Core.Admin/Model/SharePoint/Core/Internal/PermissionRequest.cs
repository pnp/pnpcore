using System;

namespace PnP.Core.Admin.Model.SharePoint
{
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
