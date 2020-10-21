using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of term sets
    /// </summary>
    public interface ITermSetCollection : IQueryable<ITermSet>, IDataModelCollection<ITermSet>, ISupportPaging<ITermSet>
    {

        #region Add methods

        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public Task<ITermSet> AddAsync(string name, string description = null);

        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public ITermSet Add(string name, string description = null);


        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public Task<ITermSet> AddBatchAsync(Batch batch, string name, string description = null);

        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public ITermSet AddBatch(Batch batch, string name, string description = null);

        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public Task<ITermSet> AddBatchAsync(string name, string description = null);

        /// <summary>
        /// Adds a new term set
        /// </summary>
        /// <param name="name">Name of the term set</param>
        /// <param name="description">Optional description of the term set</param>
        /// <returns>Newly added term set</returns>
        public ITermSet AddBatch(string name, string description = null);

        #endregion

        #region GetById for TermSets implementation

        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term set instance, if any</returns>
        public ITermSet GetById(string id, params Expression<Func<ITermSet, object>>[] selectors);

        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term set instance, if any</returns>
        public Task<ITermSet> GetByIdAsync(string id, params Expression<Func<ITermSet, object>>[] selectors);

        #endregion


    }
}
