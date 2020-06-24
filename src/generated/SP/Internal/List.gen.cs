using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a List object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class List : BaseDataModel<IList>, IList
    {

        #region Existing properties

        [SharePointProperty("BaseTemplate")]
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

        [SharePointProperty("Items", Expandable = true)]
        [GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items
        {
            get
            {
                if (!HasValue(nameof(Items)))
                {
                    var collection = new ListItemCollection(this.PnPContext, this, nameof(Items));
                    SetValue(collection);
                }
                return GetValue<IListItemCollection>();
            }
        }

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

        public bool DefaultItemOpenUseListSetting { get => GetValue<bool>(); set => SetValue(value); }

        public string DefaultViewUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool DisableGridEditing { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableAssignToEmail { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableRequestSignOff { get => GetValue<bool>(); set => SetValue(value); }

        public string EntityTypeName { get => GetValue<string>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExemptFromBlockDownloadOfNonViewableFiles { get => GetValue<bool>(); set => SetValue(value); }

        public bool FileSavePostProcessingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasExternalDataSource { get => GetValue<bool>(); set => SetValue(value); }

        public string Icon { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsCatalog { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsEnterpriseGalleryLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsPrivate { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAssetsLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSystemList { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemDeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string ListItemEntityTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public bool MultipleDataList { get => GetValue<bool>(); set => SetValue(value); }

        public int PageRenderType { get => GetValue<int>(); set => SetValue(value); }

        public string ParentWebUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ParserDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public bool ServerTemplateCanCreateFolders { get => GetValue<bool>(); set => SetValue(value); }

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


        public ICreatablesInfo CreatablesInfo
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new CreatablesInfo
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ICreatablesInfo>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IView DefaultView
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new View
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IView>();
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

        [SharePointProperty("Forms", Expandable = true)]
        public IFormCollection Forms
        {
            get
            {
                if (!HasValue(nameof(Forms)))
                {
                    var collection = new FormCollection(this.PnPContext, this, nameof(Forms));
                    SetValue(collection);
                }
                return GetValue<IFormCollection>();
            }
        }

        public IInformationRightsManagementSettings InformationRightsManagementSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new InformationRightsManagementSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IInformationRightsManagementSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public IWeb ParentWeb
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


        [SharePointProperty("Subscriptions", Expandable = true)]
        public ISubscriptionCollection Subscriptions
        {
            get
            {
                if (!HasValue(nameof(Subscriptions)))
                {
                    var collection = new SubscriptionCollection(this.PnPContext, this, nameof(Subscriptions));
                    SetValue(collection);
                }
                return GetValue<ISubscriptionCollection>();
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

        [SharePointProperty("Views", Expandable = true)]
        public IViewCollection Views
        {
            get
            {
                if (!HasValue(nameof(Views)))
                {
                    var collection = new ViewCollection(this.PnPContext, this, nameof(Views));
                    SetValue(collection);
                }
                return GetValue<IViewCollection>();
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

        #endregion

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }


    }
}
