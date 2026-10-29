using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents an organizational assets library in the Brand Center configuration.
    /// </summary>
    public class BrandcenterOrgAssetsLibrary
    {
        /// <summary>
        /// Gets or sets the display name of the organizational assets library.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the list ID of the organizational assets library.
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// Gets or sets the library URL of the organizational assets library.
        /// </summary>
        public BrandcenterSPResourcePath LibraryUrl { get; set; }

        /// <summary>
        /// Gets or sets the flags associated with the organizational assets library.
        /// </summary>
        public int OrgAssetFlags { get; set; }

        /// <summary>
        /// Gets or sets the type of the organizational assets library. 1 = ImageDocumentLibrary , 2 = OfficeTemplateLibrary, 8 = Fonts, 32 = _catalogs/brandcolors
        /// </summary>
        public int OrgAssetType { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the organizational assets library.
        /// </summary>
        public Guid UniqueId { get; set; }
    }
}
