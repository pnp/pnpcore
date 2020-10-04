using PnP.Core.Model.Security;
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
        [GraphProperty("sharepointIds", JsonPath = "webId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        // TODO: Can't find official documentation about this one, guessed it's read-only but not sure
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

        public FooterVariantThemeType FooterEmphasis { get => GetValue<FooterVariantThemeType>(); set => SetValue(value); }

        public bool FooterEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public FooterLayoutType FooterLayout { get => GetValue<FooterLayoutType>(); set => SetValue(value); }

        public VariantThemeType HeaderEmphasis { get => GetValue<VariantThemeType>(); set => SetValue(value); }

        public HeaderLayoutType HeaderLayout { get => GetValue<HeaderLayoutType>(); set => SetValue(value); }

        public bool HideTitleInHeader { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHomepageModernized { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsProvisioningComplete { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRevertHomepageLinkHidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Language { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public LogoAlignment LogoAlignment { get => GetValue<LogoAlignment>(); set => SetValue(value); }

        public string MasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool MegaMenuEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NavAudienceTargetingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NextStepsFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInOneDriveForBusinessEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInSharePointEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ObjectCacheEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool PreviewFeaturesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string PrimaryColor { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("RecycleBin", Expandable = true)]
        public IRecycleBinItemCollection RecycleBin
        {
            get
            {
                if (!HasValue(nameof(RecycleBin)))
                {
                    var recycleBin = new RecycleBinItemCollection(this.PnPContext, this, nameof(RecycleBin));
                    SetValue(recycleBin);
                }
                return GetValue<IRecycleBinItemCollection>();
            }
        }

        public bool RecycleBinEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool SaveSiteAsTemplateEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ShowUrlStructureForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public string SiteLogoDescription { get => GetValue<string>(); set => SetValue(value); }

        public string SiteLogoUrl { get => GetValue<string>(); set => SetValue(value); }

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

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl")]
        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        // Below attribute was used as sample to test the UseCustomMapping feature
        //[SharePointFieldMapping(FieldName = "NoCrawl", UseCustomMapping = true)]
        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public string RequestAccessEmail { get => GetValue<string>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public string AlternateCssUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public string CustomMasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool QuickLaunchEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMultilingual { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverwriteTranslationsOnChange { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool MembersCanShare { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool HorizontalQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        public SearchScope SearchScope { get => GetValue<SearchScope>(); set => SetValue(value); }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        // Not in Web object, requires extra work to load
        public string SearchCenterUrl { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Fields", Expandable = true)]
        public IFieldCollection Fields
        {
            get
            {
                if (!HasValue(nameof(Fields)))
                {
                    var fields = new FieldCollection(this.PnPContext, this, nameof(Fields));
                    SetValue(fields);
                }
                return GetValue<IFieldCollection>();
            }
        }

        [SharePointProperty("AvailableFields", Expandable = true)]
        public IFieldCollection AvailableFields
        {
            get
            {
                if (!HasValue(nameof(AvailableFields)))
                {
                    var fields = new FieldCollection(this.PnPContext, this, nameof(AvailableFields));
                    SetValue(fields);
                }
                return GetValue<IFieldCollection>();
            }
        }

        [SharePointProperty("Lists", Expandable = true)]
        // A special approach is needed to load all lists, comes down to adding the "system" facet to the select
        [GraphProperty("lists", Get = "sites/{hostname}:{serverrelativepath}:/lists?$select=" + List.DefaultGraphFieldsToLoad, Expandable = true)]
        public IListCollection Lists
        {
            get
            {
                if (!HasValue(nameof(Lists)))
                {
                    var lists = new ListCollection(this.PnPContext, this, nameof(Lists));
                    SetValue(lists);
                }
                return GetValue<IListCollection>();
            }
        }

        [SharePointProperty("ContentTypes", Expandable = true)]
        public IContentTypeCollection ContentTypes
        {
            get
            {
                if (!HasValue(nameof(ContentTypes)))
                {
                    var contentTypes = new ContentTypeCollection(this.PnPContext, this, nameof(ContentTypes));
                    SetValue(contentTypes);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

        [SharePointProperty("AvailableContentTypes", Expandable = true)]
        public IContentTypeCollection AvailableContentTypes
        {
            get
            {
                if (!HasValue(nameof(AvailableContentTypes)))
                {
                    var contentTypes = new ContentTypeCollection(this.PnPContext, this, nameof(AvailableContentTypes));
                    SetValue(contentTypes);
                }
                return GetValue<IContentTypeCollection>();
            }
        }

        [SharePointProperty("Webs", Expandable = true)]
        public IWebCollection Webs
        {
            get
            {
                if (!HasValue(nameof(Webs)))
                {
                    var webs = new WebCollection(this.PnPContext, this);
                    SetValue(webs);
                }
                return GetValue<IWebCollection>();
            }
        }

        [SharePointProperty("SiteUserInfoList", Expandable = true)]
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


        [SharePointProperty("RootFolder", Expandable = true)]
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

        [SharePointProperty("Folders", Expandable = true)]
        public IFolderCollection Folders
        {
            get
            {
                if (!HasValue(nameof(Folders)))
                {
                    var folders = new FolderCollection(this.PnPContext, this, nameof(Folders));
                    SetValue(folders);
                }
                return GetValue<IFolderCollection>();
            }
        }

        [SharePointProperty("AllProperties", Expandable = true)]
        public IPropertyValues AllProperties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PropertyValues();
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

        [SharePointProperty("CurrentUser", Expandable = true)]
        public ISharePointUser CurrentUser
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointUser
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointUser>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);
            }
        }

        [SharePointProperty("SiteUsers", Expandable = true)]
        public ISharePointUserCollection SiteUsers
        {
            get
            {
                if (!HasValue(nameof(SiteUsers)))
                {
                    var users = new SharePointUserCollection(this.PnPContext, this, nameof(SiteUsers));
                    SetValue(users);
                }
                return GetValue<ISharePointUserCollection>();
            }
        }

        [SharePointProperty("SiteGroups", Expandable = true)]
        public ISharePointGroupCollection SiteGroups
        {
            get
            {
                if (!HasValue(nameof(SiteGroups)))
                {
                    var groups = new SharePointGroupCollection(this.PnPContext, this, nameof(SiteGroups));
                    SetValue(groups);
                }
                return GetValue<ISharePointGroupCollection>();
            }
        }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
