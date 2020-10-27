using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a CompanyPortalContext object
    /// </summary>
    [ConcreteType(typeof(CompanyPortalContext))]
    public interface ICompanyPortalContext : IDataModel<ICompanyPortalContext>, IDataModelGet<ICompanyPortalContext>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int FooterEmphasis { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FooterLayout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string FooterNavigation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int HeaderEmphasis { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int HeaderLayout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string HubSiteData { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string HubSiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsAudienceTargeted { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsFooterEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsHubSite { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsMegaMenuEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string NavigationInfo { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteAcronym { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThemedCssFolderUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThemeInfo { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebAbsoluteUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebLogoBackgroundColor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebLogoUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebTitle { get; set; }

        #endregion

    }
}
