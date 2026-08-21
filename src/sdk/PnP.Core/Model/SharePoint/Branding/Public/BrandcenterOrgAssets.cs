using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the organizational assets in the Brand Center configuration.
    /// </summary>
    public class BrandcenterOrgAssets
    {
        /// <summary>
        /// Gets the domain of the organizational assets.
        /// </summary>
        public BrandcenterSPResourcePath Domain { get; set; }
        
        /// <summary>
        /// Gets the site ID of the organizational assets.
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Gets the web ID of the organizational assets.
        /// </summary>
        public Guid WebId { get; set; }

        /// <summary>
        /// Gets the relative URL of the organizational assets.
        /// </summary>
        public BrandcenterSPResourcePath Url { get; set; }

        /// <summary>
        /// Gets the collection of organizational assets libraries.
        /// </summary>
        public BrandcenterOrgAssetsLibraryCollection OrgAssetsLibraries { get; set; }

    }
}
