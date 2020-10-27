using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of site or web scoped features
    /// </summary>
    [ConcreteType(typeof(FeatureCollection))]
    public interface IFeatureCollection : IQueryable<IFeature>, IDataModelCollection<IFeature>
    {
        /// <summary>
        /// Enable a feature
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public Task<IFeature> EnableAsync(Guid id);

        /// <summary>
        /// Enable a feature
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public IFeature Enable(Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public Task<IFeature> EnableBatchAsync(Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public IFeature EnableBatch(Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature enable request to</param>
        /// <returns>Enabled feature</returns>
        public Task<IFeature> EnableBatchAsync(Batch batch, Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature enable request to</param>
        /// <returns>Enabled feature</returns>
        public IFeature EnableBatch(Batch batch, Guid id);

        /// <summary>
        /// Disable a feature
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public Task DisableAsync(Guid id);

        /// <summary>
        /// Disable a feature
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public void Disable(Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public Task DisableBatchAsync(Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public void DisableBatch(Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature disable request to</param>
        /// <returns></returns>
        public Task DisableBatchAsync(Batch batch, Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature disable request to</param>
        /// <returns></returns>
        public void DisableBatch(Batch batch, Guid id);

    }
}
