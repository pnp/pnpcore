using System;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class BrandcenterOrgAssets : IBrandcenterOrgAssets
    {
        public BrandcenterOrgAssets()
        {
            Domain = null;
            Url = null;
            SiteId = Guid.Empty;
            WebId = Guid.Empty;
            OrgAssetsLibraries = new BrandcenterOrgAssetsLibraryCollection();
        }

        public BrandcenterSPResourcePath Domain { get; set; }

        public Guid SiteId { get; set; }

        public Guid WebId { get; set; }

        public BrandcenterSPResourcePath Url { get; set; }

        public BrandcenterOrgAssetsLibraryCollection OrgAssetsLibraries { get; set; }

        IBrandcenterSPResourcePath IBrandcenterOrgAssets.Domain => Domain;

        IBrandcenterSPResourcePath IBrandcenterOrgAssets.Url => Url;

        IBrandcenterOrgAssetsLibraryCollection IBrandcenterOrgAssets.OrgAssetsLibraries => OrgAssetsLibraries;
    }
}
