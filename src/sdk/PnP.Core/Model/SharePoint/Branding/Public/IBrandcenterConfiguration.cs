using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Interface for /_api/brandcenter/configuration
    /// </summary>
    public interface IBrandcenterConfiguration
    {
        /// <summary>
        /// Gets the Brand Colors List Id
        /// </summary>
        public Guid BrandColorsListId { get; internal set; }

        /// <summary>
        /// Gets the Brand Colors List Url
        /// </summary>
        public BrandcenterSPResourcePath BrandColorsListUrl { get; internal set; }

        /// <summary>
        /// Gets the Brand Font Library Id
        /// </summary>
        public Guid BrandFontLibraryId { get; internal set; }

        /// <summary>
        /// Gets the Brand Font Library Url
        /// </summary>
        public BrandcenterSPResourcePath BrandFontLibraryUrl { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the Brand Center Site Feature is enabled
        /// </summary>
        public bool IsBrandCenterSiteFeatureEnabled { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the Public CDN is enabled
        /// </summary>
        public bool IsPublicCdnEnabled { get; internal set; }

        /// <summary>
        /// Gets the Org Skills Library Id
        /// </summary>
        public Guid OrgSkillsLibraryId { get; internal set; }

        /// <summary>
        /// Gets the Org Skills Library Url
        /// </summary>
        public BrandcenterSPResourcePath OrgSkillsLibraryUrl { get; internal set; }

        /// <summary>
        /// Gets the Site Id
        /// </summary>
        public Guid SiteId { get; internal set; }

        /// <summary>
        /// Gets the Site Url
        /// </summary>
        public string SiteUrl { get; internal set; }


        /// <summary>
        /// Gets the organizational assets
        /// </summary>
        public BrandcenterOrgAssets OrgAssets { get; internal set; }    
    }
}
