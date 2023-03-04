using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Web object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Web))]
    public interface IWeb : IDataModel<IWeb>, IDataModelGet<IWeb>, IDataModelLoad<IWeb>, IDataModelUpdate, IDataModelDelete, IDataModelSupportingGetChanges, ISecurableObject, IQueryableDataModel
    {
        #region Properties
        /// <summary>
        /// The Unique ID of the Web object
        /// </summary>
        public Guid Id { get; }

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
        public bool AllowAutomaticASPXPageIndexing { get; set; }

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

        /// <summary>
        /// Gets a boolean value that specifies whether the site contains highly confidential information.
        /// If the tenant settings don't allow tagging sites as confidential, this property will always return false.
        /// </summary>
        public bool ContainsConfidentialInfo { get; set; }

        /// <summary>
        /// Gets a value that specifies when the site was created.
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// The id of the default new page template. Use SetDefaultNewPageTemplateId to set the value.
        /// </summary>
        public Guid DefaultNewPageTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the design package Id of this site.
        /// </summary>
        public Guid DesignPackageId { get; set; }

        /// <summary>
        /// Gets or sets whether the recommended items are disabled on this site.
        /// </summary>
        public bool DisableRecommendedItems { get; set; }

        /// <summary>
        /// Determines if the Document Library Callout's WAC previewers are enabled or not.
        /// </summary>
        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the Web site should use Minimal Download Strategy.
        /// </summary>
        public bool EnableMinimalDownload { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer emphasis.
        /// </summary>
        public FooterVariantThemeType FooterEmphasis { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the footer is enabled on the site.
        /// </summary>
        public bool FooterEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer layout.
        /// </summary>
        public FooterLayoutType FooterLayout { get; set; }

        /// <summary>
        /// Gets or sets the value of the header emphasis.
        /// </summary>
        public VariantThemeType HeaderEmphasis { get; set; }

        /// <summary>
        /// Gets or sets the value of the header layout.
        /// </summary>
        public HeaderLayoutType HeaderLayout { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the title in header is hidden on the site.
        /// </summary>
        public bool HideTitleInHeader { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the homepage is modernized.
        /// </summary>
        public bool IsHomepageModernized { get; }

        /// <summary>
        /// Gets a value that indicates whether the provisioning is complete.
        /// </summary>
        public bool IsProvisioningComplete { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the link to revert homepage is hidden.
        /// </summary>
        public bool IsRevertHomepageLinkHidden { get; set; }

        /// <summary>
        /// Gets a value that specifies the LCID for the language that is used on the site.
        /// </summary>
        public int Language { get; }

        /// <summary>
        /// Gets a value that specifies when an item was last modified in the site.
        /// </summary>
        public DateTime LastItemModifiedDate { get; }

        /// <summary>
        /// Gets a value that specifies when an item was last modified by user in the site.
        /// </summary>
        public DateTime LastItemUserModifiedDate { get; }

        /// <summary>
        /// Gets or sets the logo alignment of the site.
        /// </summary>
        public LogoAlignment LogoAlignment { get; set; }

        /// <summary>
        /// Gets or sets the URL of the master page that is used for the website.
        /// </summary>
        public string MasterUrl { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the megamenu is enabled on the site.
        /// </summary>
        public bool MegaMenuEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the audience targeting is enabled on the navigation of the site.
        /// </summary>
        public bool NavAudienceTargetingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies if the Next steps pane should open automatically as a first run experience
        /// when someone visits the site. You can only enable the experience for sites created on or after January 1, 2020
        /// </summary>
        public bool NextStepsFirstRunEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies if the web templates experience should open automatically as a first run experience
        /// after the site was created
        /// </summary>
        public bool WebTemplatesGalleryFirstRunEnabled { get; set; }

        /// <summary>
        /// Returns if true if the tenant allowed to send push notifications in OneDrive for Business.
        /// </summary>
        public bool NotificationsInOneDriveForBusinessEnabled { get; }

        /// <summary>
        /// Returns if true if the tenant allowed to send push notifications in SharePoint.
        /// </summary>
        public bool NotificationsInSharePointEnabled { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the object cache is enabled on the site.
        /// </summary>
        public bool ObjectCacheEnabled { get; set; }

        /// <summary>
        /// Indicates whether the tenant administrator has chosen to disable the Preview Features. Default is true.
        /// </summary>
        public bool PreviewFeaturesEnabled { get; }

        /// <summary>
        /// Gets or sets the primary color of the site.
        /// </summary>
        public string PrimaryColor { get; }

        /// <summary>
        /// Gets the recycle bin of the website.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IRecycleBinItemCollection RecycleBin { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the Recycle Bin is enabled.
        /// </summary>
        public bool RecycleBinEnabled { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the Web site can be saved as a site template.
        /// </summary>
        public bool SaveSiteAsTemplateEnabled { get; set; }

        /// <summary>
        /// Search placeholder text for search box in navbar - overrides default placeholder text if set, returns null if not set.
        /// </summary>
        public string SearchBoxPlaceholderText { get; set; }

        /// <summary>
        /// Gets the server relative URL of the current site.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Gets or sets the description of the Web site logo.
        /// </summary>
        public string SiteLogoDescription { get; set; }

        /// <summary>
        /// Gets or sets the server-relative URL of the Web site logo.
        /// This can also contain an absolute URL to the logo.
        /// </summary>
        public string SiteLogoUrl { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the RSS feeds are enabled on the site.
        /// </summary>
        public bool SyndicationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies how the tenant admin members can share.
        /// </summary>
        public SharingState TenantAdminMembersCanShare { get; }

        /// <summary>
        /// Get JSON serialized ThemeData for the current web.
        /// </summary>
        public string ThemeData { get; }

        /// <summary>
        /// Gets a value that indicates whether third party MDM (Mobile Device Management) is enabled on the site.
        /// </summary>
        public bool ThirdPartyMdmEnabled { get; }

        /// <summary>
        /// Gets or sets value that specifies whether the tree view is enabled on the site.
        /// </summary>
        public bool TreeViewEnabled { get; set; }

        /// <summary>
        /// Determines if we need to use the default access request
        /// If this value is True we send access requests to owner/owner's group
        /// If this value is False we send access requests to the configured AccessRequestEmail
        /// </summary>
        public bool UseAccessRequestDefault { get; }

        /// <summary>
        /// Gets the name of the site definition or site template that was used to create the site.
        /// </summary>
        public string WebTemplate { get; }

        /// <summary>
        /// Gets the web template configuration of the site.
        /// </summary>
        public string WebTemplateConfiguration { get; }

        /// <summary>
        /// Defines whether the site has to be crawled or not
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// The email address to which any access request will be sent
        /// </summary>
        public string RequestAccessEmail { get; set; }

        /// <summary>
        /// Specifies a string that contains the site-relative URL to which users are redirected when web is browsed (typically the site's home page).
        /// </summary>
        public string WelcomePage { get; }

        /// <summary>
        /// The Title of the Site, optional attribute.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Description of the Site, optional attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The AlternateCSS of the Site, optional attribute.
        /// </summary>
        public string AlternateCssUrl { get; set; }

        /// <summary>
        /// The Custom MasterPage Url of the Site, optional attribute.
        /// </summary>
        public string CustomMasterUrl { get; set; }

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
        /// The URL of the Web object
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// List of languages (expressed via their id) that this site supports
        /// </summary>
        public List<int> SupportedUILanguageIds { get; }

        /// <summary>
        /// Gets the current user in the current context
        /// </summary>
        public ISharePointUser CurrentUser { get; }

        /// <summary>
        /// Gets the web's author
        /// </summary>
        public ISharePointUser Author { get; }

        /// <summary>
        /// Collection of lists in the current Web object.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IListCollection Lists { get; }

        /// <summary>
        /// Collection of content types in the current Web object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        /// <summary>
        /// Collection of fields in the current Web object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// Collection of webs in this current web
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IWebCollection Webs { get; }

        /// <summary>
        /// Collection of features enabled for the web
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFeatureCollection Features { get; }

        /// <summary>
        /// Collection of folders in the current Web object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFolderCollection Folders { get; }

        /// <summary>
        /// Gets a collection of metadata for the Web site.
        /// </summary>
        public IPropertyValues AllProperties { get; }

        /// <summary>
        /// Gets the collection of all content types that apply to the current scope, including those of the current Web site, as well as any parent Web sites.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IContentTypeCollection AvailableContentTypes { get; }

        /// <summary>
        /// Gets a value that specifies the collection of all fields available for the current scope, including those of the current site, as well as any parent sites.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
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

        /// <summary>
        /// Gets the collection of all users that belong to the site collection.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ISharePointUserCollection SiteUsers { get; }

        /// <summary>
        /// Gets the collection of all groups that belong to the site collection.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ISharePointGroupCollection SiteGroups { get; }

        /// <summary>
        /// Gets a value that specifies the collection of user custom actions for the site.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IUserCustomActionCollection UserCustomActions { get; }

        /// <summary>
        /// Get's the permission levels set on this web
        /// </summary>
        public IBasePermissions EffectiveBasePermissions { get; }

        /// <summary>
        /// Regional settings configured for this web
        /// </summary>
        public IRegionalSettings RegionalSettings { get; }

        /// <summary>
        /// Associated SharePoint Member group
        /// </summary>
        public ISharePointGroup AssociatedMemberGroup { get; }

        /// <summary>
        /// Associated SharePoint owner group
        /// </summary>
        public ISharePointGroup AssociatedOwnerGroup { get; }

        /// <summary>
        /// Associated SharePoint Visitor group
        /// </summary>
        public ISharePointGroup AssociatedVisitorGroup { get; }

        /// <summary>
        /// Role Definitions defined in this web
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IRoleDefinitionCollection RoleDefinitions { get; }

        /// <summary>
        /// Navigation on the Web
        /// </summary>
        public INavigation Navigation { get; }

        /// <summary>
        /// Event Receivers defined in this web
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IEventReceiverDefinitionCollection EventReceivers { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #endregion

        #region Methods

        #region Pages API

        /// <summary>
        /// Gets the modern pages of this site, optionally scoped down via a filter
        /// </summary>
        /// <param name="pageName">Page name to filter on, uses a "starts with" filter</param>
        /// <returns>One of more modern pages</returns>
        public Task<List<IPage>> GetPagesAsync(string pageName = null);

        /// <summary>
        /// Gets the modern pages of this site, optionally scoped down via a filter
        /// </summary>
        /// <param name="pageName">Page name to filter on, uses a "starts with" filter</param>
        /// <returns>One of more modern pages</returns>
        public List<IPage> GetPages(string pageName = null);

        /// <summary>
        /// Gets Viva Connections dashboard.
        /// If there is no dashboard configured, returns null
        /// </summary>
        /// <returns>Viva Dashboard or null</returns>
        public Task<IVivaDashboard> GetVivaDashboardAsync();

        /// <summary>
        /// Gets Viva Connections dashboard.
        /// If there is no dashboard configured, returns null
        /// </summary>
        /// <returns>Viva Dashboard or null</returns>
        public IVivaDashboard GetVivaDashboard();

        /// <summary>
        /// Creates a new modern page
        /// </summary>
        /// <param name="pageLayoutType">Optionally specify the page type, defaults to <see cref="PageLayoutType.Article"/></param>
        /// <returns>Created modern page</returns>
        public Task<IPage> NewPageAsync(PageLayoutType pageLayoutType = PageLayoutType.Article);

        /// <summary>
        /// Creates a new modern page
        /// </summary>
        /// <param name="pageLayoutType">Optionally specify the page type, defaults to <see cref="PageLayoutType.Article"/></param>
        /// <returns>Created modern page</returns>
        public IPage NewPage(PageLayoutType pageLayoutType = PageLayoutType.Article);
        #endregion

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

        #region GetFolderById
        /// <summary>
        /// Get a folder in the current web from its id.
        /// </summary>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByIdAsync(Guid folderId, params Expression<Func<IFolder, object>>[] expressions);


        /// <summary>
        /// Get a folder in the current web from its id.
        /// </summary>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderById(Guid folderId, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its id via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByIdBatchAsync(Batch batch, Guid folderId, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its id via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderByIdBatch(Batch batch, Guid folderId, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its id via batch.
        /// </summary>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public Task<IFolder> GetFolderByIdBatchAsync(Guid folderId, params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Get a folder in the current web from its id via batch.
        /// </summary>
        /// <param name="folderId">The id of the folder to get.</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <returns>The folder to get</returns>
        public IFolder GetFolderByIdBatch(Guid folderId, params Expression<Func<IFolder, object>>[] expressions);
        #endregion

        #region GetFileByServerRelativeUrl
        /// <summary>
        /// Get a file in the current web from its server relative URL, it not available null will be returned
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get or null if the file was not available</returns>
        public IFile GetFileByServerRelativeUrlOrDefault(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL, it not available null will be returned
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get or null if the file was not available</returns>
        public Task<IFile> GetFileByServerRelativeUrlOrDefaultAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);


        /// <summary>
        /// Get a file in the current web from its server relative URL.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its server relative URL via batch.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative URL of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions);

        #endregion

        #region GetFileById

        /// <summary>
        /// Get a file in the current web from its unique id.
        /// </summary>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByIdAsync(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its unique id.
        /// </summary>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileById(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its unique id via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByIdBatchAsync(Batch batch, Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its unique id via batch.
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileByIdBatch(Batch batch, Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its unique id via batch.
        /// </summary>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public Task<IFile> GetFileByIdBatchAsync(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get a file in the current web from its unique id via batch.
        /// </summary>
        /// <param name="uniqueFileId">The unique id of the file to get.</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns>The file to get</returns>
        public IFile GetFileByIdBatch(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions);

        #endregion

        #region GetFileByLink

        /// <summary>
        /// Get's a file from a given link (sharing link, path to file)
        /// </summary>
        /// <param name="link">Link pointing to a file</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns><see cref="IFile"/> reference when the file could be found, exception otherwise</returns>
        public Task<IFile> GetFileByLinkAsync(string link, params Expression<Func<IFile, object>>[] expressions);

        /// <summary>
        /// Get's a file from a given link (sharing link, path to file)
        /// </summary>
        /// <param name="link">Link pointing to a file</param>
        /// <param name="expressions">Properties to load for the requested <see cref="IFile"/></param>
        /// <returns><see cref="IFile"/> reference when the file could be found, exception otherwise</returns>
        public IFile GetFileByLink(string link, params Expression<Func<IFile, object>>[] expressions);
        
        #endregion

        #region IsNoScriptSite 
        /// <summary>
        /// Checks if this web is configured for NoScript
        /// </summary>
        /// <returns>True if set to NoScript, false otherwise</returns>
        public Task<bool> IsNoScriptSiteAsync();

        /// <summary>
        /// Checks if this web is configured for NoScript
        /// </summary>
        /// <returns>True if set to NoScript, false otherwise</returns>
        public bool IsNoScriptSite();
        #endregion

        #region Users

        /// <summary>
        /// Retrieves everyone except external users and ensures the user in the current web
        /// </summary>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> GetEveryoneExceptExternalUsersAsync();

        /// <summary>
        /// Retrieves everyone except external users and ensures the user in the current web
        /// </summary>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public ISharePointUser GetEveryoneExceptExternalUsers();

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> EnsureUserAsync(string userPrincipalName);

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public ISharePointUser EnsureUser(string userPrincipalName);

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> EnsureUserBatchAsync(string userPrincipalName);

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public ISharePointUser EnsureUserBatch(string userPrincipalName);

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> EnsureUserBatchAsync(Batch batch, string userPrincipalName);

        /// <summary>
        /// Ensures the given users exists
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="userPrincipalName">UserPrincipalName value of the user to verify</param>
        /// <returns>The ensured <see cref="ISharePointUser"/></returns>
        public ISharePointUser EnsureUserBatch(Batch batch, string userPrincipalName);

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> GetCurrentUserAsync();

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public ISharePointUser GetCurrentUser();

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> GetCurrentUserBatchAsync();

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public ISharePointUser GetCurrentUserBatch();

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public Task<ISharePointUser> GetCurrentUserBatchAsync(Batch batch);

        /// <summary>
        /// Get's the current logged on user making the request to SharePoint
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The current <see cref="ISharePointUser"/></returns>
        public ISharePointUser GetCurrentUserBatch(Batch batch);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUser(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public ISharePointUser GetUserById(int userId);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUserAsync(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public Task<ISharePointUser> GetUserByIdAsync(int userId);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUserBatch(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public ISharePointUser GetUserByIdBatch(int userId);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUserBatchAsync(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public Task<ISharePointUser> GetUserByIdBatchAsync(int userId);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUserBatch(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public ISharePointUser GetUserByIdBatch(Batch batch, int userId);

        /// <summary>
        /// Get's a user by it's id in this site. The user needs to exist, use <see cref="EnsureUserBatchAsync(string)"/> if you want to create the user if it does not yet exist
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <param name="userId">Id of the user to get</param>
        /// <returns>The found user as <see cref="ISharePointPrincipal"/></returns>
        public Task<ISharePointUser> GetUserByIdBatchAsync(Batch batch, int userId);

        /// <summary>
        /// Checks if the provided list of user UPN's are valid users in Azure AD
        /// </summary>
        /// <param name="userList">List of user UPN's to validate in Azure AD</param>
        /// <returns>>A list of users that were not found in Azure AD</returns>
        public Task<IList<string>> ValidateUsersAsync(IList<string> userList);

        /// <summary>
        /// Checks if the provided list of user UPN's are valid users in Azure AD
        /// </summary>
        /// <param name="userList">List of user UPN's to validate in Azure AD</param>
        /// <returns>A list of users that were not found in Azure AD</returns>
        public IList<string> ValidateUsers(IList<string> userList);

        /// <summary>
        /// Checks if the provided list of user UPN's are valid users in Azure AD and returns the 'ensured' SharePoint user
        /// </summary>
        /// <param name="userList">List of user UPN's to validate in Azure AD</param>
        /// <returns>The list of <see cref="ISharePointUser"/> that exist</returns>
        public Task<IList<ISharePointUser>> ValidateAndEnsureUsersAsync(IList<string> userList);

        /// <summary>
        /// Checks if the provided list of user UPN's are valid users in Azure AD and returns the 'ensured' SharePoint user
        /// </summary>
        /// <param name="userList">List of user UPN's to validate in Azure AD</param>
        /// <returns>The list of <see cref="ISharePointUser"/> that exist</returns>
        public IList<ISharePointUser> ValidateAndEnsureUsers(IList<string> userList);
        #endregion

        #region Multi-lingual

        /// <summary>
        /// Ensure a site can support multilingual (pages) for the passed along languages
        /// </summary>
        /// <param name="requiredLanguageIds">List of langauges to support for multilingual on this site. See https://capacreative.co.uk/resources/reference-sharepoint-online-languages-ids/ for information on the language ids</param>
        /// <returns></returns>
        Task EnsureMultilingualAsync(List<int> requiredLanguageIds);

        /// <summary>
        /// Ensure a site can support multilingual (pages) for the passed along languages
        /// </summary>
        /// <param name="requiredLanguageIds">List of langauges to support for multilingual on this site. See https://capacreative.co.uk/resources/reference-sharepoint-online-languages-ids/ for information on the language ids</param>
        /// <returns></returns>
        void EnsureMultilingual(List<int> requiredLanguageIds);

        #endregion

        #region Syntex support
        /// <summary>
        /// Is the tenant enabled for SharePoint Syntex
        /// </summary>
        /// <returns>True if SharePoint Syntex is enabled, false otherwise</returns>
        Task<bool> IsSyntexEnabledAsync();

        /// <summary>
        /// Is the tenant enabled for SharePoint Syntex
        /// </summary>
        /// <returns>True if SharePoint Syntex is enabled, false otherwise</returns>
        bool IsSyntexEnabled();

        /// <summary>
        /// Is SharePoint Syntex enabled for the current user
        /// </summary>
        /// <returns>True if SharePoint Syntex is enabled for the current user, false otherwise</returns>
        Task<bool> IsSyntexEnabledForCurrentUserAsync();

        /// <summary>
        /// Is SharePoint Syntex enabled for the current user
        /// </summary>
        /// <returns>True if SharePoint Syntex is enabled for the current user, false otherwise</returns>
        bool IsSyntexEnabledForCurrentUser();

        /// <summary>
        /// Is this web a Syntex Content Center
        /// </summary>
        /// <returns>True if this web is a Syntex Content Center, false otherwise</returns>
        Task<bool> IsSyntexContentCenterAsync();

        /// <summary>
        /// Is this web a Syntex Content Center
        /// </summary>
        /// <returns>True if this web is a Syntex Content Center, false otherwise</returns>
        bool IsSyntexContentCenter();

        /// <summary>
        /// Returns the current web as <see cref="ISyntexContentCenter"/> if the web is a Syntex Content Center, null is returned otherwise
        /// </summary>
        /// <returns>The current web as <see cref="ISyntexContentCenter"/></returns>
        Task<ISyntexContentCenter> AsSyntexContentCenterAsync();

        /// <summary>
        /// Returns the current web as <see cref="ISyntexContentCenter"/> if the web is a Syntex Content Center, null is returned otherwise
        /// </summary>
        /// <returns>The current web as <see cref="ISyntexContentCenter"/></returns>
        ISyntexContentCenter AsSyntexContentCenter();
        #endregion

        #region Hub Site

        /// <summary>
        /// Sync the hub site theme from parent hub site
        /// </summary>
        public Task SyncHubSiteThemeAsync();

        #endregion

        #region Is sub site

        /// <summary>
        /// Checks if this web is a sub site
        /// </summary>
        /// <returns>True if the web is a sub site</returns>
        Task<bool> IsSubSiteAsync();

        /// <summary>
        /// Checks if this web is a sub site
        /// </summary>
        /// <returns>True if the web is a sub site</returns>
        bool IsSubSite();

        #endregion

        #region AccessRequest

        /// <summary>
        /// Applies the access request settings
        /// </summary>
        /// <param name="operation">The operation to be performed</param>
        /// <param name="email">Applies the email address for the 'SpecificMail' operation</param>
        /// <returns></returns>
        public Task SetAccessRequest(AccessRequestOption operation, string email = null);

        /// <summary>
        /// Applies the access request settings
        /// </summary>
        /// <param name="operation">The operation to be performed</param>
        /// <param name="email">Applies the email address for the 'SpecificMail' operation</param>
        /// <returns></returns>
        public Task SetAccessRequestAsync(AccessRequestOption operation, string email = null);

        #endregion

        #region Ensure page scheduling

        /// <summary>
        /// Ensures that page publishing can work for this site. Page scheduling only works for the root web of a site collection
        /// </summary>
        /// <returns></returns>
        Task EnsurePageSchedulingAsync();

        /// <summary>
        /// Ensures that page publishing can work for this site. Page scheduling only works for the root web of a site collection
        /// </summary>
        /// <returns></returns>
        void EnsurePageScheduling();

        #endregion

        #region Has Communication Site features
        /// <summary>
        /// Does this web have the communication site features enabled?
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        Task<bool> HasCommunicationSiteFeaturesAsync();

        /// <summary>
        /// Does this web have the communication site features enabled?
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        bool HasCommunicationSiteFeatures();
        #endregion

        #region Branding
        /// <summary>
        /// Returns the branding manager which can be used to change the look of the web
        /// </summary>
        /// <returns>An <see cref="IBrandingManager"/> instance</returns>
        IBrandingManager GetBrandingManager();
        #endregion

        #region Search

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <returns>The results of the search query</returns>
        Task<ISearchResult> SearchAsync(SearchOptions query);

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <returns>The results of the search query</returns>
        ISearchResult Search(SearchOptions query);

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <returns>The results of the search query</returns>
        Task<IBatchSingleResult<ISearchResult>> SearchBatchAsync(SearchOptions query);

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <returns>The results of the search query</returns>
        IBatchSingleResult<ISearchResult> SearchBatch(SearchOptions query);

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The results of the search query</returns>
        Task<IBatchSingleResult<ISearchResult>> SearchBatchAsync(Batch batch, SearchOptions query);

        /// <summary>
        /// Performs a search query
        /// </summary>
        /// <param name="query">Search query to run</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The results of the search query</returns>
        IBatchSingleResult<ISearchResult> SearchBatch(Batch batch, SearchOptions query);
        #endregion

        #region Web Templates

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        Task<List<IWebTemplate>> GetWebTemplatesAsync(int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        List<IWebTemplate> GetWebTemplates(int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        Task<IEnumerableBatchResult<IWebTemplate>> GetWebTemplatesBatchAsync(int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        IEnumerableBatchResult<IWebTemplate> GetWebTemplatesBatch(int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        Task<IEnumerableBatchResult<IWebTemplate>> GetWebTemplatesBatchAsync(Batch batch, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Returns a collection of site templates available for the site.
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        IEnumerableBatchResult<IWebTemplate> GetWebTemplatesBatch(Batch batch, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <returns>The template with the specified name, if available</returns>
        Task<IWebTemplate> GetWebTemplateByNameAsync(string name, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <returns>The template with the specified name, if available</returns>
        IWebTemplate GetWebTemplateByName(string name, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <returns>The list of webtemplates available on the current web</returns>
        Task<IBatchSingleResult<IWebTemplate>> GetWebTemplateByNameBatchAsync(string name, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <returns>The template with the specified name, if available</returns>
        IBatchSingleResult<IWebTemplate> GetWebTemplateByNameBatch(string name, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The template with the specified name, if available</returns>
        Task<IBatchSingleResult<IWebTemplate>> GetWebTemplateByNameBatchAsync(Batch batch, string name, int lcid = 1033, bool includeCrossLanguage = false);

        /// <summary>
        /// Return a specific web template based by a name
        /// </summary>
        /// <param name="lcid">Specifies the LCID of the site templates to be retrieved.</param>
        /// <param name="includeCrossLanguage">Specifies whether to include language-neutral site templates.</param>
        /// <param name="name">Name of the template to retrieve</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>The template with the specified name, if available</returns>
        IBatchSingleResult<IWebTemplate> GetWebTemplateByNameBatch(Batch batch, string name, int lcid = 1033, bool includeCrossLanguage = false);

        #endregion

        #region Link unfurling

        /// <summary>
        /// Takes in a sharing or regular SharePoint link and tries to provide relavant information about the resource behind the passed in link
        /// </summary>
        /// <param name="link">Link to resource to get information for</param>
        /// <param name="unfurlOptions">Options to control the unfurling data gathering</param>
        /// <returns>Data about the unfurled resource</returns>
        Task<IUnfurledResource> UnfurlLinkAsync(string link, UnfurlOptions unfurlOptions = null);

        /// <summary>
        /// Takes in a sharing or regular SharePoint link and tries to provide relavant information about the resource behind the passed in link
        /// </summary>
        /// <param name="link">Link to resource to get information for</param>
        /// <param name="unfurlOptions">Options to control the unfurling data gathering</param>
        /// <returns>Data about the unfurled resource</returns>
        IUnfurledResource UnfurlLink(string link, UnfurlOptions unfurlOptions = null);

        #endregion

        #region Recycle bin

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryAsync(RecycleBinQueryOptions options);

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        IRecycleBinItemCollection GetRecycleBinItemsByQuery(RecycleBinQueryOptions options);

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryBatchAsync(RecycleBinQueryOptions options);

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        IRecycleBinItemCollection GetRecycleBinItemsByQueryBatch(RecycleBinQueryOptions options);

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryBatchAsync(Batch batch, RecycleBinQueryOptions options);

        /// <summary>
        /// Searches the recyclebin returning items based upon the passed search criteria
        /// </summary>
        /// <param name="options">Recycle bin search criteria</param>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>A list containing zero or more recycle bin items</returns>
        IRecycleBinItemCollection GetRecycleBinItemsByQueryBatch(Batch batch, RecycleBinQueryOptions options);
        #endregion

        #region Get Search Configuration

        /// <summary>
        /// Gets the search configuration of the web
        /// </summary>
        /// <returns>Search configuration XML</returns>
        Task<string> GetSearchConfigurationXmlAsync();

        /// <summary>
        /// Gets the search configuration of the web
        /// </summary>
        /// <returns>Search configuration XML</returns>
        string GetSearchConfigurationXml();

        /// <summary>
        /// Gets the managed properties from the search configuration of this web
        /// </summary>
        /// <returns>List of managed properties</returns>
        Task<List<IManagedProperty>> GetSearchConfigurationManagedPropertiesAsync();

        /// <summary>
        /// Gets the managed properties from the search configuration of this web
        /// </summary>
        /// <returns>List of managed properties</returns>
        List<IManagedProperty> GetSearchConfigurationManagedProperties();

        /// <summary>
        /// Sets the search configuration for the web
        /// </summary>
        /// <param name="configuration">Search configuration, obtained via <see cref="GetSearchConfigurationXml"/> to apply</param>
        Task SetSearchConfigurationXmlAsync(string configuration);

        /// <summary>
        /// Sets the search configuration for the web
        /// </summary>
        /// <param name="configuration">Search configuration, obtained via <see cref="GetSearchConfigurationXml"/> to apply</param>
        void SetSearchConfigurationXml(string configuration);
        #endregion

        #region Get WSS Id for term

        /// <summary>
        /// Returns the Id for a term if present in the TaxonomyHiddenList. Otherwise returns -1;
        /// </summary>
        /// <param name="termId">Id of the term to lookup</param>
        /// <returns>Id of the term in the taxonomy hidden list, otherwise -1</returns>
        Task<int> GetWssIdForTermAsync(string termId);

        /// <summary>
        /// Returns the Id for a term if present in the TaxonomyHiddenList. Otherwise returns -1;
        /// </summary>
        /// <param name="termId">Id of the term to lookup</param>
        /// <returns>Id of the term in the taxonomy hidden list, otherwise -1</returns>
        int GetWssIdForTerm(string termId);
        #endregion

        #region Effective user permissions

        /// <summary>
        /// Gets the user effective permissions of a user for a web
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        IBasePermissions GetUserEffectivePermissions(string userPrincipalName);

        /// <summary>
        /// Gets the user effective permissions of a user for a web
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        Task<IBasePermissions> GetUserEffectivePermissionsAsync(string userPrincipalName);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a web
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        bool CheckIfUserHasPermissions(string userPrincipalName, PermissionKind permissionKind);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a web
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        Task<bool> CheckIfUserHasPermissionsAsync(string userPrincipalName, PermissionKind permissionKind);

        #endregion

        #region Reindex web
        /// <summary>
        /// Reindexes this web
        /// </summary>
        /// <returns></returns>
        Task ReIndexAsync();

        /// <summary>
        /// Reindexes this web
        /// </summary>
        /// <returns></returns>
        void ReIndex();

        #endregion

        #region Indexed properties
        
        /// <summary>
        /// Adds a web property as an indexed property
        /// </summary>
        /// <remarks>The property must already exist as metadata of the Web</remarks>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if it was successfully added or if it is already exists otherwise false</returns>
        public Task<bool> AddIndexedPropertyAsync(string propertyName);

        /// <summary>
        /// Adds a web property as an indexed property
        /// </summary>
        /// <remarks>The property must already exist as metadata of the Web</remarks>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if it was successfully added or if it is already exists otherwise false</returns>
        public bool AddIndexedProperty(string propertyName);

        /// <summary>
        /// Removes a web propetry from the indexed properties
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if it was successfully removed otherwise false</returns>
        public Task<bool> RemoveIndexedPropertyAsync(string propertyName);

        /// <summary>
        /// Removes a web propetry from the indexed properties
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if it was successfully removed otherwise false</returns>
        public bool RemoveIndexedProperty(string propertyName);

        #endregion

        #endregion

        #region TO IMPLEMENT
        // Take information from here to update documentation of this class member
        // https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee546309(v=office.15)

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
        //public IModernizeHomepageResult CanModernizeHomepage { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IClientWebPartCollection ClientWebParts { get; }

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
        //public ISiteCollectionCorporateCatalogAccessor SiteCollectionAppCatalog { get; }

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
        //public IWebInformationCollection WebInfos { get; }

        #endregion
    }
}
