using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Site class, write your custom code here
    /// </summary>
    [SharePointType("SP.Site", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Site : BaseDataModel<ISite>, ISite
    {
        #region Construction
        public Site()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public bool AllowCreateDeclarativeWorkflow { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDesigner { get => GetValue<bool>(); set => SetValue(value); }

        public int AllowExternalEmbeddingWrapper { get => GetValue<int>(); set => SetValue(value); }

        public bool AllowMasterPageEditing { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRevertFromTemplate { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSaveDeclarativeWorkflowAsTemplate { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSavePublishDeclarativeWorkflow { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSelfServiceUpgrade { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSelfServiceUpgradeEvaluation { get => GetValue<bool>(); set => SetValue(value); }

        public int AuditLogTrimmingRetention { get => GetValue<int>(); set => SetValue(value); }

        public bool CanSyncHubSitePermissions { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanUpgrade { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ChannelGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public int CompatibilityLevel { get => GetValue<int>(); set => SetValue(value); }

        public string ComplianceAttribute { get => GetValue<string>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableCompanyWideSharingLinks { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExternalSharingTipsEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int ExternalUserExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public string GeoLocation { get => GetValue<string>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid HubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("sharepointIds", JsonPath = "siteId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string SensitivityLabelId { get => GetValue<string>(); set => SetValue(value); }

        public Guid SensitivityLabel { get => GetValue<Guid>(); set => SetValue(value); }

        public bool IsHubSite { get => GetValue<bool>(); set => SetValue(value); }

        public string LockIssue { get => GetValue<string>(); set => SetValue(value); }

        public int MaxItemsPerThrottledOperation { get => GetValue<int>(); set => SetValue(value); }

        public bool NeedsB2BUpgrade { get => GetValue<bool>(); set => SetValue(value); }

        public string PrimaryUri { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public Guid RelatedGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public string RequiredDesignerVersion { get => GetValue<string>(); set => SetValue(value); }

        public int SearchBoxInNavBar { get => GetValue<int>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowPeoplePickerSuggestionsForGuestUsers { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowUrlStructure { get => GetValue<bool>(); set => SetValue(value); }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string StatusBarLink { get => GetValue<string>(); set => SetValue(value); }

        public string StatusBarText { get => GetValue<string>(); set => SetValue(value); }

        public bool ThicketSupportDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool TrimAuditLog { get => GetValue<bool>(); set => SetValue(value); }

        public bool UIVersionConfigurationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime UpgradeReminderDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool UpgradeScheduled { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime UpgradeScheduledDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool Upgrading { get => GetValue<bool>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }


        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }


        public IWeb RootWeb { get => GetModelValue<IWeb>(); }


        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }


        #endregion

        #region New properties

        public int SandboxedCodeActivationCapability { get => GetValue<int>(); set => SetValue(value); }

        public IAudit Audit { get => GetModelValue<IAudit>(); }


        public IScriptSafeDomainCollection CustomScriptSafeDomains { get => GetModelCollectionValue<IScriptSafeDomainCollection>(); }


        public IEventReceiverDefinitionCollection EventReceivers { get => GetModelCollectionValue<IEventReceiverDefinitionCollection>(); }


        public IGroup HubSiteSynchronizableVisitorGroup { get => GetModelValue<IGroup>(); }


        public IUser Owner { get => GetModelValue<IUser>(); }


        public IUser SecondaryContact { get => GetModelValue<IUser>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
