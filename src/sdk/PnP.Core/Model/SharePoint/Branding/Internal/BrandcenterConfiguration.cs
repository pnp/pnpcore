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
            OrgAssets = new BrandcenterOrgAssets();
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

        IBrandcenterSPResourcePath IBrandcenterConfiguration.BrandColorsListUrl
        {
            get => BrandColorsListUrl;
            set => BrandColorsListUrl = (BrandcenterSPResourcePath)value;
        }

        IBrandcenterSPResourcePath IBrandcenterConfiguration.BrandFontLibraryUrl
        {
            get => BrandFontLibraryUrl;
            set => BrandFontLibraryUrl = (BrandcenterSPResourcePath)value;
        }

        IBrandcenterSPResourcePath IBrandcenterConfiguration.OrgSkillsLibraryUrl
        {
            get => OrgSkillsLibraryUrl;
            set => OrgSkillsLibraryUrl = (BrandcenterSPResourcePath)value;
        }

        IBrandcenterOrgAssets IBrandcenterConfiguration.OrgAssets
        {
            get => OrgAssets;
            set => OrgAssets = (BrandcenterOrgAssets)value;
        }
    }
}
