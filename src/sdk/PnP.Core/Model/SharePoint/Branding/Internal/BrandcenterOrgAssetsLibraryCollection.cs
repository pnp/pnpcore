using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class BrandcenterOrgAssetsLibraryCollection : IBrandcenterOrgAssetsLibraryCollection
    {
        public BrandcenterOrgAssetsLibraryCollection()
        {
            OrgAssetsLibraries = new List<BrandcenterOrgAssetsLibrary>();
            Items = new List<BrandcenterOrgAssetsLibrary>();
        }

        public List<BrandcenterOrgAssetsLibrary> OrgAssetsLibraries { get; set; }

        public List<BrandcenterOrgAssetsLibrary> Items { get; set; }

        IEnumerable<IBrandcenterOrgAssetsLibrary> IBrandcenterOrgAssetsLibraryCollection.OrgAssetsLibraries => OrgAssetsLibraries;

        IEnumerable<IBrandcenterOrgAssetsLibrary> IBrandcenterOrgAssetsLibraryCollection.Items => Items;
    }
}
