using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Web object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Web))]
    public interface IWeb : IDataModel<IWeb>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// The Unique ID of the Web object
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Defines whether the site has to be crawled or not
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// The email address to which any access request will be sent
        /// </summary>
        public string RequestAccessEmail { get; set; }

        /// <summary>
        /// Defines the Welcome Page (Home Page) of the site to which the Provisioning Template is applied.
        /// </summary>
        public string WelcomePage { get; set; }

        /// <summary>
        /// The Title of the Site, optional attribute.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Description of the Site, optional attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The SiteLogo of the Site, optional attribute.
        /// </summary>
        public string SiteLogo { get; set; }

        /// <summary>
        /// The AlternateCSS of the Site, optional attribute.
        /// </summary>
        public string AlternateCSS { get; set; }

        /// <summary>
        /// The MasterPage Url of the Site, optional attribute.
        /// </summary>
        public string MasterPageUrl { get; set; }

        /// <summary>
        /// The Custom MasterPage Url of the Site, optional attribute.
        /// </summary>
        public string CustomMasterPageUrl { get; set; }

        /// <summary>
        /// Defines whether the comments on site pages are disabled or not
        /// </summary>
        public bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Enables or disables the QuickLaunch for the site
        /// </summary>
        public bool QuickLaunchEnabled { get; set; }

        /// <summary>
        /// Defines whether to enable Multilingual capabilities for the current web
        /// </summary>
        public bool IsMultilingual { get; set; }

        /// <summary>
        /// Defines whether to OverwriteTranslationsOnChange on change for the current web
        /// </summary>
        public bool OverwriteTranslationsOnChange { get; set; }

        /// <summary>
        /// Defines whether to exclude the web from offline client
        /// </summary>
        public bool ExcludeFromOfflineClient { get; set; }

        /// <summary>
        /// Defines whether members can share content from the current web
        /// </summary>
        public bool MembersCanShare { get; set; }

        /// <summary>
        /// Defines whether disable flows for the current web
        /// </summary>
        public bool DisableFlows { get; set; }

        /// <summary>
        /// Defines whether disable PowerApps for the current web
        /// </summary>
        public bool DisableAppViews { get; set; }

        /// <summary>
        /// Defines whether to enable the Horizontal QuickLaunch for the current web
        /// </summary>
        public bool HorizontalQuickLaunch { get; set; }

        /// <summary>
        /// Defines the SearchScope for the site
        /// </summary>
        public SearchScopes SearchScope { get; set; }

        /// <summary>
        /// Define if the suitebar search box should show or not 
        /// </summary>
        public SearchBoxInNavBar SearchBoxInNavBar { get; set; }

        /// <summary>
        /// Defines the Search Center URL
        /// </summary>
        public string SearchCenterUrl { get; set; }

        /// <summary>
        /// The URL of the Web object
        /// </summary>
        public Uri Url { get; }


        /// <summary>
        /// Collection of lists in the current Web object
        /// </summary>
        public IListCollection Lists { get; }

        /// <summary>
        /// Collection of content types in the current Web object
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        /// <summary>
        /// Collection of fields in the current Web object
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// Collection of webs in this current web
        /// </summary>
        public IWebCollection Webs { get; }

        /// <summary>
        /// Collection of features enabled for the web
        /// </summary>
        public IFeatureCollection Features { get; }
    }
}
