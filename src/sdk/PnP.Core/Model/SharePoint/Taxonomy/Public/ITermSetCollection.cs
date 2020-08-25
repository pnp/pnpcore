using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of term sets
    /// </summary>
    public interface ITermSetCollection : IQueryable<ITermSet>, IDataModelCollection<ITermSet>, ISupportPaging<ITermSet>
    {
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
    }
}
