using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a List object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(List))]
    public interface IList : IDataModel<IList>, IDataModelGet<IList>, IDataModelLoad<IList>, IDataModelUpdate, IDataModelDelete, IDataModelSupportingGetChanges, ISecurableObject, IQueryableDataModel
    {
        #region Properties

        /// <summary>
        /// The Unique ID of the List object
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets or sets the list title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the list
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the identifier of the document template for the new list.
        /// SP REST property name: DocumentTemplateUrl.
        /// </summary>
        public string DocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list is displayed on the Quick Launch of the site.
        /// </summary>
        public bool OnQuickLaunch { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the list server template of the new list.
        /// https://msdn.microsoft.com/en-us/library/office/microsoft.sharepoint.client.listtemplatetype.aspx
        /// SP REST property name: BaseTemplate
        /// </summary>
        public ListTemplateType TemplateType { get; }

        /// <summary>
        /// Gets or sets whether verisioning is enabled on the list
        /// </summary>
        public bool EnableVersioning { get; set; }

        /// <summary>
        /// Gets or sets whether minor verisioning is enabled on the list
        /// </summary>
        public bool EnableMinorVersions { get; set; }

        /// <summary>
        /// Gets or sets the DraftVersionVisibility for the list
        /// </summary>
        public DraftVisibilityType DraftVersionVisibility { get; set; }

        /// <summary>
        /// Gets or sets whether moderation/content approval is enabled on the list
        /// </summary>
        public bool EnableModeration { get; set; }

        /// <summary>
        /// Gets the root folder of the list.
        /// </summary>
        public IFolder RootFolder { get; }

        /// <summary>
        /// Gets or sets the MinorVersionLimit  for versioning, just in case it is enabled on the list
        /// SP REST property name: MajorWithMinorVersionsLimit.
        /// </summary>
        public int MinorVersionLimit { get; set; }

        /// <summary>
        /// Gets or sets the MinorVersionLimit  for verisioning, just in case it is enabled on the list
        /// SP REST property name: MajorVersionLimit.
        /// </summary>
        public int MaxVersionLimit { get; set; }

        /// <summary>
        /// Gets or sets whether content types are enabled
        /// </summary>
        public bool ContentTypesEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether to hide the list
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets whether to force checkout of documents in the library
        /// </summary>
        public bool ForceCheckout { get; set; }

        /// <summary>
        /// Gets or sets whether attachments are enabled. Defaults to true.
        /// </summary>
        public bool EnableAttachments { get; set; }

        /// <summary>
        /// Gets or sets whether folder is enabled. Defaults to true.
        /// </summary>
        public bool EnableFolderCreation { get; set; }

        /// <summary>
        /// Gets or sets the Guid for TemplateFeature
        /// </summary>
        public Guid TemplateFeatureId { get; }

        /// <summary>
        /// Defines a list of default values for the Fields of the List Instance
        /// </summary>
        public Dictionary<string, string> FieldDefaults { get; }

        /// <summary>
        /// Defines if the current list or library has to be included in crawling, optional attribute.
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// Defines the current list UI/UX experience (valid for SPO only).
        /// SP REST property name: ListExperienceOptions.
        /// </summary>
        public ListExperience ListExperience { get; set; }

        /// <summary>
        /// Defines a value that specifies the location of the default display form for the list.
        /// </summary>
        public string DefaultDisplayFormUrl { get; set; }

        /// <summary>
        /// Defines a value that specifies the URL of the edit form to use for list items in the list.
        /// </summary>
        public string DefaultEditFormUrl { get; set; }

        /// <summary>
        /// Defines a value that specifies the location of the default new form for the list.
        /// </summary>
        public string DefaultNewFormUrl { get; set; }

        /// <summary>
        /// The url to the default view of this list.
        /// </summary>
        public string DefaultViewUrl { get; }

        /// <summary>
        /// Gets or sets whether the item is opened by default using the browser.
        /// </summary>
        public bool DefaultItemOpenInBrowser { get; set; }

        /// <summary>
        /// Defines a value that specifies the reading order of the list.
        /// </summary>
        public ListReadingDirection Direction { get; set; }

        /// <summary>
        /// Defines a value that specifies the URI for the icon of the list, optional attribute.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Defines if IRM Expire property, optional attribute.
        /// </summary>
        public bool IrmExpire { get; set; }

        /// <summary>
        /// Defines the IRM Reject property, optional attribute.
        /// </summary>
        public bool IrmReject { get; set; }

        /// <summary>
        /// Defines if IRM is enabled for this list.
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// Defines a value that specifies a flag that a client application can use to determine whether to display the list, optional attribute.
        /// </summary>
        public bool IsApplicationList { get; set; }

        /// <summary>
        /// Gets a bool value that indicates whether the list is a gallery, such list templates, Web Parts, or Master Pages.
        /// </summary>
        public bool IsCatalog { get; }

        /// <summary>
        /// Is this library the default document library of this site
        /// </summary>
        public bool IsDefaultDocumentLibrary { get; }

        /// <summary>
        /// Gets a bool value that indicates whether the document library is a private list with restricted permissions, such as for Solutions
        /// </summary>
        public bool IsPrivate { get; }

        /// <summary>
        /// Is this library the site's site asset library
        /// </summary>
        public bool IsSiteAssetsLibrary { get; }

        /// <summary>
        /// Specifies whether the list is system list that does not contain end user data and created by system account.
        /// </summary>
        public bool IsSystemList { get; }

        /// <summary>
        /// Defines the Read Security property, optional attribute.
        /// </summary>
        public int ReadSecurity { get; set; }

        /// <summary>
        /// Defines the Write Security property, optional attribute.
        /// </summary>
        public int WriteSecurity { get; set; }

        /// <summary>
        /// Defines a value that specifies the data validation criteria for a list item, optional attribute.
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Defines a value that specifies the error message returned when data validation fails for a list item, optional attribute.
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// The entity needed when updating list items
        /// </summary>
        string ListItemEntityTypeFullName { get; }

        /// <summary>
        /// Number of items in the library
        /// </summary>
        public int ItemCount { get; }

        /// <summary>
        /// Specifies the date and time that the list was created.
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// Specifies when an item in the list was last deleted. If no item has yet been deleted from the list
        /// the list creation time is returned.
        /// </summary>
        public DateTime LastItemDeletedDate { get; }

        /// <summary>
        /// Specifies when an item, field, or property of the list was last modified. If no item has been created in the list,
        /// the list creation time is returned.
        /// </summary>
        public DateTime LastItemModifiedDate { get; }

        /// <summary>
        /// Specifies when an item of the list was last modified by a non-system update. A non-system update is a change to a list item that is visible to users.
        /// If no item has been created in the list, the list creation time is returned.
        /// </summary>
        public DateTime LastItemUserModifiedDate { get; }

        /// <summary>
        /// Collection of list items in the current List object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IListItemCollection Items { get; }

        /// <summary>
        /// Collection of content types for this list
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        /// <summary>
        /// Collection of fields for this list
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// Load the IRM settings of this list
        /// </summary>
        public IInformationRightsManagementSettings InformationRightsManagementSettings { get; }

        /// <summary>
        /// Get a list of the views
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IViewCollection Views { get; }

        /// <summary>
        /// Collection of list webhooks
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IListSubscriptionCollection Webhooks { get; }

        /// <summary>
        /// Event Receivers defined in this list
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IEventReceiverDefinitionCollection EventReceivers { get; }

        /// <summary>
        /// Gets a value that specifies the collection of user custom actions for this list
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IUserCustomActionCollection UserCustomActions { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #endregion

        #region Methods
        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <returns>Id of the recycle bin item</returns>
        public Guid Recycle();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <returns>Id of the recycle bin item</returns>
        public Task<Guid> RecycleAsync();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <returns>Id of the recycle bin item</returns>
        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <returns>Id of the recycle bin item</returns>
        public Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns>Id of the recycle bin item</returns>
        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch(Batch batch);

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns>Id of the recycle bin item</returns>
        public Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync(Batch batch);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryAsync(string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQuery(string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryAsync(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQuery(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(Batch batch, string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(Batch batch, string query, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(Batch batch, CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(Batch batch, CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task<Dictionary<string, object>> LoadListDataAsStreamAsync(RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Dictionary<string, object> LoadListDataAsStream(RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task<IBatchSingleResult<Dictionary<string, object>>> LoadListDataAsStreamBatchAsync(RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public IBatchSingleResult<Dictionary<string, object>> LoadListDataAsStreamBatch(RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task<IBatchSingleResult<Dictionary<string, object>>> LoadListDataAsStreamBatchAsync(Batch batch, RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public IBatchSingleResult<Dictionary<string, object>> LoadListDataAsStreamBatch(Batch batch, RenderListDataOptions renderOptions);

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public IComplianceTag GetComplianceTag();

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public Task<IComplianceTag> GetComplianceTagAsync();

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        public Task<IBatchSingleResult<IComplianceTag>> GetComplianceTagBatchAsync(Batch batch);

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        public IBatchSingleResult<IComplianceTag> GetComplianceTagBatch(Batch batch);

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public Task<IBatchSingleResult<IComplianceTag>> GetComplianceTagBatchAsync();

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public IBatchSingleResult<IComplianceTag> GetComplianceTagBatch();

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public void SetComplianceTag(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public Task SetComplianceTagAsync(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public void SetComplianceTagBatch(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public Task SetComplianceTagBatchAsync(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public void SetComplianceTagBatch(Batch batch, string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Sets a compliance tag / retention label for this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="complianceTagValue">Compliance tag</param>
        /// <param name="blockDelete">Prevent deletion of the list (Hold)</param>
        /// <param name="blockEdit">Prevent editing of the list (Record)</param>
        /// <param name="syncToItems">If true the compliance tag is synced to the list items in this list</param>
        public Task SetComplianceTagBatchAsync(Batch batch, string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems);

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public IListItem AddListFolder(string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public Task<IListItem> AddListFolderAsync(string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public IListItem AddListFolderBatch(string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public Task<IListItem> AddListFolderBatchAsync(string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public IListItem AddListFolderBatch(Batch batch, string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Adds a folder
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="path"></param>
        /// <param name="parentFolder"></param>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public Task<IListItem> AddListFolderBatchAsync(Batch batch, string path, string parentFolder = null, string contentTypeId = "0x0120");

        /// <summary>
        /// Classifies and extracts all unprocessed files in the list
        /// </summary>
        /// <param name="force">Also classify and extract files that were processed before</param>
        /// <param name="pageSize">Page size used when loading the files in this library</param>
        /// <returns>Information about the created classify and extract requests</returns>
        Task<List<ISyntexClassifyAndExtractResult>> ClassifyAndExtractAsync(bool force = false, int pageSize = 500);

        /// <summary>
        /// Classifies and extracts all unprocessed files in the list
        /// </summary>
        /// <param name="force">Also classify and extract files that were processed before</param>
        /// <param name="pageSize">Page size used when loading the files in this library</param>
        /// <returns>Information about the created classify and extract requests</returns>
        List<ISyntexClassifyAndExtractResult> ClassifyAndExtract(bool force = false, int pageSize = 500);

        /// <summary>
        /// Classifies and extracts all unprocessed files in the list via the Syntex off-peak queue
        /// </summary>
        /// <returns>Information about the created classify and extract requests</returns>
        Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractOffPeakAsync();

        /// <summary>
        /// Classifies and extracts all unprocessed files in the list via the Syntex off-peak queue
        /// </summary>
        /// <returns>Information about the created classify and extract requests</returns>
        ISyntexClassifyAndExtractResult ClassifyAndExtractOffPeak();

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <returns>List of connected flow instances</returns>
        Task<List<IFlowInstance>> GetFlowInstancesAsync();

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <returns>List of connected flow instances</returns>
        List<IFlowInstance> GetFlowInstances();

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <returns>List of connected flow instances</returns>
        Task<IEnumerableBatchResult<IFlowInstance>> GetFlowInstancesBatchAsync(Batch batch);

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <returns>List of connected flow instances</returns>
        IEnumerableBatchResult<IFlowInstance> GetFlowInstancesBatch(Batch batch);

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <returns>List of connected flow instances</returns>
        Task<IEnumerableBatchResult<IFlowInstance>> GetFlowInstancesBatchAsync();

        /// <summary>
        /// Returns a list of flow instances connected to this list
        /// </summary>
        /// <returns>List of connected flow instances</returns>
        IEnumerableBatchResult<IFlowInstance> GetFlowInstancesBatch();

        /// <summary>
        /// Reorders the content types on the list. This controls the order of content types in the "New" menu and "List Settings" page
        /// </summary>
        /// <param name="contentTypeIdList">Ordered list of content type ids to set</param>
        /// <returns></returns>
        Task ReorderContentTypesAsync(List<string> contentTypeIdList);

        /// <summary>
        /// Reorders the content types on the list. This controls the order of content types in the "New" menu and "List Settings" page
        /// </summary>
        /// <param name="contentTypeIdList">Ordered list of content type ids to set</param>
        /// <returns></returns>
        void ReorderContentTypes(List<string> contentTypeIdList);

        /// <summary>
        /// Returns the current list or content types in the current order
        /// </summary>
        /// <returns>Ordered list of content type id's, returns null when the list is not enabled to use content types</returns>
        Task<List<string>> GetContentTypeOrderAsync();

        /// <summary>
        /// Returns the current list or content types in the current order
        /// </summary>
        /// <returns>Ordered list of content type id's, returns null when the list is not enabled to use content types</returns>
        List<string> GetContentTypeOrder();

        /// <summary>
        /// Find files in the list, can be slow as it iterates over all the files in the list. If performance
        /// is key, then try using a search based solution
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of found files as type <see cref="IFile"/></returns>
        List<IFile> FindFiles(string match);

        /// <summary>
        /// Find files in the list, can be slow as it iterates over all the files in the list. If performance
        /// is key, then try using a search based solution
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of found files as type <see cref="IFile"/></returns>
        Task<List<IFile>> FindFilesAsync(string match);

        /// <summary>
        /// Gets the user effective permissions of a user for a list
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        IBasePermissions GetUserEffectivePermissions(string userPrincipalName);

        /// <summary>
        /// Gets the user effective permissions of a user for a list
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        Task<IBasePermissions> GetUserEffectivePermissionsAsync(string userPrincipalName);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a list
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        bool CheckIfUserHasPermissions(string userPrincipalName, PermissionKind permissionKind);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a list
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        Task<bool> CheckIfUserHasPermissionsAsync(string userPrincipalName, PermissionKind permissionKind);

        /// <summary>
        /// Gets a list of default column values set (at folder level) for this library
        /// </summary>
        /// <returns>List of default column values</returns>
        Task<List<DefaultColumnValueOptions>> GetDefaultColumnValuesAsync();

        /// <summary>
        /// Gets a list of default column values set (at folder level) for this library 
        /// </summary>
        /// <returns>List of default column values</returns>
        List<DefaultColumnValueOptions> GetDefaultColumnValues();

        /// <summary>
        /// Clears the default column values set (at folder level) for this library
        /// </summary>
        /// <returns></returns>
        Task ClearDefaultColumnValuesAsync();

        /// <summary>
        /// Clears the default column values set (at folder level) for this library
        /// </summary>
        /// <returns></returns>
        void ClearDefaultColumnValues();

        /// <summary>
        /// Sets the default column value settings (at folder level) for this library
        /// </summary>
        /// <param name="defaultColumnValues">List with default column values to set</param>
        /// <returns></returns>
        Task SetDefaultColumnValuesAsync(List<DefaultColumnValueOptions> defaultColumnValues);

        /// <summary>
        /// Sets the default column value settings (at folder level) for this library
        /// </summary>
        /// <param name="defaultColumnValues">List with default column values to set</param>
        /// <returns></returns>
        void SetDefaultColumnValues(List<DefaultColumnValueOptions> defaultColumnValues);

        /// <summary>
        /// Reindexes this list
        /// </summary>
        /// <returns></returns>
        Task ReIndexAsync();

        /// <summary>
        /// Reindexes this list
        /// </summary>
        /// <returns></returns>
        void ReIndex();

        /// <summary>
        /// Enable audience targeting for a list
        /// </summary>
        /// <returns></returns>
        Task EnableAudienceTargetingAsync();

        /// <summary>
        /// Enable audience targeting for a list
        /// </summary>
        /// <returns></returns>
        void EnableAudienceTargeting();
        #endregion
    }
}
