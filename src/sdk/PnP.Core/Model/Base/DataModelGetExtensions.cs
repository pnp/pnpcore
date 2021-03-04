using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extension methods for <see cref="IDataModelProcess"/>
    /// </summary>
    internal static class DataModelGetExtensions
    {
       /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelGet"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal static Task<IBatchSingleResult<TModel>> GetBatchAsync<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
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
        internal static IBatchSingleResult<TModel> GetBatch<TModel>(this IDataModelGet<TModel> dataModelGet, Batch batch, params Expression<Func<TModel, object>>[] expressions)
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
        internal static IBatchSingleResult<TModel> GetBatch<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
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
        internal static TModel Get<TModel>(this IDataModelGet<TModel> dataModelGet, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelGet == null)
            {
                throw new ArgumentNullException(nameof(dataModelGet));
            }

            return dataModelGet.GetAsync(expressions).GetAwaiter().GetResult();
        }

    }
}
