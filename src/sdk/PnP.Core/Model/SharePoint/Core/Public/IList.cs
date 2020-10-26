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
    public interface IList : IDataModel<IList>, IDataModelGet<IList>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
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
        /// Get a list of the views
        /// </summary>
        public IViewCollection Views { get; }

        /// <summary>
        /// Moves this list into the site collection recycle bin, returns the recyle bin item id
        /// </summary>
        public Task<Guid> RecycleAsync();

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryAsync(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQuery(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryAsync(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQuery(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryBatchAsync(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQueryBatch(string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryBatchAsync(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQueryBatch(CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryBatchAsync(Batch batch, string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQueryBatch(Batch batch, string query);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task GetItemsByCamlQueryBatchAsync(Batch batch, CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query
        /// </summary>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="queryOptions"><see cref="CamlQueryOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public void GetItemsByCamlQueryBatch(Batch batch, CamlQueryOptions queryOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Task<Dictionary<string, object>> GetListDataAsStreamAsync(RenderListDataOptions renderOptions);

        /// <summary>
        /// Loads list items based up on a CAML query and the RenderListDataAsStream API
        /// </summary>
        /// <param name="renderOptions"><see cref="RenderListDataOptions"/> defining the query to execute</param>
        /// <returns></returns>
        public Dictionary<string, object> GetListDataAsStream(RenderListDataOptions renderOptions);


    }
}
