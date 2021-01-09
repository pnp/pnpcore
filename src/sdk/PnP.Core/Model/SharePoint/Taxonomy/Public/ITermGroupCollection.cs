using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of term groups
    /// </summary>
    [ConcreteType(typeof(TermGroupCollection))]
    public interface ITermGroupCollection : IQueryable<ITermGroup>, IDataModelCollection<ITermGroup>, ISupportPaging<ITermGroup>, IDataModelCollectionDeleteByStringId
    {
        #region Add methods

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public Task<ITermGroup> AddAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public ITermGroup Add(string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public Task<ITermGroup> AddBatchAsync(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public ITermGroup AddBatch(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public Task<ITermGroup> AddBatchAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        /// <summary>
        /// Adds a new term group
        /// </summary>
        /// <param name="name">Display name of the group</param>
        /// <param name="description">Optional description of the group</param>
        /// <param name="scope">Optional scope of the group</param>
        /// <returns>Newly added group</returns>
        public ITermGroup AddBatch(string name, string description = null, TermGroupScope scope = TermGroupScope.Global);

        #endregion

        #region GetById methods

        /// <summary>
        /// Method to select a term group by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public ITermGroup GetById(string id, params Expression<Func<ITermGroup, object>>[] selectors);

        /// <summary>
        /// Method to select a term group by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public Task<ITermGroup> GetByIdAsync(string id, params Expression<Func<ITermGroup, object>>[] selectors);

        #endregion

        #region GetByName methods

        /// <summary>
        /// Method to select a term group by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public ITermGroup GetByName(string name, params Expression<Func<ITermGroup, object>>[] selectors);

        /// <summary>
        /// Method to select a term group by name
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public Task<ITermGroup> GetByNameAsync(string name, params Expression<Func<ITermGroup, object>>[] selectors);

        #endregion

    }
}
