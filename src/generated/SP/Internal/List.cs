using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List class, write your custom code here
    /// </summary>
    [SharePointType("SP.List", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class List : BaseDataModel<IList>, IList
    {
        #region Construction
        public List()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        [SharePointProperty("BaseTemplate")]
        [GraphProperty("list", JsonPath = "template")]
        public int TemplateType { get => GetValue<int>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "contentTypesEnabled")]
        public bool ContentTypesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string DefaultDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string Direction { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("DocumentTemplateUrl")]
        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public int DraftVersionVisibility { get => GetValue<int>(); set => SetValue(value); }

        public bool EnableAttachments { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableFolderCreation { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinorVersions { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableModeration { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableVersioning { get => GetValue<bool>(); set => SetValue(value); }

        public bool ForceCheckout { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "hidden")]
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmExpire { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmReject { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsApplicationList { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("ListExperienceOptions")]
        public int ListExperience { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("MajorVersionLimit")]
        public int MaxVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("MajorWithMinorVersionsLimit")]
        public int MinorVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public bool OnQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        public int ReadSecurity { get => GetValue<int>(); set => SetValue(value); }

        public Guid TemplateFeatureId { get => GetValue<Guid>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public int WriteSecurity { get => GetValue<int>(); set => SetValue(value); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }


        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }


        public IInformationRightsManagementSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementSettings>(); }


        [GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items { get => GetModelCollectionValue<IListItemCollection>(); }


        public IFolder RootFolder { get => GetModelValue<IFolder>(); }


        public IViewCollection Views { get => GetModelCollectionValue<IViewCollection>(); }


        #endregion

        #region New properties

        public string AdditionalUXProperties { get => GetValue<string>(); set => SetValue(value); }

        public bool AllowContentTypes { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDeletion { get => GetValue<bool>(); set => SetValue(value); }

        public int BaseType { get => GetValue<int>(); set => SetValue(value); }

        public int BrowserFileHandling { get => GetValue<int>(); set => SetValue(value); }

        public string Color { get => GetValue<string>(); set => SetValue(value); }

        public bool CrawlNonDefaultViews { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid DefaultContentApprovalWorkflowId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool DefaultItemOpenInBrowser { get => GetValue<bool>(); set => SetValue(value); }

        public bool DefaultItemOpenUseListSetting { get => GetValue<bool>(); set => SetValue(value); }

        public string DefaultViewUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool DisableCommenting { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableGridEditing { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableAssignToEmail { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableRequestSignOff { get => GetValue<bool>(); set => SetValue(value); }

        public string EntityTypeName { get => GetValue<string>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExemptFromBlockDownloadOfNonViewableFiles { get => GetValue<bool>(); set => SetValue(value); }

        public bool FileSavePostProcessingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasExternalDataSource { get => GetValue<bool>(); set => SetValue(value); }

        public string Icon { get => GetValue<string>(); set => SetValue(value); }

        public bool IsCatalog { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDefaultDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsEnterpriseGalleryLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsPrivate { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAssetsLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSystemList { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemDeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool ListFormCustomized { get => GetValue<bool>(); set => SetValue(value); }

        public string ListItemEntityTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public int ListSchemaVersion { get => GetValue<int>(); set => SetValue(value); }

        public bool MultipleDataList { get => GetValue<bool>(); set => SetValue(value); }

        public int PageRenderType { get => GetValue<int>(); set => SetValue(value); }

        public string ParentWebUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ParserDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public bool ServerTemplateCanCreateFolders { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowHiddenFieldsInModernForm { get => GetValue<bool>(); set => SetValue(value); }

        public string TemplateTypeId { get => GetValue<string>(); set => SetValue(value); }

        public IUser Author { get => GetModelValue<IUser>(); }


        public ICreatablesInfo CreatablesInfo { get => GetModelValue<ICreatablesInfo>(); }


        public IView DefaultView { get => GetModelValue<IView>(); }


        public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }


        public IEventReceiverDefinitionCollection EventReceivers { get => GetModelCollectionValue<IEventReceiverDefinitionCollection>(); }


        public IFormCollection Forms { get => GetModelCollectionValue<IFormCollection>(); }


        public IWeb ParentWeb { get => GetModelValue<IWeb>(); }


        public ISubscriptionCollection Subscriptions { get => GetModelCollectionValue<ISubscriptionCollection>(); }


        public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }


        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }


        public IWorkflowAssociationCollection WorkflowAssociations { get => GetModelCollectionValue<IWorkflowAssociationCollection>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
