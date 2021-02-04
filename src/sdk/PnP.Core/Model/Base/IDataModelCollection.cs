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
        /// Enables using the .LoadProperties lambda expression syntax on a collection
        /// </summary>
        /// <param name="expressions">Expression</param>
        /// <returns>Null...return value is not needed</returns>
        public IQueryable<TModel> LoadProperties(params Expression<Func<TModel, object>>[] expressions);

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

        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetBatch(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public IEnumerable<TModel> Get(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetBatchAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetBatch(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetBatchAsync(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetBatch(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for the first model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task<TModel> GetFirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public TModel GetFirstOrDefault(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetFirstOrDefaultBatchAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetFirstOrDefaultBatch(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public Task GetFirstOrDefaultBatchAsync(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Queries for model instances using a given optional LINQ filter expression and field load expressions
        /// </summary>
        /// <param name="batch">Batch being used</param>
        /// <param name="predicate">LINQ filter expression</param>
        /// <param name="expressions">LINQ field load expressions</param>
        /// <returns>List of found model instances</returns>
        public void GetFirstOrDefaultBatch(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions);

    }
}
