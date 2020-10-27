using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Web class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Web : BaseDataModel<IWeb>, IWeb
    {
        #region Construction
        public Web()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

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

        public string AlternateCssUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid AppInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClassicWelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ContainsConfidentialInfo { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public string CustomMasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool CustomSiteActionsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public Guid DefaultNewPageTemplateId { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DesignerDownloadUrlForCurrentUser { get => GetValue<string>(); set => SetValue(value); }

        public Guid DesignPackageId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableRecommendedItems { get => GetValue<bool>(); set => SetValue(value); }

        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinimalDownload { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public int FooterEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public bool FooterEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int FooterLayout { get => GetValue<int>(); set => SetValue(value); }

        public int HeaderEmphasis { get => GetValue<int>(); set => SetValue(value); }

        public int HeaderLayout { get => GetValue<int>(); set => SetValue(value); }

        public bool HideTitleInHeader { get => GetValue<bool>(); set => SetValue(value); }

        public bool HorizontalQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("sharepointIds", JsonPath = "webId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public bool IsHomepageModernized { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMultilingual { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsProvisioningComplete { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRevertHomepageLinkHidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Language { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public int LogoAlignment { get => GetValue<int>(); set => SetValue(value); }

        public string MasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool MegaMenuEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool MembersCanShare { get => GetValue<bool>(); set => SetValue(value); }

        public bool NavAudienceTargetingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NextStepsFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInOneDriveForBusinessEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInSharePointEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ObjectCacheEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverwriteTranslationsOnChange { get => GetValue<bool>(); set => SetValue(value); }

        public bool PreviewFeaturesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string PrimaryColor { get => GetValue<string>(); set => SetValue(value); }

        public bool QuickLaunchEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool RecycleBinEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string RequestAccessEmail { get => GetValue<string>(); set => SetValue(value); }

        public bool SaveSiteAsTemplateEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int SearchBoxInNavBar { get => GetValue<int>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public int SearchScope { get => GetValue<int>(); set => SetValue(value); }

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

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public bool TreeViewEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int UIVersion { get => GetValue<int>(); set => SetValue(value); }

        public bool UIVersionConfigurationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("webUrl")]
        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public bool UseAccessRequestDefault { get => GetValue<bool>(); set => SetValue(value); }

        public string WebTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string WebTemplateConfiguration { get => GetValue<string>(); set => SetValue(value); }

        public bool WebTemplatesGalleryFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public IPropertyValues AllProperties { get => GetModelValue<IPropertyValues>(); }


        public IContentTypeCollection AvailableContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }


        public IFieldCollection AvailableFields { get => GetModelCollectionValue<IFieldCollection>(); }


        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }


        public IUser CurrentUser { get => GetModelValue<IUser>(); }


        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }


        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }


        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }


        [GraphProperty("lists", Get = "sites/{hostname}:{serverrelativepath}:/lists?$select=system,createdDateTime,description,eTag,id,lastModifiedDateTime,name,webUrl,displayName,createdBy,lastModifiedBy,parentReference,list",Expandable = true)]
        public IListCollection Lists { get => GetModelCollectionValue<IListCollection>(); }


        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }


        public IFolder RootFolder { get => GetModelValue<IFolder>(); }


        public IGroupCollection SiteGroups { get => GetModelCollectionValue<IGroupCollection>(); }


        public IList SiteUserInfoList { get => GetModelValue<IList>(); }


        public IUserCollection SiteUsers { get => GetModelCollectionValue<IUserCollection>(); }


        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }


        public IWebCollection Webs { get => GetModelCollectionValue<IWebCollection>(); }


        #endregion

        #region New properties

        public IList AccessRequestsList { get => GetModelValue<IList>(); }


        public IActivityEntityCollection Activities { get => GetModelCollectionValue<IActivityEntityCollection>(); }


        public IActivityLogger ActivityLogger { get => GetModelValue<IActivityLogger>(); }


        public IAlertCollection Alerts { get => GetModelCollectionValue<IAlertCollection>(); }


        public IAppTileCollection AppTiles { get => GetModelCollectionValue<IAppTileCollection>(); }


        public IGroup AssociatedMemberGroup { get => GetModelValue<IGroup>(); }


        public IGroup AssociatedOwnerGroup { get => GetModelValue<IGroup>(); }


        public IGroup AssociatedVisitorGroup { get => GetModelValue<IGroup>(); }


        public IUser Author { get => GetModelValue<IUser>(); }


        public IModernizeHomepageResult CanModernizeHomepage { get => GetModelValue<IModernizeHomepageResult>(); }


        public IClientWebPartCollection ClientWebParts { get => GetModelCollectionValue<IClientWebPartCollection>(); }


        public IDataLeakagePreventionStatusInfo DataLeakagePreventionStatusInfo { get => GetModelValue<IDataLeakagePreventionStatusInfo>(); }


        public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }


        public IEventReceiverDefinitionCollection EventReceivers { get => GetModelCollectionValue<IEventReceiverDefinitionCollection>(); }


        public IHostedAppsManager HostedApps { get => GetModelValue<IHostedAppsManager>(); }


        public IListTemplateCollection ListTemplates { get => GetModelCollectionValue<IListTemplateCollection>(); }


        public IMultilingualSettings MultilingualSettings { get => GetModelValue<IMultilingualSettings>(); }


        public INavigation Navigation { get => GetModelValue<INavigation>(); }


        public ISharedDocumentInfoCollection OneDriveSharedItems { get => GetModelCollectionValue<ISharedDocumentInfoCollection>(); }


        public IWebInformation ParentWeb { get => GetModelValue<IWebInformation>(); }


        public IPushNotificationSubscriberCollection PushNotificationSubscribers { get => GetModelCollectionValue<IPushNotificationSubscriberCollection>(); }


        public IRegionalSettings RegionalSettings { get => GetModelValue<IRegionalSettings>(); }


        public IRoleDefinitionCollection RoleDefinitions { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }


        public ISiteCollectionCorporateCatalogAccessor SiteCollectionAppCatalog { get => GetModelValue<ISiteCollectionCorporateCatalogAccessor>(); }


        public ITenantCorporateCatalogAccessor TenantAppCatalog { get => GetModelValue<ITenantCorporateCatalogAccessor>(); }


        public IThemeInfo ThemeInfo { get => GetModelValue<IThemeInfo>(); }


        public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }


        public IWebInformationCollection WebInfos { get => GetModelCollectionValue<IWebInformationCollection>(); }


        public IWorkflowAssociationCollection WorkflowAssociations { get => GetModelCollectionValue<IWorkflowAssociationCollection>(); }


        public IWorkflowTemplateCollection WorkflowTemplates { get => GetModelCollectionValue<IWorkflowTemplateCollection>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
