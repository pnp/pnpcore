using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Public interface to define a collection of events for a Team
    /// </summary>
    [ConcreteType(typeof(GraphEventCollection))]
    public interface IGraphEventCollection : IQueryable<IGraphEvent>, IAsyncEnumerable<IGraphEvent>, IDataModelCollection<IGraphEvent>, IDataModelCollectionLoad<IGraphEvent>, IDataModelCollectionDeleteByGuidId, ISupportModules<IGraphEventCollection>
    {
        #region Add Methods

        /// <summary>
        /// Adds a new event
        /// </summary>
        /// <param name="options">Event options</param>
        /// <returns>Created graph event</returns>
        public Task<IGraphEvent> AddAsync(EventCreateOptions options);

        /// <summary>
        /// Adds a new event
        /// </summary>
        /// <param name="options">Event options</param>
        /// <returns>Created graph event</returns>
        public IGraphEvent Add(EventCreateOptions options);

        #endregion
    }
}
