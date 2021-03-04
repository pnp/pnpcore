using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a List object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(List))]
    public interface IList : IDataModel<IList>, IDataModelGet<IList>, IDataModelLoad<IList>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
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
        /// </summary>
        public string DocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list is displayed on the Quick Launch of the site.
        /// </summary>
        public bool OnQuickLaunch { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the list server template of the new list.
        /// https://msdn.microsoft.com/en-us/library/office/microsoft.sharepoint.client.listtemplatetype.aspx
        /// </summary>
        public ListTemplateType TemplateType { get; }

        /// <summary>
        /// Gets or sets a value that specifies whether the new list is displayed on the Quick Launch of the site.
        /// </summary>
        public string Url { get; }

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
        public int DraftVersionVisibility { get; set; }

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
        /// </summary>
        public int MinorVersionLimit { get; set; }

        /// <summary>
        /// Gets or sets the MinorVersionLimit  for verisioning, just in case it is enabled on the list
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
        /// Collection of list items in the current List object
        /// </summary>
        public IListItemCollection Items { get; }

        /// <summary>
        /// Collection of content types for this list
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        /// <summary>
        /// Collection of fields for this list
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// Load the IRM settings of this list
        /// </summary>
        public IInformationRightsManagementSettings InformationRightsManagementSettings { get; }

        /// <summary>
        /// Collection of role assignments for this list
        /// </summary>
        public IRoleAssignmentCollection RoleAssignments { get; }


        /// <summary>
        /// Get a list of the views
        /// </summary>
        public IViewCollection Views { get; }

        /// <summary>
        /// Returns if the list has unique role assignments
        /// </summary>
        public bool HasUniqueRoleAssignments { get; }

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
        public void RecycleBatch();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <returns>Id of the recycle bin item</returns>
        public Task RecycleBatchAsync();

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns>Id of the recycle bin item</returns>
        public void RecycleBatch(Batch batch);

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns>Id of the recycle bin item</returns>
        public Task RecycleBatchAsync(Batch batch);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryAsync(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQuery(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryAsync(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQuery(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(Batch batch, string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(Batch batch, string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task LoadItemsByCamlQueryBatchAsync(Batch batch, CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void LoadItemsByCamlQueryBatch(Batch batch, CamlQueryOptions queryOptions);

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
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public IComplianceTag GetComplianceTag();

        /// <summary>
        /// Retrieves the compliance tag / retention label for this list
        /// </summary>
        public Task<IComplianceTag> GetComplianceTagAsync();

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
        /// Creates unique role assignments for the list.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for the list.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        public Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Removes the local role assignments so that the list, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        public void ResetRoleInheritance();

        /// <summary>
        /// Removes the local role assignments so that the list, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        public Task ResetRoleInheritanceAsync();

        /// <summary>
        /// Returns the role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        public IRoleDefinitionCollection GetRoleDefinitions(int principalId);

        /// <summary>
        /// Returns the role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        public Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync(int principalId);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool AddRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public Task<bool> AddRoleDefinitionsAsync(int principalId, params string[] names);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool RemoveRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names);

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

    }
}
