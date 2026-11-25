using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a collection of organizational assets libraries in the Brand Center configuration.
    /// </summary>
    public interface IBrandcenterOrgAssetsLibraryCollection
    {
        /// <summary>
        /// Gets the list of organizational assets libraries.
        /// </summary>
        IEnumerable<IBrandcenterOrgAssetsLibrary> OrgAssetsLibraries { get; }

        /// <summary>
        /// Gets the collection of organizational assets libraries.
        /// </summary>
        IEnumerable<IBrandcenterOrgAssetsLibrary> Items { get; }
    }
}
