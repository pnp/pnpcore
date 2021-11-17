using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of List objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ListCollection))]
    public interface IListCollection : IQueryable<IList>, IAsyncEnumerable<IList>, IDataModelCollection<IList>, IDataModelCollectionLoad<IList>, IDataModelCollectionDeleteByGuidId, ISupportModules<IListCollection>
    {
        #region Add Methods
        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public Task<IList> AddAsync(string title, ListTemplateType templateType);

        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public IList Add(string title, ListTemplateType templateType);

        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public Task<IList> AddBatchAsync(Batch batch, string title, ListTemplateType templateType);

        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public IList AddBatch(Batch batch, string title, ListTemplateType templateType);

        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public Task<IList> AddBatchAsync(string title, ListTemplateType templateType);

        /// <summary>
        /// Adds a new list
        /// </summary>
        /// <param name="title">Title of the list</param>
        /// <param name="templateType">Template type</param>
        /// <returns>Newly added list</returns>
        public IList AddBatch(string title, ListTemplateType templateType);
        #endregion

        #region GetByTitle methods
        /// <summary>
        /// Select a list by title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public IList GetByTitle(string title, params Expression<Func<IList, object>>[] selectors);

        /// <summary>
        /// Select a list by title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] selectors);
        #endregion

        #region GetById methods
        /// <summary>
        /// Method to select a list by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public IList GetById(Guid id, params Expression<Func<IList, object>>[] selectors);

        /// <summary>
        /// Method to select a list by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public Task<IList> GetByIdAsync(Guid id, params Expression<Func<IList, object>>[] selectors);
        #endregion

        #region GetByServerRelativeUrl implementation
        /// <summary>
        /// Method to select a list by server relative url
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative url of the list to return</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public IList GetByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors);

        /// <summary>
        /// Method to select a list by server relative url
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative url of the list to return</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public Task<IList> GetByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors);
        #endregion

        #region EnsureSiteAssetsLibrary
        /// <summary>
        /// Ensures there's an Asset Library in the site, if not present it will be created
        /// </summary>
        /// <returns>The asset library</returns>
        Task<IList> EnsureSiteAssetsLibraryAsync();

        /// <summary>
        /// Ensures there's an Asset Library in the site, if not present it will be created
        /// </summary>
        /// <returns>The asset library</returns>
        IList EnsureSiteAssetsLibrary();
        #endregion

    }
}
