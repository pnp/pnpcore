using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// A permission grant
    /// </summary>
    [Obsolete("Use IPermissionGrant2 instead")]
    public interface IPermissionGrant
    {
        /// <summary>
        /// ClientId
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Type of consent
        /// </summary>
        string ConsentType { get; set; }

        /// <summary>
        /// Domain Isolation
        /// </summary>
        bool  IsDomainIsolated { get; set; }

        /// <summary>
        /// The object id
        /// </summary>
        string ObjectId { get; set; }

        /// <summary>
        /// Name of the package
        /// </summary>
        string PackageName { get; set; }

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
    }
}
