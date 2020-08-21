using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of term groups
    /// </summary>
    public interface ITermGroupCollection : IQueryable<ITermGroup>, IDataModelCollection<ITermGroup>, ISupportPaging<ITermGroup>
    {
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
    }
}
