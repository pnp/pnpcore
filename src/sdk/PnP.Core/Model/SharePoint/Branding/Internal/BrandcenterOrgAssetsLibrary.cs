using System;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class BrandcenterOrgAssetsLibrary : IBrandcenterOrgAssetsLibrary
    {
        public string DisplayName { get; set; }

        public Guid ListId { get; set; }

        public BrandcenterSPResourcePath LibraryUrl { get; set; }

        public int OrgAssetFlags { get; set; }

        public int OrgAssetType { get; set; }

        public Guid UniqueId { get; set; }

        IBrandcenterSPResourcePath IBrandcenterOrgAssetsLibrary.LibraryUrl => LibraryUrl;
    }
}
