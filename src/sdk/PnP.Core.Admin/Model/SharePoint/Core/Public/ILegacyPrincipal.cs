using PnP.Core.Model.SharePoint;
using System.Collections.Generic;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines a legacy principal used by either Azure ACS or SharePoint AddIns
    /// </summary>
    public interface ILegacyPrincipal
    {
        /// <summary>
        /// Identifier of the legacy principal
        /// </summary>
        string AppIdentifier { get; }

        /// <summary>
        /// The server relative url of the <see cref="IWeb"/> where the legacy principal is located
        /// </summary>
        string ServerRelativeUrl { get; }

        /// <summary>
        /// Title of the legacy principal
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Can this legacy principal use app-only (application permissions)?
        /// </summary>
        bool AllowAppOnly { get; }

        /// <summary>
        /// List of site collection scoped permissions for this legacy principal
        /// </summary>
        IEnumerable<ILegacySiteCollectionPermission> SiteCollectionScopedPermissions { get; }

        /// <summary>
        /// List of tenant scoped permissions for this legacy principal
        /// </summary>
        IEnumerable<ILegacyTenantPermission> TenantScopedPermissions { get; }

    }
}
