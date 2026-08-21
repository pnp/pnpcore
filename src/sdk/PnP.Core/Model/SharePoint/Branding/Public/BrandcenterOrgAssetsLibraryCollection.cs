using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a collection of organizational assets libraries in the Brand Center configuration.
    /// </summary>
    public class BrandcenterOrgAssetsLibraryCollection
    {
        /// <summary>
        /// Gets or sets the list of organizational assets libraries.
        /// </summary>
        public List<BrandcenterOrgAssetsLibrary> OrgAssetsLibraries { get; set; }

        /// <summary>
        /// Gets or sets the collection of organizational assets libraries.
        /// </summary>
        public List<BrandcenterOrgAssetsLibrary> Items { get; set; }

    }
}
