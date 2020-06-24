using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of site or web scoped features
    /// </summary>
    public interface IFeatureCollection : IQueryable<IFeature>,IDataModelCollection<IFeature>
    {
        /// <summary>
        /// Enable a feature
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public Task<IFeature> EnableAsync(Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <returns>Enabled feature</returns>
        public IFeature Enable(Guid id);

        /// <summary>
        /// Enable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to enable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature enable request to</param>
        /// <returns>Enabled feature</returns>
        public IFeature Enable(Batch batch, Guid id);

        /// <summary>
        /// Disable a feature
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public Task DisableAsync(Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <returns></returns>
        public void Disable(Guid id);

        /// <summary>
        /// Disable a feature in batch
        /// </summary>
        /// <param name="id">Id of the feature to disable</param>
        /// <param name="batch"><see cref="Batch"/> to add this feature disable request to</param>
        /// <returns></returns>
        public void Disable(Batch batch, Guid id);

    }
}
