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

        #region Existing properties

        public string Classification { get => GetValue<string>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("sharepointIds", JsonPath = "siteId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public int SearchBoxInNavBar { get => GetValue<int>(); set => SetValue(value); }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("RootWeb", Expandable = true)]
        public IWeb RootWeb
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Web
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
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


        #endregion

        #region New properties

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

        public int SandboxedCodeActivationCapability { get => GetValue<int>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

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

        public IAudit Audit
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Audit
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IAudit>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("CustomScriptSafeDomains", Expandable = true)]
        public IScriptSafeDomainCollection CustomScriptSafeDomains
        {
            get
            {
                if (!HasValue(nameof(CustomScriptSafeDomains)))
                {
                    var collection = new ScriptSafeDomainCollection(this.PnPContext, this, nameof(CustomScriptSafeDomains));
                    SetValue(collection);
                }
                return GetValue<IScriptSafeDomainCollection>();
            }
        }

        [SharePointProperty("EventReceivers", Expandable = true)]
        public IEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (!HasValue(nameof(EventReceivers)))
                {
                    var collection = new EventReceiverDefinitionCollection(this.PnPContext, this, nameof(EventReceivers));
                    SetValue(collection);
                }
                return GetValue<IEventReceiverDefinitionCollection>();
            }
        }

        [SharePointProperty("Features", Expandable = true)]
        public IFeatureCollection Features
        {
            get
            {
                if (!HasValue(nameof(Features)))
                {
                    var collection = new FeatureCollection(this.PnPContext, this, nameof(Features));
                    SetValue(collection);
                }
                return GetValue<IFeatureCollection>();
            }
        }

        public IGroup HubSiteSynchronizableVisitorGroup
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Group
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IGroup>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IUser Owner
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("RecycleBin", Expandable = true)]
        public IRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (!HasValue(nameof(RecycleBin)))
                {
                    var collection = new RecycleBinItemCollection(this.PnPContext, this, nameof(RecycleBin));
                    SetValue(collection);
                }
                return GetValue<IRecycleBinItemCollection>();
            }
        }

        public IUser SecondaryContact
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new User
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("UserCustomActions", Expandable = true)]
        public IUserCustomActionCollection UserCustomActions
        {
            get
            {
                if (!HasValue(nameof(UserCustomActions)))
                {
                    var collection = new UserCustomActionCollection(this.PnPContext, this, nameof(UserCustomActions));
                    SetValue(collection);
                }
                return GetValue<IUserCustomActionCollection>();
            }
        }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
