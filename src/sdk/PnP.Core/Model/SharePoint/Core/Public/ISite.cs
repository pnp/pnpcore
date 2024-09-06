using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Site object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Site))]
    public interface ISite : IDataModel<ISite>, IDataModelGet<ISite>, IDataModelLoad<ISite>, IDataModelUpdate, IDataModelSupportingGetChanges
    {
        /// <summary>
        /// The Unique ID of the Site object
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The unique ID of the associated Microsoft 365 Group (if any)
        /// </summary>
        public Guid GroupId { get; }

        /// <summary>
        /// The URL of the Site object
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// The Classification of the Site object
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Defines whether social bar is disabled on Site Pages in this site collection
        /// </summary>
        public bool SocialBarOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Whether suite nav search box is shown on modern and classic pages 
        /// </summary>
        public SearchBoxInNavBar SearchBoxInNavBar { get; set; }

        /// <summary>
        /// The RootWeb of the Site object
        /// </summary>
        public IWeb RootWeb { get; }

        /// <summary>
        /// Collection of sub-webs in the current Site object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IWebCollection AllWebs { get; }

        /// <summary>
        /// Collection of features enabled for the site
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFeatureCollection Features { get; }        

        /// <summary>
        /// Gets or sets a value that specifies whether the creation of declarative workflows is allowed on this site collection.
        /// </summary>
        public bool AllowCreateDeclarativeWorkflow { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether a designer can be used on this site collection.
        /// </summary>
        public bool AllowDesigner { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether external embedding wrapper is allowed on this site collection.
        /// 0 means "Don't allow embedding any external domains"
        /// 1 means "Only allow embedding external domains from allow-embed-domains-list"
        /// 2 means "Allow embedding any external domains"
        /// </summary>
        public int AllowExternalEmbeddingWrapper { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether master page editing is allowed on this site collection.
        /// </summary>
        public bool AllowMasterPageEditing { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether this site collection can be reverted to its base template.
        /// </summary>
        public bool AllowRevertFromTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether it is allowed to save declarative workflows as template on this site collection.
        /// </summary>
        public bool AllowSaveDeclarativeWorkflowAsTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether it is allowed to save and publish declarative workflows on this site collection.
        /// </summary>
        public bool AllowSavePublishDeclarativeWorkflow { get; set; }

        /// <summary>
        /// This is the number of days of audit log data to retain. If unset and
        /// audit trimming is enabled, the retention defaults default configured schedule for trimming
        /// </summary>
        public int AuditLogTrimmingRetention { get; set; }

        /// <summary>
        /// Gets or sets value if syncing hub site permissions to this associated site is allowed.
        /// </summary>
        public bool CanSyncHubSitePermissions { get; set; }

        /// <summary>
        /// Gets the ID of the Modern Group associated with this site.
        /// </summary>
        public Guid ChannelGroupId { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the comments on site pages are disabled on this site collection.
        /// </summary>
        public bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Sets whether or not to disable app views for all child webs.
        /// True means app views are disabled throughout the site collection; False otherwise.
        /// </summary>
        public bool DisableAppViews { get; set; }

        /// <summary>
        /// Sets whether or not to disable company sharing links for all child webs.
        /// True means companywide sharing links are disabled throughout the site collection, regardless of the settings on the root or child webs.
        /// False means each web can individually decide whether to turn on or off companywide sharing links.
        /// </summary>
        public bool DisableCompanyWideSharingLinks { get; set; }

        /// <summary>
        /// Sets whether or not to disable Flows for all child webs.
        /// True means Flows are disabled throughout the site collection; False otherwise.
        /// </summary>
        public bool DisableFlows { get; set; }

        /// <summary>
        /// Gets a boolean value that specifies whether users will be greeted with a notification bar
        /// telling them that the site can be shared with external users.
        /// True - notification bar is enabled; False - otherwise
        /// </summary>
        public bool ExternalSharingTipsEnabled { get; }

        /// <summary>
        /// Property that indicates the default number of days an external user will expire in.
        /// 0 if the policy is disabled or unavailable, otherwise the number of days
        /// </summary>
        public int ExternalUserExpirationInDays { get; }

        /// <summary>
        /// returns the Geo Location hosting this site collection
        /// </summary>
        public string GeoLocation { get; }

        /// <summary>
        /// Gets the ID of the HubSite this site is associated with.
        /// </summary>
        /// <remarks>Use JoinHubSite method to change the value of this property.</remarks>
        public Guid HubSiteId { get; }

        /// <summary>
        /// Gets the Information Protection Label Id for an individual site collection.
        /// </summary>
        public Guid SensitivityLabelId { get; }

        /// <summary>
        /// Information Protection Label Id for an individual site collection
        /// </summary>
        public string SensitivityLabel { get; }

        /// <summary>
        /// Returns whether or not this site is a HubSite. Hub sites can be associated with one or more sites.
        /// </summary>
        public bool IsHubSite { get; }

        /// <summary>
        /// Gets the comment that is used in locking a site collection.
        /// </summary>
        public string LockIssue { get; }

        /// <summary>
        /// Maximum items that will not be throttled per operation.
        /// </summary>
        public int MaxItemsPerThrottledOperation { get; }

        /// <summary>
        /// Gets or sets a bool value that specifies whether the site collection is read-only, locked, and unavailable for write access.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public bool ReadOnly { get; set; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Gets the ID of the Modern Group related to this site.
        /// </summary>
        public Guid RelatedGroupId { get; }

        /// <summary>
        /// Gets a value that specifies the collection of Recycle Bin items for the site collection.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IRecycleBinItemCollection RecycleBin { get; }

        /// <summary>
        /// Search placeholder text for search box in navbar - overrides default placeholder text if set.
        /// </summary>
        public string SearchBoxPlaceholderText { get; set; }

        /// <summary>
        /// Gets the server-relative URL of the root Web site in the site collection.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// ShareByEmailEnabled when true means that user's will be able to grant permissions to guests for resources within the site collection
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// Property that indicates whether users will be able to share links to documents that can be accessed without logging in.
        /// </summary>
        public bool ShareByLinkEnabled { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether guest users should be displayed as suggestions in people picker on this site collection.
        /// </summary>
        public bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the status bar link on this site collection (sets the SiteStatusBarLink property on the root web).
        /// </summary>
        public string StatusBarLink { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the text of the status bar on this site collection (sets the SiteStatusBarText property on the root web).
        /// </summary>
        public string StatusBarText { get; set; }

        /// <summary>
        /// Gets a value that indicates whether thicket support is disabled on this site collection.
        /// </summary>
        public bool ThicketSupportDisabled { get; }

        /// <summary>
        /// When this flag is set for the site, the audit events are trimmed periodically.
        /// </summary>
        public bool TrimAuditLog { get; set; }

        /// <summary>
        /// Gets a value that specifies the collection of user custom actions for the site collection.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IUserCustomActionCollection UserCustomActions { get; }

        /// <summary>
        /// Gets the synchronizable visitor group for a hub site
        /// </summary>
        public ISharePointGroup HubSiteSynchronizableVisitorGroup { get; }

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

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IAudit Audit { get; }

        /// <summary>
        /// Retrieves the available compliance tags / retention labels for this site
        /// </summary>
        public IEnumerable<IComplianceTag> GetAvailableComplianceTags();

        /// <summary>
        /// Retrieves the available compliance tags / retention labels for this site
        /// </summary>
        public Task<IEnumerable<IComplianceTag>> GetAvailableComplianceTagsAsync();


        /// <summary>
        /// Registers the current site as a primary hub site
        /// </summary>
        public Task<IHubSite> RegisterHubSiteAsync();

        /// <summary>
        /// Registers the current site as a primary hub site
        /// </summary>
        public IHubSite RegisterHubSite();

        /// <summary>
        /// Unregisters the current site as a primary hub site
        /// </summary>
        public Task<bool> UnregisterHubSiteAsync();

        /// <summary>
        /// Unregisters the current site as a primary hub site
        /// </summary>
        public bool UnregisterHubSite();

        /// <summary>
        /// Associates the current site to a primary hub site
        /// </summary>
        public Task<bool> JoinHubSiteAsync(Guid hubSiteId);

        /// <summary>
        /// Associates the current site to a primary hub site
        /// </summary>
        public bool JoinHubSite(Guid hubSiteId);

        /// <summary>
        /// Disassociates current site from primary hub site
        /// </summary>
        /// <returns></returns>
        public Task<bool> UnJoinHubSiteAsync();

        /// <summary>
        /// Disassociates current site from primary hub site
        /// </summary>
        /// <returns></returns>
        public bool UnJoinHubSite();

        /// <summary>
        /// Gets hubsite data from the current site OR another specified hub site ID
        /// </summary>
        /// <param name="id">Hub Site Guid</param>
        /// <returns></returns>
        public Task<IHubSite> GetHubSiteDataAsync(Guid? id);

        /// <summary>
        /// Gets hubsite data from the current site OR another specified hub site ID
        /// </summary>
        /// <param name="id">Hub Site Guid</param>
        /// <returns></returns>
        public IHubSite GetHubSiteData(Guid? id);

        /// <summary>
        /// Checks if current site is a HomeSite
        /// </summary>
        public Task<bool> IsHomeSiteAsync();

        /// <summary>
        /// Checks if current site is a HomeSite
        /// </summary>
        public bool IsHomeSite();

        /// <summary>
        /// Creates a new migration job
        /// <param name="exportObjectUris">Array of the objects to migrate (absolute url to the file / folder)</param>
        /// <param name="destinationUri">Destination URI to where the objects have to be migrated</param>
        /// <param name="options">Migration options</param>
        /// <param name="waitUntilFinished">Defines if we have to wait until all the migrations have finished</param>
        /// <param name="waitAfterStatusCheck">Duration between every GetCopyJobProgress call in seconds. Defaults to 1.</param>
        /// <returns>List of all the jobs being created (one for every exportObjectUri)</returns>
        /// </summary>
        public Task<IList<ICopyMigrationInfo>> CreateCopyJobsAsync(string[] exportObjectUris, string destinationUri, CopyMigrationOptions options, bool waitUntilFinished = false, int waitAfterStatusCheck = 1);

        /// <summary>
        /// Creates a new migration job
        /// <param name="exportObjectUris">Array of the objects to migrate (absolute url to the file / folder)</param>
        /// <param name="destinationUri">Destination URI to where the objects have to be migrated</param>
        /// <param name="options">Migration options</param>
        /// <param name="waitUntilFinished">Defines if we have to wait until all the migrations have finished</param>
        /// <param name="waitAfterStatusCheck">Duration between every GetCopyJobProgress call in seconds. Defaults to 1.</param>
        /// <returns>List of all the jobs being created (one for every exportObjectUri)</returns>
        /// </summary>
        public IList<ICopyMigrationInfo> CreateCopyJobs(string[] exportObjectUris, string destinationUri, CopyMigrationOptions options, bool waitUntilFinished = false, int waitAfterStatusCheck = 1);

        /// <summary>
        /// Gets the progress of an existing migration info object
        /// <param name="copyMigrationInfo">Migration job to get the progress of</param>
        /// <returns>Progress of the copy job</returns>
        /// </summary>
        public Task<ICopyJobProgress> GetCopyJobProgressAsync(ICopyMigrationInfo copyMigrationInfo);

        /// <summary>
        /// Gets the progress of an existing migration info object
        /// <param name="copyMigrationInfo">Migration job to get the progress of</param>
        /// <returns>Progress of the copy job</returns>
        /// </summary>
        public ICopyJobProgress GetCopyJobProgress(ICopyMigrationInfo copyMigrationInfo);

        /// <summary>
        /// Ensures that a migration job has completely run
        /// <param name="copyMigrationInfos">List of migration jobs to check the process of</param>
        /// <param name="waitAfterStatusCheck">Duration between every GetCopyJobProgress call in seconds. Defaults to 1.</param>
        /// </summary>
        public Task EnsureCopyJobHasFinishedAsync(IList<ICopyMigrationInfo> copyMigrationInfos, int waitAfterStatusCheck = 1);

        /// <summary>
        /// Ensures that a migration job has completely run
        /// <param name="copyMigrationInfos">List of migration jobs to check the process of</param>
        /// <param name="waitAfterStatusCheck">Duration between every GetCopyJobProgress call in seconds. Defaults to 1.</param>
        /// </summary>
        public void EnsureCopyJobHasFinished(IList<ICopyMigrationInfo> copyMigrationInfos, int waitAfterStatusCheck = 1);

        /// <summary>
        /// Gets site analytics
        /// </summary>
        /// <param name="options">Defines which analytics are needed</param>
        /// <returns>The requested analytics data</returns>
        public Task<List<IActivityStat>> GetAnalyticsAsync(AnalyticsOptions options = null);

        /// <summary>
        /// Gets site analytics
        /// </summary>
        /// <param name="options">Defines which analytics are needed</param>
        /// <returns>The requested analytics data</returns>
        public List<IActivityStat> GetAnalytics(AnalyticsOptions options = null);

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
        /// Gets the managed properties from the search configuration of this site
        /// </summary>
        /// <returns>List of managed properties</returns>
        Task<List<IManagedProperty>> GetSearchConfigurationManagedPropertiesAsync();

        /// <summary>
        /// Gets the managed properties from the search configuration of this site
        /// </summary>
        /// <returns>List of managed properties</returns>
        List<IManagedProperty> GetSearchConfigurationManagedProperties();

        /// <summary>
        /// Sets the search configuration for the site
        /// </summary>
        /// <param name="configuration">Search configuration, obtained via <see cref="GetSearchConfigurationXml"/> to apply</param>
        Task SetSearchConfigurationXmlAsync(string configuration);

        /// <summary>
        /// Sets the search configuration for the site
        /// </summary>
        /// <param name="configuration">Search configuration, obtained via <see cref="GetSearchConfigurationXml"/> to apply</param>
        void SetSearchConfigurationXml(string configuration);

    }
}
