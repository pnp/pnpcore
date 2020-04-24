using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the basic untyped read interface for Domain Model objects that can be read.
    /// </summary>
    public interface IDataModelGet
    {
        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "I prefer Get over Read :-)")]
        object Get(Batch batch, params Expression<Func<object, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "I prefer Get over Read :-)")]
        object Get(params Expression<Func<object, object>>[] expressions);

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<object> GetAsync(params Expression<Func<object, object>>[] expressions);

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
    public interface IDataModelGet<TModel> : IDataModelGet
    {
        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        TModel Get(Batch batch, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        TModel Get(params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        Task<TModel> GetAsync(params Expression<Func<TModel, object>>[] expressions);

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
