using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Site object
    /// </summary>
    [ConcreteType(typeof(Site))]
    public interface ISite : IDataModel<ISite>, IDataModelGet<ISite>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowCreateDeclarativeWorkflow { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowDesigner { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AllowExternalEmbeddingWrapper { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowMasterPageEditing { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowRevertFromTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSaveDeclarativeWorkflowAsTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSavePublishDeclarativeWorkflow { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSelfServiceUpgrade { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSelfServiceUpgradeEvaluation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int AuditLogTrimmingRetention { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanSyncHubSitePermissions { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanUpgrade { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ChannelGroupId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int CompatibilityLevel { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ComplianceAttribute { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableAppViews { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableCompanyWideSharingLinks { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableFlows { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ExternalSharingTipsEnabled { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ExternalUserExpirationInDays { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GeoLocation { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid GroupId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid HubSiteId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SensitivityLabelId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SensitivityLabel { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsHubSite { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LockIssue { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MaxItemsPerThrottledOperation { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NeedsB2BUpgrade { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PrimaryUri { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid RelatedGroupId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RequiredDesignerVersion { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SearchBoxInNavBar { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SearchBoxPlaceholderText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShareByLinkEnabled { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowUrlStructure { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool SocialBarOnSitePagesDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StatusBarLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StatusBarText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ThicketSupportDisabled { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool TrimAuditLog { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UIVersionConfigurationEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime UpgradeReminderDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UpgradeScheduled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime UpgradeScheduledDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Upgrading { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFeatureCollection Features { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRecycleBinItemCollection RecycleBin { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWeb RootWeb { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCustomActionCollection UserCustomActions { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int SandboxedCodeActivationCapability { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAudit Audit { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IScriptSafeDomainCollection CustomScriptSafeDomains { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IEventReceiverDefinitionCollection EventReceivers { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroup HubSiteSynchronizableVisitorGroup { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Owner { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser SecondaryContact { get; }

        #endregion

    }
}
