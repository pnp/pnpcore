using System;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class BrandcenterConfiguration : IBrandcenterConfiguration
    {
        public BrandcenterConfiguration()
        {
            BrandColorsListId = Guid.Empty;
            BrandColorsListUrl = null;
            BrandFontLibraryId = Guid.Empty;
            BrandFontLibraryUrl = null;
            IsBrandCenterSiteFeatureEnabled = false;
            IsPublicCdnEnabled = false;
            OrgSkillsLibraryId = Guid.Empty;
            OrgSkillsLibraryUrl = null;
            SiteId = Guid.Empty;
            SiteUrl = null;
            OrgAssets = new BrandcenterOrgAssets
            {
                Domain = null,
                SiteId = Guid.Empty,
                WebId = Guid.Empty,
                Url = null
            };
        }

        public Guid BrandColorsListId { get; set; }

        public BrandcenterSPResourcePath BrandColorsListUrl { get; set; }

        public Guid BrandFontLibraryId { get; set; }

        public BrandcenterSPResourcePath BrandFontLibraryUrl { get; set; }

        public bool IsBrandCenterSiteFeatureEnabled { get; set; }

        public bool IsPublicCdnEnabled { get; set; }

        public Guid OrgSkillsLibraryId { get; set; }

        public BrandcenterSPResourcePath OrgSkillsLibraryUrl { get; set; }

        public Guid SiteId { get; set; }

        public string SiteUrl { get; set; }

        public BrandcenterOrgAssets OrgAssets { get; set; }
    }
}
