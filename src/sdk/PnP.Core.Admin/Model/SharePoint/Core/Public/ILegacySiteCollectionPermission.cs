using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the permissions a legacy principal (<see cref="ILegacyPrincipal"/>) has on a site collection
    /// </summary>
    public interface ILegacySiteCollectionPermission
    {
        /// <summary>
        /// Site collection for which the permissions is granted
        /// </summary>
        Guid SiteId { get; }

        /// <summary>
        /// Web for which the permissions is granted. If empty guid then the permissions is granted on the site collection level
        /// </summary>
        Guid WebId { get; }

        /// <summary>
        /// List for which the permissions is granted. If empty guid then the permissions is granted on the web or site collection level
        /// </summary>
        Guid ListId { get; }

        /// <summary>
        /// The granted right
        /// </summary>
        LegacySiteCollectionPermissionRight Right { get; }
    }
}
