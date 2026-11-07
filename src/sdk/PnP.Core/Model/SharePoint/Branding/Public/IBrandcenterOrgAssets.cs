using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the organizational assets in the Brand Center configuration.
    /// </summary>
    public interface IBrandcenterOrgAssets
    {
        /// <summary>
        /// Gets the domain of the organizational assets.
        /// </summary>
        IBrandcenterSPResourcePath Domain { get; }

        /// <summary>
        /// Gets the site ID of the organizational assets.
        /// </summary>
        Guid SiteId { get; }

        /// <summary>
        /// Gets the web ID of the organizational assets.
        /// </summary>
        Guid WebId { get; }

        /// <summary>
        /// Gets the relative URL of the organizational assets.
        /// </summary>
        IBrandcenterSPResourcePath Url { get; }

        /// <summary>
        /// Gets the collection of organizational assets libraries.
        /// </summary>
        IBrandcenterOrgAssetsLibraryCollection OrgAssetsLibraries { get; }
    }
}
