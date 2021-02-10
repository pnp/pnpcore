using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the basic untyped read interface for Domain Model objects that can be read, only used internally
    /// </summary>
    internal interface IDataModelGet
    {
        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchSingleResult> GetBatchAsync(Batch batch, params Expression<Func<object, object>>[] expressions);

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<object> GetAsync(ApiResponse apiResponse, params Expression<Func<object, object>>[] expressions);

    }

    /// <summary>
    /// Defines the read interface for Domain Model objects that can be read.
    /// </summary>
    public interface IDataModelGet<TModel>
    {
        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<IBatchSingleResult<TModel>> GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<TModel> GetAsync(ApiResponse apiResponse, params Expression<Func<TModel, object>>[] expressions);

    }
}
