using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the basic untyped read interface for Domain Model objects that can be loaded, only used internally
    /// </summary>
    internal interface IDataModelLoad
    {
        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchResult> LoadBatchAsync(Batch batch, params LambdaExpression[] expressions);

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task LoadAsync(params LambdaExpression[] expressions);
    }

    /// <summary>
    /// Defines the read interface for Domain Model objects that can be loaded.
    /// </summary>
    public interface IDataModelLoad<TModel>
    {
        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchResult> LoadBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task LoadAsync(params Expression<Func<TModel, object>>[] expressions);
    }
}
