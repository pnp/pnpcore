using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every collection of Domain Model objects which supports explicit load
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model objects</typeparam>
    public interface IDataModelCollectionLoad<TModel>
    {
        /// <summary>
        /// Loads the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task LoadAsync(params Expression<Func<TModel, object>>[] expressions);


        /// <summary>
        /// Batches the load of the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">The batch to use</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchResult> LoadBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions);
    }
}
