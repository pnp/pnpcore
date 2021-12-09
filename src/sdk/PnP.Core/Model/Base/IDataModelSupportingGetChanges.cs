using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the interface for Domain Model objects that can obtain a change log.
    /// </summary>
    public interface IDataModelSupportingGetChanges
    {
        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public Task<IList<IChange>> GetChangesAsync(ChangeQueryOptions query);

        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public IList<IChange> GetChanges(ChangeQueryOptions query);

        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(Batch batch, ChangeQueryOptions query);

        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="batch">Batch to add this request to </param>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public IEnumerableBatchResult<IChange> GetChangesBatch(Batch batch, ChangeQueryOptions query);

        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(ChangeQueryOptions query);

        /// <summary>
        /// Gets the list of changes.
        /// </summary>
        /// <remarks>
        /// This does not load the parent object or any properties. It returns a completely separate object.
        /// </remarks>
        /// <param name="query">The query.</param>
        /// <returns>The list of changes.</returns>
        public IEnumerableBatchResult<IChange> GetChangesBatch(ChangeQueryOptions query);
    }
}
