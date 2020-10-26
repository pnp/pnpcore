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

        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }

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

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IFieldCollection AvailableFields { get => GetModelCollectionValue<IFieldCollection>(); }

        // A special approach is needed to load all lists, comes down to adding the "system" facet to the select
        [GraphProperty("lists", Get = "sites/{hostname}:{serverrelativepath}:/lists?$select=" + List.DefaultGraphFieldsToLoad, Expandable = true)]
        public IListCollection Lists { get => GetModelCollectionValue<IListCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IContentTypeCollection AvailableContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IWebCollection Webs { get => GetModelCollectionValue<IWebCollection>(); }

        public IList SiteUserInfoList { get => GetModelValue<IList>(); }

        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }

        public IFolder RootFolder { get => GetModelValue<IFolder>(); }

        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }

        public IPropertyValues AllProperties { get => GetModelValue<IPropertyValues>(); }

        public ISharePointUser CurrentUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUserCollection SiteUsers { get => GetModelCollectionValue<ISharePointUserCollection>(); }

        public ISharePointGroupCollection SiteGroups { get => GetModelCollectionValue<ISharePointGroupCollection>(); }

        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }

        public IBasePermissions EffectiveBasePermissions { get => GetModelValue<IBasePermissions>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
