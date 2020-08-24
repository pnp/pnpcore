using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of terms
    /// </summary>
    public interface ITermCollection : IQueryable<ITerm>, IDataModelCollection<ITerm>, ISupportPaging<ITerm>
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
    }
}
