using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a CompanyPortalContext object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class CompanyPortalContext : BaseDataModel<ICompanyPortalContext>, ICompanyPortalContext
    {

        #region New properties

        public int FooterEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public int FooterLayout { get => GetValue<int>(); set => SetValue(value); }

        public string FooterNavigation { get => GetValue<string>(); set => SetValue(value); }

        public int HeaderEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public int HeaderLayout { get => GetValue<int>(); set => SetValue(value); }

        public string HubSiteData { get => GetValue<string>(); set => SetValue(value); }

        public string HubSiteId { get => GetValue<string>(); set => SetValue(value); }

        public bool IsAudienceTargeted { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsFooterEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHubSite { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMegaMenuEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string NavigationInfo { get => GetValue<string>(); set => SetValue(value); }

        public string SiteAcronym { get => GetValue<string>(); set => SetValue(value); }

        public string SiteId { get => GetValue<string>(); set => SetValue(value); }

        public string SiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public string ThemedCssFolderUrl { get => GetValue<string>(); set => SetValue(value); }

        public string ThemeInfo { get => GetValue<string>(); set => SetValue(value); }

        public string WebAbsoluteUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebLogoBackgroundColor { get => GetValue<string>(); set => SetValue(value); }

        public string WebLogoUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string WebTitle { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
