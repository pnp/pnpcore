using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Web object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Web : BaseDataModel<IWeb>, IWeb
    {

        #region Existing properties

        [SharePointProperty("AlternateCssUrl")]
        public string AlternateCSS { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("CustomMasterUrl")]
        public string CustomMasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool HorizontalQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("sharepointIds", JsonPath = "webId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public bool IsMultilingual { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("MasterUrl")]
        public string MasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool MembersCanShare { get => GetValue<bool>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverwriteTranslationsOnChange { get => GetValue<bool>(); set => SetValue(value); }

        public bool QuickLaunchEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string RequestAccessEmail { get => GetValue<string>(); set => SetValue(value); }

        public int SearchBoxInNavBar { get => GetValue<int>(); set => SetValue(value); }

        public int SearchScope { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("SiteLogoUrl")]
        public string SiteLogo { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("name")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl")]
        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("ContentTypes", Expandable = true)]
        public IContentTypeCollection ContentTypes
        {
            get
            {
                if (!HasValue(nameof(ContentTypes)))
                {
                    var collection = new ContentTypeCollection(this.PnPContext, this, nameof(ContentTypes));
                    SetValue(collection);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

        [SharePointProperty("Fields", Expandable = true)]
        public IFieldCollection Fields
        {
            get
            {
                if (!HasValue(nameof(Fields)))
                {
                    var collection = new FieldCollection(this.PnPContext, this, nameof(Fields));
                    SetValue(collection);
                }
                return GetValue<IFieldCollection>();
            }
        }

        [SharePointProperty("Lists", Expandable = true)]
        [GraphProperty("lists", Expandable = true)]
        public IListCollection Lists
        {
            get
            {
                if (!HasValue(nameof(Lists)))
                {
                    var collection = new ListCollection(this.PnPContext, this, nameof(Lists));
                    SetValue(collection);
                }
                return GetValue<IListCollection>();
            }
        }

        [SharePointProperty("Webs", Expandable = true)]
        public IWebCollection Webs
        {
            get
            {
                if (!HasValue(nameof(Webs)))
                {
                    var collection = new WebCollection(this.PnPContext, this, nameof(Webs));
                    SetValue(collection);
                }
                return GetValue<IWebCollection>();
            }
        }

        #endregion

        #region New properties

        public string AccessRequestListUrl { get => GetValue<string>(); set => SetValue(value); }

        public string AccessRequestSiteDescription { get => GetValue<string>(); set => SetValue(value); }

        public bool AllowAutomaticASPXPageIndexing { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowCreateDeclarativeWorkflowForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDesignerForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowMasterPageEditingForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRevertFromTemplateForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRssFeeds { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSavePublishDeclarativeWorkflowForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public Guid AppInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClassicWelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public bool ContainsConfidentialInfo { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool CustomSiteActionsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public Guid DefaultNewPageTemplateId { get => GetValue<Guid>(); set => SetValue(value); }

        public string DesignerDownloadUrlForCurrentUser { get => GetValue<string>(); set => SetValue(value); }

        public Guid DesignPackageId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool DisableRecommendedItems { get => GetValue<bool>(); set => SetValue(value); }

        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinimalDownload { get => GetValue<bool>(); set => SetValue(value); }

        public int FooterEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public bool FooterEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int FooterLayout { get => GetValue<int>(); set => SetValue(value); }

        public int HeaderEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public int HeaderLayout { get => GetValue<int>(); set => SetValue(value); }

        public bool HideTitleInHeader { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHomepageModernized { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsProvisioningComplete { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRevertHomepageLinkHidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Language { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public int LogoAlignment { get => GetValue<int>(); set => SetValue(value); }

        public bool MegaMenuEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NavAudienceTargetingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NextStepsFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInOneDriveForBusinessEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInSharePointEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ObjectCacheEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool PreviewFeaturesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string PrimaryColor { get => GetValue<string>(); set => SetValue(value); }

        public bool RecycleBinEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool SaveSiteAsTemplateEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShowUrlStructureForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public string SiteLogoDescription { get => GetValue<string>(); set => SetValue(value); }

        public bool SyndicationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int TenantAdminMembersCanShare { get => GetValue<int>(); set => SetValue(value); }

        public bool TenantTagPolicyEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string ThemeData { get => GetValue<string>(); set => SetValue(value); }

        public string ThemedCssFolderUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ThirdPartyMdmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool TreeViewEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int UIVersion { get => GetValue<int>(); set => SetValue(value); }

        public bool UIVersionConfigurationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool UseAccessRequestDefault { get => GetValue<bool>(); set => SetValue(value); }

        public string WebTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string WebTemplateConfiguration { get => GetValue<string>(); set => SetValue(value); }

        public bool WebTemplatesGalleryFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("Activities", Expandable = true)]
        public IActivityEntityCollection Activities
        {
            get
            {
                if (!HasValue(nameof(Activities)))
                {
                    var collection = new ActivityEntityCollection(this.PnPContext, this, nameof(Activities));
                    SetValue(collection);
                }
                return GetValue<IActivityEntityCollection>();
            }
        }

        public IActivityLogger ActivityLogger
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ActivityLogger
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IActivityLogger>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("Alerts", Expandable = true)]
        public IAlertCollection Alerts
        {
            get
            {
                if (!HasValue(nameof(Alerts)))
                {
                    var collection = new AlertCollection(this.PnPContext, this, nameof(Alerts));
                    SetValue(collection);
                }
                return GetValue<IAlertCollection>();
            }
        }

        public IPropertyValues AllProperties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PropertyValues
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPropertyValues>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("AppTiles", Expandable = true)]
        public IAppTileCollection AppTiles
        {
            get
            {
                if (!HasValue(nameof(AppTiles)))
                {
                    var collection = new AppTileCollection(this.PnPContext, this, nameof(AppTiles));
                    SetValue(collection);
                }
                return GetValue<IAppTileCollection>();
            }
        }

        public IGroup AssociatedMemberGroup
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


        public IGroup AssociatedOwnerGroup
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


        public IGroup AssociatedVisitorGroup
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


        public IUser Author
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


        [SharePointProperty("AvailableContentTypes", Expandable = true)]
        public IContentTypeCollection AvailableContentTypes
        {
            get
            {
                if (!HasValue(nameof(AvailableContentTypes)))
                {
                    var collection = new ContentTypeCollection(this.PnPContext, this, nameof(AvailableContentTypes));
                    SetValue(collection);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

        [SharePointProperty("AvailableFields", Expandable = true)]
        public IFieldCollection AvailableFields
        {
            get
            {
                if (!HasValue(nameof(AvailableFields)))
                {
                    var collection = new FieldCollection(this.PnPContext, this, nameof(AvailableFields));
                    SetValue(collection);
                }
                return GetValue<IFieldCollection>();
            }
        }

        public IModernizeHomepageResult CanModernizeHomepage
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ModernizeHomepageResult
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IModernizeHomepageResult>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("ClientWebParts", Expandable = true)]
        public IClientWebPartCollection ClientWebParts
        {
            get
            {
                if (!HasValue(nameof(ClientWebParts)))
                {
                    var collection = new ClientWebPartCollection(this.PnPContext, this, nameof(ClientWebParts));
                    SetValue(collection);
                }
                return GetValue<IClientWebPartCollection>();
            }
        }

        public IUser CurrentUser
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


        public IDataLeakagePreventionStatusInfo DataLeakagePreventionStatusInfo
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new DataLeakagePreventionStatusInfo
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IDataLeakagePreventionStatusInfo>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IUserResource DescriptionResource
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new UserResource
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUserResource>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
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

        [SharePointProperty("Folders", Expandable = true)]
        public IFolderCollection Folders
        {
            get
            {
                if (!HasValue(nameof(Folders)))
                {
                    var collection = new FolderCollection(this.PnPContext, this, nameof(Folders));
                    SetValue(collection);
                }
                return GetValue<IFolderCollection>();
            }
        }

        public IHostedAppsManager HostedApps
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new HostedAppsManager
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IHostedAppsManager>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("ListTemplates", Expandable = true)]
        public IListTemplateCollection ListTemplates
        {
            get
            {
                if (!HasValue(nameof(ListTemplates)))
                {
                    var collection = new ListTemplateCollection(this.PnPContext, this, nameof(ListTemplates));
                    SetValue(collection);
                }
                return GetValue<IListTemplateCollection>();
            }
        }

        public IMultilingualSettings MultilingualSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new MultilingualSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IMultilingualSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public INavigation Navigation
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Navigation
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<INavigation>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("OneDriveSharedItems", Expandable = true)]
        public ISharedDocumentInfoCollection OneDriveSharedItems
        {
            get
            {
                if (!HasValue(nameof(OneDriveSharedItems)))
                {
                    var collection = new SharedDocumentInfoCollection(this.PnPContext, this, nameof(OneDriveSharedItems));
                    SetValue(collection);
                }
                return GetValue<ISharedDocumentInfoCollection>();
            }
        }

        public IWebInformation ParentWeb
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new WebInformation
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IWebInformation>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("PushNotificationSubscribers", Expandable = true)]
        public IPushNotificationSubscriberCollection PushNotificationSubscribers
        {
            get
            {
                if (!HasValue(nameof(PushNotificationSubscribers)))
                {
                    var collection = new PushNotificationSubscriberCollection(this.PnPContext, this, nameof(PushNotificationSubscribers));
                    SetValue(collection);
                }
                return GetValue<IPushNotificationSubscriberCollection>();
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

        public IRegionalSettings RegionalSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new RegionalSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IRegionalSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("RoleDefinitions", Expandable = true)]
        public IRoleDefinitionCollection RoleDefinitions
        {
            get
            {
                if (!HasValue(nameof(RoleDefinitions)))
                {
                    var collection = new RoleDefinitionCollection(this.PnPContext, this, nameof(RoleDefinitions));
                    SetValue(collection);
                }
                return GetValue<IRoleDefinitionCollection>();
            }
        }

        public IFolder RootFolder
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new Folder
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IFolder>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public ISiteCollectionCorporateCatalogAccessor SiteCollectionAppCatalog
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SiteCollectionCorporateCatalogAccessor
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISiteCollectionCorporateCatalogAccessor>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("SiteGroups", Expandable = true)]
        public IGroupCollection SiteGroups
        {
            get
            {
                if (!HasValue(nameof(SiteGroups)))
                {
                    var collection = new GroupCollection(this.PnPContext, this, nameof(SiteGroups));
                    SetValue(collection);
                }
                return GetValue<IGroupCollection>();
            }
        }

        public IList SiteUserInfoList
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new List
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IList>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("SiteUsers", Expandable = true)]
        public IUserCollection SiteUsers
        {
            get
            {
                if (!HasValue(nameof(SiteUsers)))
                {
                    var collection = new UserCollection(this.PnPContext, this, nameof(SiteUsers));
                    SetValue(collection);
                }
                return GetValue<IUserCollection>();
            }
        }

        public ITenantCorporateCatalogAccessor TenantAppCatalog
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new TenantCorporateCatalogAccessor
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ITenantCorporateCatalogAccessor>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IThemeInfo ThemeInfo
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ThemeInfo
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IThemeInfo>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IUserResource TitleResource
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new UserResource
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IUserResource>();
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

        [SharePointProperty("WebInfos", Expandable = true)]
        public IWebInformationCollection WebInfos
        {
            get
            {
                if (!HasValue(nameof(WebInfos)))
                {
                    var collection = new WebInformationCollection(this.PnPContext, this, nameof(WebInfos));
                    SetValue(collection);
                }
                return GetValue<IWebInformationCollection>();
            }
        }

        [SharePointProperty("WorkflowAssociations", Expandable = true)]
        public IWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                if (!HasValue(nameof(WorkflowAssociations)))
                {
                    var collection = new WorkflowAssociationCollection(this.PnPContext, this, nameof(WorkflowAssociations));
                    SetValue(collection);
                }
                return GetValue<IWorkflowAssociationCollection>();
            }
        }

        [SharePointProperty("WorkflowTemplates", Expandable = true)]
        public IWorkflowTemplateCollection WorkflowTemplates
        {
            get
            {
                if (!HasValue(nameof(WorkflowTemplates)))
                {
                    var collection = new WorkflowTemplateCollection(this.PnPContext, this, nameof(WorkflowTemplates));
                    SetValue(collection);
                }
                return GetValue<IWorkflowTemplateCollection>();
            }
        }

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
