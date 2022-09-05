using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint groups
    /// </summary>
    [ConcreteType(typeof(SharePointGroupCollection))]
    public interface ISharePointGroupCollection : IQueryable<ISharePointGroup>, IAsyncEnumerable<ISharePointGroup>, IDataModelCollection<ISharePointGroup>, IDataModelCollectionLoad<ISharePointGroup>, ISupportModules<ISharePointGroupCollection>
    {
        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public ISharePointGroup Add(string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public ISharePointGroup AddBatch(string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public ISharePointGroup AddBatch(Batch batch, string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public Task<ISharePointGroup> AddAsync(string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public Task<ISharePointGroup> AddBatchAsync(string name);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="name">Name of the new group to add</param>
        /// <returns></returns>
        public Task<ISharePointGroup> AddBatchAsync(Batch batch, string name);

    }
}
