using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Web object
    /// </summary>
    [ConcreteType(typeof(Web))]
    public interface IWeb : IDataModel<IWeb>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableAppViews { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableFlows { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ExcludeFromOfflineClient { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HorizontalQuickLaunch { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsMultilingual { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool MembersCanShare { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool OverwriteTranslationsOnChange { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool QuickLaunchEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RequestAccessEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SearchBoxInNavBar { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SearchScope { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WelcomePage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListCollection Lists { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWebCollection Webs { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AccessRequestListUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AccessRequestSiteDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowAutomaticASPXPageIndexing { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowCreateDeclarativeWorkflowForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowDesignerForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowMasterPageEditingForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowRevertFromTemplateForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowRssFeeds { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowSavePublishDeclarativeWorkflowForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AlternateCssUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid AppInstanceId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ClassicWelcomePage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ContainsConfidentialInfo { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomMasterUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CustomSiteActionsDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DefaultNewPageTemplateId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DesignerDownloadUrlForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DesignPackageId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableRecommendedItems { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableMinimalDownload { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FooterEmphasis { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool FooterEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FooterLayout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int HeaderEmphasis { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int HeaderLayout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HideTitleInHeader { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsHomepageModernized { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsProvisioningComplete { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsRevertHomepageLinkHidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Language { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastItemModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastItemUserModifiedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LogoAlignment { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string MasterUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool MegaMenuEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NavAudienceTargetingEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NextStepsFirstRunEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NotificationsInOneDriveForBusinessEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NotificationsInSharePointEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ObjectCacheEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PreviewFeaturesEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PrimaryColor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool RecycleBinEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool SaveSiteAsTemplateEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SearchBoxPlaceholderText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowUrlStructureForCurrentUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteLogoDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteLogoUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool SyndicationEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int TenantAdminMembersCanShare { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool TenantTagPolicyEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThemeData { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ThemedCssFolderUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ThirdPartyMdmEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool TreeViewEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UIVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UIVersionConfigurationEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UseAccessRequestDefault { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebTemplateConfiguration { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool WebTemplatesGalleryFirstRunEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IActivityEntityCollection Activities { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IActivityLogger ActivityLogger { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAlertCollection Alerts { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPropertyValues AllProperties { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IAppTileCollection AppTiles { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroup AssociatedMemberGroup { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroup AssociatedOwnerGroup { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroup AssociatedVisitorGroup { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IContentTypeCollection AvailableContentTypes { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldCollection AvailableFields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IModernizeHomepageResult CanModernizeHomepage { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IClientWebPartCollection ClientWebParts { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CurrentUser { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IDataLeakagePreventionStatusInfo DataLeakagePreventionStatusInfo { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource DescriptionResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IEventReceiverDefinitionCollection EventReceivers { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFeatureCollection Features { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolderCollection Folders { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IHostedAppsManager HostedApps { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListTemplateCollection ListTemplates { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IMultilingualSettings MultilingualSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public INavigation Navigation { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISharedDocumentInfoCollection OneDriveSharedItems { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWebInformation ParentWeb { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPushNotificationSubscriberCollection PushNotificationSubscribers { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRecycleBinItemCollection RecycleBin { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRegionalSettings RegionalSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IRoleDefinitionCollection RoleDefinitions { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolder RootFolder { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISiteCollectionCorporateCatalogAccessor SiteCollectionAppCatalog { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IGroupCollection SiteGroups { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IList SiteUserInfoList { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCollection SiteUsers { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ITenantCorporateCatalogAccessor TenantAppCatalog { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IThemeInfo ThemeInfo { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource TitleResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserCustomActionCollection UserCustomActions { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWebInformationCollection WebInfos { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWorkflowAssociationCollection WorkflowAssociations { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWorkflowTemplateCollection WorkflowTemplates { get; }

        #endregion

    }
}
