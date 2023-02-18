using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// A permission request on the SharePoint apps principal
    /// </summary>
    public interface IPermissionRequest
    {
        /// <summary>
        /// The id
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Domain Isolation
        /// </summary>
        bool IsDomainIsolated { get; set; }
        /// <summary>
        /// The isolated domain url
        /// </summary>
        string IsolatedDomainUrl { get; set; }
        /// <summary>
        /// App id of a multi-tenant app
        /// </summary>
        string MultiTenantAppId { get; set; }
        /// <summary>
        /// The reply url of the multi-tenant app
        /// </summary>
        string MultiTenantAppReplyUrl { get; set; }
        /// <summary>
        /// Name of the package approver
        /// </summary>
        string PackageApproverName { get; set; }
        /// <summary>
        /// Name of the package
        /// </summary>
        string PackageName { get; set; }
        /// <summary>
        /// Version of the package
        /// </summary>
        string PackageVersion { get; set; }
        /// <summary>
        /// The requested resource
        /// </summary>
        string Resource { get; set; }
        /// <summary>
        /// Id of the requested resource
        /// </summary>
        string ResourceId { get; set; }
        /// <summary>
        /// Permission scope
        /// </summary>
        string Scope { get; set; }
        /// <summary>
        /// Requested timestamp
        /// </summary>
        DateTime TimeRequested { get; set; }
    }
}
