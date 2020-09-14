using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Site object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Site))]
    public interface ISite : IDataModel<ISite>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the Site object
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The unique ID of the connected Microsoft 365 Group (if any)
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
        /// Define if the suitebar search box should show or not 
        /// </summary>
        public SearchBoxInNavBar SearchBoxInNavBar { get; set; }

        /// <summary>
        /// Defines the Search Center URL
        /// </summary>
        public string SearchCenterUrl { get; set; }

        /// <summary>
        /// The RootWeb of the Site object
        /// </summary>
        public IWeb RootWeb { get; }

        /// <summary>
        /// Collection of sub-webs in the current Site object
        /// </summary>
        public IWebCollection AllWebs { get; }

        /// <summary>
        /// Collection of features enabled for the site
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
        /// Gets or sets a value that specifies whether version to version upgrade is allowed on this site collection.
        /// </summary>
        public bool AllowSelfServiceUpgrade { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether upgrade evaluation site collection is allowed on this site collection.
        /// </summary>
        public bool AllowSelfServiceUpgradeEvaluation { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the audit log trimming retention on this site.
        /// </summary>
        public int AuditLogTrimmingRetention { get; set; }

        // TODO: Review if readonly
        // TODO: Review this property docs
        /// <summary>
        /// Gets a value that specifies whether the site collections has permissions to sync to hub site.
        /// </summary>
        public bool CanSyncHubSitePermissions { get; }

        /// <summary>
        /// Property indicating whether or not this object can be upgraded.
        /// </summary>
        public bool CanUpgrade { get; }

        // TODO: Review if readonly
        // TODO: Review this property docs
        /// <summary>
        /// Gets the Id of the channel group.
        /// </summary>
        public Guid ChannelGroupId { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the comments on site pages are disabled on this site collection.
        /// </summary>
        public bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Gets the major version of this site collection for purposes of major version-level compatibility checks.
        /// </summary>
        public int CompatibilityLevel { get; }

        /// <summary>
        /// Gets or sets the compliance attribute of this site collection.
        /// </summary>
        public string ComplianceAttribute { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the app views are disabled on this site collection.
        /// </summary>
        public bool DisableAppViews { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the company-wide sharing links are disabled on this site collection.
        /// </summary>
        public bool DisableCompanyWideSharingLinks { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether Flows are disabled on this site collection.
        /// </summary>
        public bool DisableFlows { get; set; }

        /// <summary>
        /// Gets a value that indicates whether external sharing tips are enabled.
        /// </summary>
        public bool ExternalSharingTipsEnabled { get; }

        // TODO: Review if readonly
        /// <summary>
        /// Gets or sets the expiration in days for external users on this site collection.
        /// </summary>
        public int ExternalUserExpirationInDays { get; set; }

        /// <summary>
        /// Gets the geolocation of this site collection.
        /// </summary>
        public string GeoLocation { get; }

        /// <summary>
        /// Gets the Id of the Hub Site this site collection is connected to.
        /// </summary>
        public Guid HubSiteId { get; }

        // TODO: Review if readonly
        /// <summary>
        /// Gets or sets the Id (as String) of the sensitivity label of this site collection.
        /// </summary>
        public string SensitivityLabelId { get; set; }

        // TODO: Review this property docs
        // TODO: Review if readonly
        /// <summary>
        /// Gets or sets the Id (as Guid) of the sensitivity label of this site collection
        /// </summary>
        public Guid SensitivityLabel { get; set; }

        /// <summary>
        /// Gets a value that indicates whether this site collection is a Hub Site.
        /// </summary>
        public bool IsHubSite { get; }

        /// <summary>
        /// Gets or sets the comment that is used in locking a site collection.
        /// </summary>
        public string LockIssue { get; set; }

        /// <summary>
        /// Gets a value that specifies the maximum number of list items allowed per operation before throttling will occur.
        /// </summary>
        public int MaxItemsPerThrottledOperation { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether this site collection needs a B2B upgrade.
        /// </summary>
        public bool NeedsB2BUpgrade { get; set; }

        /// <summary>
        /// Specifies the primary URI of this site collection, including the host name, port number, and path.
        /// </summary>
        public Uri PrimaryUri { get; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the site collection is read-only, locked, and unavailable for write access.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public bool ReadOnly { get; set; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Gets the Id of the group related to this site collection.
        /// </summary>
        public Guid RelatedGroupId { get; }

        /// <summary>
        /// Gets a value that specifies the collection of Recycle Bin items for the site collection.
        /// </summary>
        public IRecycleBinItemCollection RecycleBin { get; }

        /// <summary>
        /// Gets a value that indicates the required Designer version for this site collection.
        /// </summary>
        public string RequiredDesignerVersion { get; }

        // TODO Probably not expecting this to be implemented, to decide whether include it or not
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public int SandboxedCodeActivationCapability { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the placeholder text of this site collection search box.
        /// </summary>
        public string SearchBoxPlaceholderText { get; set; }

        /// <summary>
        /// Gets the server-relative URL of the root Web site in the site collection.
        /// </summary>
        public string ServerRelativeUrl { get; }

        // TODO: Review if readonly
        /// <summary>
        /// Gets or sets a value that specifies whether sharing by e-mail is enabled on this site collection.
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// Property that indicates whether users will be able to share links to documents that can be accessed without logging in.
        /// </summary>
        public bool ShareByLinkEnabled { get;  }

        /// <summary>
        /// Gets or sets a value that specifies whether guest users should be displayed as suggestions in people picker on this site collection.
        /// </summary>
        public bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the URL structure of this site collection is viewable.
        /// </summary>
        public bool ShowUrlStructure { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the status bar link on this site collection.
        /// </summary>
        public string StatusBarLink { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the text of the status bar on this site collection.
        /// </summary>
        public string StatusBarText { get; set; }

        // TODO: Review this property docs (No clue what thicket support is)
        /// <summary>
        /// Gets a value that indicates whether thicket support is disabled on this site collection.
        /// </summary>
        public bool ThicketSupportDisabled { get;  }

        /// <summary>
        /// Gets or sets a value that specifies whether audit log is trimmed.
        /// </summary>
        public bool TrimAuditLog { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the Visual Upgrade UI of this site collection is displayed.
        /// </summary>
        public bool UIVersionConfigurationEnabled { get; set; }

        /// <summary>
        /// Specifies a date, after which site collection administrators will be reminded to upgrade the site collection.
        /// </summary>
        public DateTime UpgradeReminderDate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies if upgrade is scheduled on this site collection.
        /// </summary>
        public bool UpgradeScheduled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the scheduled date of this site colleection upgrage.
        /// </summary>
        public DateTime UpgradeScheduledDate { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the site is currently upgrading.
        /// </summary>
        public bool Upgrading { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IAudit Audit { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IScriptSafeDomainCollection CustomScriptSafeDomains { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IEventReceiverDefinitionCollection EventReceivers { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IGroup HubSiteSynchronizableVisitorGroup { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser Owner { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUser SecondaryContact { get; }

        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IUserCustomActionCollection UserCustomActions { get; }
    }
}
