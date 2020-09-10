using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of List objects of SharePoint Online
    /// </summary>
    public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>, ISupportPaging<IList>
    {
        #region Extension Methods
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
    }
}
