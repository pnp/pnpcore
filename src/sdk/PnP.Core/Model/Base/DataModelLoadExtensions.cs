using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extension methods for <see cref="IDataModelLoad"/>
    /// </summary>
    public static class DataModelLoadExtensions
    {
        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static Task<IBatchResult> LoadBatchAsync<TModel>(this IDataModelLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(null, expressions);
        }

        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchResult LoadBatch<TModel>(this IDataModelLoad<TModel> dataModelLoad, Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static IBatchResult LoadBatch<TModel>(this IDataModelLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            return dataModelLoad.LoadBatchAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModelLoad"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public static void Load<TModel>(this IDataModelLoad<TModel> dataModelLoad, params Expression<Func<TModel, object>>[] expressions)
        {
            if (dataModelLoad == null)
            {
                throw new ArgumentNullException(nameof(dataModelLoad));
            }

            dataModelLoad.LoadAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
#pragma warning disable CA1801 // Review unused parameters
#pragma warning disable IDE0060 // Remove unused parameter
        public static TModel QueryProperties<TModel>(this IDataModelLoad<TModel> dataModel, params Expression<Func<TModel, object>>[] expressions)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Review unused parameters
        {
            throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_QueryPropertiesUse);
        }
    }
}
