using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every collection of Domain Model objects
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model objects</typeparam>
    public interface IDataModelCollection<TModel> : IEnumerable<TModel>, IDataModelParent, IDataModelWithContext, IRequestableCollection
    {
        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task<IEnumerable<TModel>> GetAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public IEnumerable<TModel> Get(params Expression<Func<TModel, object>>[] expressions);
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetBatchAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetBatch(params Expression<Func<TModel, object>>[] expressions);


    }
}
