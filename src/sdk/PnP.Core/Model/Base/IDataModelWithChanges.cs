using PnP.Core.Model.SharePoint;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the interface for Domain Model objects that can obtain a change log.
    /// </summary>
    public interface IDataModelWithChanges
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
    }
}
