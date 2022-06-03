using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ContentType objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ContentTypeCollection))]
    public interface IContentTypeCollection : IQueryable<IContentType>, IAsyncEnumerable<IContentType>, IDataModelCollection<IContentType>, IDataModelCollectionLoad<IContentType>, ISupportModules<IContentTypeCollection>
    {
        #region Extension Methods

        #region Add

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddBatchAsync(string id, string name, string description = null, string group = null);

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType AddBatch(string id, string name, string description = null, string group = null);

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="batch"><see cref="Batch"/> to use</param>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddBatchAsync(Batch batch, string id, string name, string description = null, string group = null);

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="batch"><see cref="Batch"/> to use</param>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType AddBatch(Batch batch, string id, string name, string description = null, string group = null);

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddAsync(string id, string name, string description = null, string group = null);

        /// <summary>
        /// Add a content type
        /// Check the documentation for a well formed Content Type Id:
        /// https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <param name="name">Name of the content type</param>
        /// <param name="description">Description of the content type</param>
        /// <param name="group">Group of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType Add(string id, string name, string description = null, string group = null);

        #endregion

        #region AddAvailableContentType
        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddAvailableContentTypeBatchAsync(string id);

        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType AddAvailableContentTypeBatch(string id);

        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="batch"><see cref="Batch"/> to use</param>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddAvailableContentTypeBatchAsync(Batch batch, string id);

        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="batch"><see cref="Batch"/> to use</param>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType AddAvailableContentTypeBatch(Batch batch, string id);

        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        Task<IContentType> AddAvailableContentTypeAsync(string id);

        /// <summary>
        /// Add an existing content type
        /// </summary>
        /// <param name="id">Id of the content type</param>
        /// <returns>The newly added content type</returns>
        IContentType AddAvailableContentType(string id);

        #endregion

        #region Document Sets

        /// <summary>
        /// Creates a document set
        /// </summary>
        /// <param name="id">Id of the document set</param>
        /// <param name="name">Name of the document set</param>
        /// <param name="description">Description of the document set</param>
        /// <param name="group">Group of the document set</param>
        /// <param name="options">Options for creating the document set</param>
        /// <returns>The newly added document set</returns>
        Task<IDocumentSet> AddDocumentSetAsync(string id, string name, string description = null, string group = null, DocumentSetOptions options = null);

        /// <summary>
        /// Creates a document set
        /// </summary>
        /// <param name="id">Id of the document set</param>
        /// <param name="name">Name of the document set</param>
        /// <param name="description">Description of the document set</param>
        /// <param name="group">Group of the document set</param>
        /// <param name="options">Options for creating the document set</param>
        /// <returns>The newly added document set</returns>
        IDocumentSet AddDocumentSet(string id, string name, string description = null, string group = null, DocumentSetOptions options = null);

        #endregion

        #endregion
    }
}
