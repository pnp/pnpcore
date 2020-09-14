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

        [SharePointProperty("RootWeb", Expandable = true)]
        public IWeb RootWeb
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var rootWeb = new Web
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(rootWeb);
                    InstantiateNavigationProperty();
                }
                return GetValue<IWeb>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }

        private IWebCollection allWebs;

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

        [SharePointProperty("Features", Expandable = true)]
        public IFeatureCollection Features
        {
            get
            {
                if (!HasValue(nameof(Features)))
                {
                    var features = new FeatureCollection(this.PnPContext, this, nameof(Features));
                    SetValue(features);
                }
                return GetValue<IFeatureCollection>();
            }
        }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        /* Not directly a field in the Site object*/
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }



        // ==================== TO TEST ==================================

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

        [SharePointProperty("RecycleBin", Expandable = true)]
        public IRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (!HasValue(nameof(RecycleBin)))
                {
                    var folders = new RecycleBinItemCollection(this.PnPContext, this, nameof(RecycleBin));
                    SetValue(folders);
                }
                return GetValue<IRecycleBinItemCollection>();
            }
        }

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

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
