using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extension methods for <see cref="IDataModelGet"/>
    /// </summary>
    public static class DataModelGetExtensions
    {
        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static IBatchSingleResult GetBatch(this IDataModelGet dataModelGet, Batch batch, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static IBatchSingleResult GetBatch(this IDataModelGet dataModelGet, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetBatchAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static object Get(this IDataModelGet dataModelGet, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static object Get(this IDataModelGet dataModelGet, ApiResponse apiResponse, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetAsync(apiResponse, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static Task<IBatchSingleResult> GetBatchAsync(this IDataModelGet dataModelGet, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetBatchAsync(null, expressions);
        }

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static Task<object> GetAsync(this IDataModelGet dataModelGet, params Expression<Func<object, object>>[] expressions)
        {
            return dataModelGet.GetAsync(default, expressions);
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static Task<IBatchSingleResult<TModel>> GetBatchAsync<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetBatchAsync(null, expressions);
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchSingleResult<TModel> GetBatch<TModel>(this IDataModelGet<TModel> dataModelGet, Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchSingleResult<TModel> GetBatch<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetBatchAsync(expressions).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static async Task<TModel> GetAsync<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return await dataModelGet.GetAsync(default, expressions).ConfigureAwait(true);
        }

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static TModel Get<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static TModel Get<TModel>(this IDataModelGet<TModel> dataModelGet, ApiResponse apiResponse, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetAsync(apiResponse, expressions).GetAwaiter().GetResult();
        }
    }
}
