using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of terms
    /// </summary>
    public interface ITermCollection : IDataModelCollection<ITerm>, ISupportPaging<ITerm>
    {
        /// <summary>
        /// Adds a new term 
        /// </summary>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public Task<ITerm> AddAsync(string name, string description = null);

        /// <summary>
        /// Adds a new term
        /// </summary>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public ITerm Add(string name, string description = null);


        /// <summary>
        /// Adds a new term
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public Task<ITerm> AddBatchAsync(Batch batch, string name, string description = null);

        /// <summary>
        /// Adds a new term
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public ITerm AddBatch(Batch batch, string name, string description = null);

        /// <summary>
        /// Adds a new term 
        /// </summary>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public Task<ITerm> AddBatchAsync(string name, string description = null);

        /// <summary>
        /// Adds a new term 
        /// </summary>
        /// <param name="name">Name of the term</param>
        /// <param name="description">Optional description of the term</param>
        /// <returns>Newly added term</returns>
        public ITerm AddBatch(string name, string description = null);

        /// <summary>
        /// Loads a term by id
        /// </summary>
        /// <param name="id">Id of the term to load</param>
        /// <param name="expressions">Properties to load</param>
        /// <returns>Found term if any, null otherwise</returns>
        public Task<ITerm> GetByIdAsync(string id);

    }
}
