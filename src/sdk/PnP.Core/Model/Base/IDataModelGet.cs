using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the read interface for Domain Model objects that can be read.
    /// </summary>
    public interface IDataModelGet<TModel>
    {
        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<TModel> GetAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
#pragma warning disable CA1716 // Identifiers should not match keywords
        TModel Get(params Expression<Func<TModel, object>>[] expressions);
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchSingleResult<TModel>> GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        IBatchSingleResult<TModel> GetBatch(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchSingleResult<TModel>> GetBatchAsync(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        IBatchSingleResult<TModel> GetBatch(params Expression<Func<TModel, object>>[] expressions);
    }
}