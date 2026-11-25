using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents an organizational assets library in the Brand Center configuration.
    /// </summary>
    public interface IBrandcenterOrgAssetsLibrary
    {
        /// <summary>
        /// Gets the display name of the organizational assets library.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the list ID of the organizational assets library.
        /// </summary>
        Guid ListId { get; }

        /// <summary>
        /// Gets the library URL of the organizational assets library.
        /// </summary>
        IBrandcenterSPResourcePath LibraryUrl { get; }

        /// <summary>
        /// Gets the flags associated with the organizational assets library.
        /// </summary>
        int OrgAssetFlags { get; }

        /// <summary>
        /// Gets the type of the organizational assets library. 1 = ImageDocumentLibrary , 2 = OfficeTemplateLibrary, 8 = Fonts, 32 = _catalogs/brandcolors
        /// </summary>
        int OrgAssetType { get; }

        /// <summary>
        /// Gets the unique ID of the organizational assets library.
        /// </summary>
        Guid UniqueId { get; }
    }
}
