using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Site object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Site : BaseDataModel<ISite>, ISite
    {
        [GraphProperty("sharepointIds", JsonPath = "siteId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public IWeb RootWeb { get => GetModelValue<IWeb>(); set => SetModelValue(value); }

        private IWebCollection allWebs;

        // Note: AllWebs is no real property in SharePoint, so expand/select do not return a thing...
        // TODO: evaluate why we need this
        public IWebCollection AllWebs
        {
            get
            {
                if (allWebs == null)
                {
                    allWebs = new WebCollection(this.PnPContext, this, "AllWebs");
                }

                return allWebs;
            }
            set
            {
                allWebs = value;
            }
        }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        /* Not directly a field in the Site object*/
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

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

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public int CompatibilityLevel { get => GetValue<int>(); set => SetValue(value); }

        public string ComplianceAttribute { get => GetValue<string>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableCompanyWideSharingLinks { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExternalSharingTipsEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int ExternalUserExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public string GeoLocation { get => GetValue<string>(); set => SetValue(value); }

        public Guid HubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool IsHubSite { get => GetValue<bool>(); set => SetValue(value); }

        public string LockIssue { get => GetValue<string>(); set => SetValue(value); }

        public int MaxItemsPerThrottledOperation { get => GetValue<int>(); set => SetValue(value); }

        public bool NeedsB2BUpgrade { get => GetValue<bool>(); set => SetValue(value); }

        public Uri PrimaryUri { get => GetValue<Uri>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public Guid RelatedGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }

        public string RequiredDesignerVersion { get => GetValue<string>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public string SensitivityLabelId { get => GetValue<string>(); set => SetValue(value); }

        public Guid SensitivityLabel { get => GetValue<Guid>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShareByLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowPeoplePickerSuggestionsForGuestUsers { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowUrlStructure { get => GetValue<bool>(); set => SetValue(value); }

        public string StatusBarLink { get => GetValue<string>(); set => SetValue(value); }

        public string StatusBarText { get => GetValue<string>(); set => SetValue(value); }

        public bool ThicketSupportDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool TrimAuditLog { get => GetValue<bool>(); set => SetValue(value); }

        public bool UIVersionConfigurationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime UpgradeReminderDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool UpgradeScheduled { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime UpgradeScheduledDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool Upgrading { get => GetValue<bool>(); set => SetValue(value); }

        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
