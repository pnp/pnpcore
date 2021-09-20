using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a List object
    /// </summary>
    [ConcreteType(typeof(List))]
    public interface IList : IDataModel<IList>, IDataModelGet<IList>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool ContentTypesEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultDisplayFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultEditFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultNewFormUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DraftVersionVisibility { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableAttachments { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableFolderCreation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableMinorVersions { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableModeration { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableVersioning { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ForceCheckout { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmExpire { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmReject { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsApplicationList { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool OnQuickLaunch { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ReadSecurity { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid TemplateFeatureId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int WriteSecurity { get; set; }

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
        public IInformationRightsManagementSettings InformationRightsManagementSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListItemCollection Items { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolder RootFolder { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IViewCollection Views { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AdditionalUXProperties { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowContentTypes { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowDeletion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int BaseTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int BaseType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int BrowserFileHandling { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CrawlNonDefaultViews { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid DefaultContentApprovalWorkflowId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DefaultItemOpenInBrowser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DefaultItemOpenUseListSetting { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultViewUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableCommenting { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableGridEditing { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DocumentTemplateUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableAssignToEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableRequestSignOff { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ExcludeFromOfflineClient { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ExemptFromBlockDownloadOfNonViewableFiles { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool FileSavePostProcessingEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasExternalDataSource { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCatalog { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsDefaultDocumentLibrary { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsEnterpriseGalleryLibrary { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSiteAssetsLibrary { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsSystemList { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastItemDeletedDate { get; set; }

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
        public int ListExperienceOptions { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ListFormCustomized { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListItemEntityTypeFullName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ListSchemaVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MajorVersionLimit { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MajorWithMinorVersionsLimit { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool MultipleDataList { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int PageRenderType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ParentWebUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ParserDisabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ServerTemplateCanCreateFolders { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowHiddenFieldsInModernForm { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TemplateTypeId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ICreatablesInfo CreatablesInfo { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IView DefaultView { get; }

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
        public IFormCollection Forms { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWeb ParentWeb { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISubscriptionCollection Subscriptions { get; }

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
        public IWorkflowAssociationCollection WorkflowAssociations { get; }

        #endregion

    }
}
