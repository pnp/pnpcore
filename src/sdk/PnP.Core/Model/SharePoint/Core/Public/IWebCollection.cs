using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Web objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(WebCollection))]
    public interface IWebCollection : IQueryable<IWeb>, IAsyncEnumerable<IWeb>, IDataModelCollection<IWeb>, IDataModelCollectionLoad<IWeb>
    {
        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public Task<IWeb> AddAsync(WebOptions webOptions);

        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public IWeb Add(WebOptions webOptions);

        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public Task<IWeb> AddBatchAsync(WebOptions webOptions);

        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public IWeb AddBatch(WebOptions webOptions);

        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public Task<IWeb> AddBatchAsync(Batch batch, WebOptions webOptions);

        /// <summary>
        /// Adds a new web to the current web
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="webOptions">Options used when creating the new web</param>
        /// <returns>The newly created web</returns>
        public IWeb AddBatch(Batch batch, WebOptions webOptions);
    }
}
