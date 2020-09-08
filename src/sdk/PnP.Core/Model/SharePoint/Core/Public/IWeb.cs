using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
        /// <summary>
        /// Gets the URL of the access request list to the current site.
        /// </summary>
        public string AccessRequestListUrl { get; }

        /// <summary>
        /// Gets or sets the description of the access request to this site.
        /// </summary>
        public string AccessRequestSiteDescription { get; set; }

        /// <summary>
        /// Gets a value that specifies whether the automatic ASPX page indexed is allowed.
        /// </summary>
        public bool AllowAutomaticASPXPageIndexing { get; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to create declarative workflow on this site.
        /// </summary>
        public bool AllowCreateDeclarativeWorkflowForCurrentUser { get; set; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to use a designer application to customize this site.
        /// </summary>
        public bool AllowDesignerForCurrentUser { get; set; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to edit the master page.
        /// </summary>
        public bool AllowMasterPageEditingForCurrentUser { get; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to revert the site to a default site template.
        /// </summary>
        public bool AllowRevertFromTemplateForCurrentUser { get; }

        /// <summary>
        /// Gets a value that specifies whether the site allows RSS feeds.
        /// </summary>
        public bool AllowRssFeeds { get; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to save declarative workflows as template.
        /// </summary>
        public bool AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser { get; }

        /// <summary>
        /// Gets a value that specifies whether the current user is allowed to publish a declarative workflow.
        /// </summary>
        public bool AllowSavePublishDeclarativeWorkflowForCurrentUser { get; }

        /// <summary>
        /// The instance Id of the App Instance that this website represents.
        /// </summary>
        public Guid AppInstanceId { get; }

        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
        /// <summary>
        /// Gets or sets the welcome page of the site in Classic UI mode.
        /// </summary>
        public string ClassicWelcomePage { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether this site contains confidential information.
        /// </summary>
        public bool ContainsConfidentialInfo { get; set; }

        /// <summary>
        /// Gets a value that specifies when the site was created.
        /// </summary>
        public DateTime Created { get; }

        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
        /// <summary>
        /// Gets a value that specifies whether custom site actions are disabled on this site.
        /// </summary>
        public bool CustomSiteActionsDisabled { get; set; }

        #region ON GOING
        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
        /// <summary>
        /// Gets or sets the default new page template Id of the site.
        /// </summary>
        public Guid DefaultNewPageTemplateId { get; set; }

        /// <summary>
        /// Gets the designer download URL for current user.
        /// </summary>
        public string DesignerDownloadUrlForCurrentUser { get; }

        /// <summary>
        /// Gets or sets the design package Id of this site.
        /// </summary>
        public Guid DesignPackageId { get; set; }

        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
        /// <summary>
        /// Gets or sets whether the recommended items are disabled on this site.
        /// </summary>
        public bool DisableRecommendedItems { get; set; }

        /// <summary>
        /// Determines if the Document Library Callout's WAC previewers are enabled or not.
        /// </summary>
        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get; }
        #endregion

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
        public SearchScope SearchScope { get; set; }

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

        /// <summary>
        /// Collection of folders in the current Web object
        /// </summary>
        public IFolderCollection Folders { get; }

        /// <summary>
        /// Gets a collection of metadata for the Web site.
        /// </summary>
        public IPropertyValues AllProperties { get; }

        /// <summary>
        /// Gets the collection of all content types that apply to the current scope, including those of the current Web site, as well as any parent Web sites.
        /// </summary>
        public IContentTypeCollection AvailableContentTypes { get; }

        /// <summary>
        /// Gets a value that specifies the collection of all fields available for the current scope, including those of the current site, as well as any parent sites.
        /// </summary>
        public IFieldCollection AvailableFields { get; }

        /// <summary>
        /// Gets the root folder for the website.
        /// </summary>
        public IFolder RootFolder { get; }

        /// <summary>
        /// Gets the UserInfo list of the site collection that contains the website.
        /// </summary>
        public IList SiteUserInfoList { get; }

        #region Methods

        #region GetFolderByServerRelativeUrl

        /// <summary>
        /// Get a folder in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);


        /// <summary>
        /// Get a folder in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions);

        #endregion

        #region GetFileByServerRelativeUrl

        /// <summary>
        /// Get a file in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);


        /// <summary>
        /// Get a file in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        #endregion

        #endregion

        #region TO IMPLEMENT
        // TODO: Take information from here to update documentation of this class member
        // TODO: https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee546309(v=office.15)

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool EnableMinimalDownload { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int FooterEmphasis { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool FooterEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int FooterLayout { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int HeaderEmphasis { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int HeaderLayout { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool HideTitleInHeader { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool IsHomepageModernized { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool IsProvisioningComplete { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool IsRevertHomepageLinkHidden { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int Language { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public DateTime LastItemModifiedDate { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public DateTime LastItemUserModifiedDate { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int LogoAlignment { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string MasterUrl { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool MegaMenuEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool NavAudienceTargetingEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool NextStepsFirstRunEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool NotificationsInOneDriveForBusinessEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool NotificationsInSharePointEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool ObjectCacheEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool PreviewFeaturesEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string PrimaryColor { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool RecycleBinEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool SaveSiteAsTemplateEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string SearchBoxPlaceholderText { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string ServerRelativeUrl { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool ShowUrlStructureForCurrentUser { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string SiteLogoDescription { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string SiteLogoUrl { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool SyndicationEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int TenantAdminMembersCanShare { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool TenantTagPolicyEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string ThemeData { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string ThemedCssFolderUrl { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool ThirdPartyMdmEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool TreeViewEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int UIVersion { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool UIVersionConfigurationEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool UseAccessRequestDefault { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string WebTemplate { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public string WebTemplateConfiguration { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public bool WebTemplatesGalleryFirstRunEnabled { get; set; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IActivityEntityCollection Activities { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IActivityLogger ActivityLogger { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IAlertCollection Alerts { get; }



        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IAppTileCollection AppTiles { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IGroup AssociatedMemberGroup { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IGroup AssociatedOwnerGroup { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IGroup AssociatedVisitorGroup { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser Author { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IModernizeHomepageResult CanModernizeHomepage { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IClientWebPartCollection ClientWebParts { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser CurrentUser { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IDataLeakagePreventionStatusInfo DataLeakagePreventionStatusInfo { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource DescriptionResource { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IEventReceiverDefinitionCollection EventReceivers { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IHostedAppsManager HostedApps { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IListTemplateCollection ListTemplates { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IMultilingualSettings MultilingualSettings { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public INavigation Navigation { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public ISharedDocumentInfoCollection OneDriveSharedItems { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWebInformation ParentWeb { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IPushNotificationSubscriberCollection PushNotificationSubscribers { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IRecycleBinItemCollection RecycleBin { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IRegionalSettings RegionalSettings { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IRoleDefinitionCollection RoleDefinitions { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public ISiteCollectionCorporateCatalogAccessor SiteCollectionAppCatalog { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IGroupCollection SiteGroups { get; }



        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserCollection SiteUsers { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public ITenantCorporateCatalogAccessor TenantAppCatalog { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IThemeInfo ThemeInfo { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserResource TitleResource { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserCustomActionCollection UserCustomActions { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWebInformationCollection WebInfos { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWorkflowAssociationCollection WorkflowAssociations { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IWorkflowTemplateCollection WorkflowTemplates { get; }

        #endregion
    }
}
