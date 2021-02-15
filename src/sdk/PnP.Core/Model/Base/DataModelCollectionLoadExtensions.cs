using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extension methods for <see cref="IDataModelCollectionLoad{TModel}"/>
    /// </summary>
    public static class DataModelCollectionLoadExtensions
    {

        /// <summary>
        /// Batches the load of the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static Task<IBatchResult> LoadBatchAsync<TModel>(this IDataModelCollectionLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(null, expressions);
        }

        /// <summary>
        /// Batches the load of the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchResult LoadBatch<TModel>(this IDataModelCollectionLoad<TModel> dataModelLoad, Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the load of the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchResult LoadBatch<TModel>(this IDataModelCollectionLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(expressions).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Loads the list from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static void Load<TModel>(this IDataModelCollectionLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            dataModelLoad.LoadAsync(expressions).GetAwaiter().GetResult();
        }

    }
}
