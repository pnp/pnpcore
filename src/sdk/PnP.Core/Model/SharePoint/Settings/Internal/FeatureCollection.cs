using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FeatureCollection
    {
        public async Task DisableAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!this.items.Any(o => o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature was not activated, nothing to deactive");
            }

            var feature = this.items.FirstOrDefault(o => o.DefinitionId == id);

            if (feature != default)
            {
                var ftr = feature as Feature;
                await ftr.RemoveAsync().ConfigureAwait(false);
                this.items.Remove(feature);
            }
        }

        public async Task DisableBatchAsync(Guid id)
        {
            await DisableBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public async Task DisableBatchAsync(Batch batch, Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!this.items.Any(o => o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature was not activated, nothing to deactive");
            }

            var feature = this.items.FirstOrDefault(o => o.DefinitionId == id);

            if (feature != default)
            {
                var ftr = feature as Feature;
                await ftr.RemoveBatchAsync(batch).ConfigureAwait(false);
                this.items.Remove(feature);
            }
        }

        public async Task<IFeature> EnableAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if(this.items.Any(o=>o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature was already activated");
            }

            var feature = CreateNewAndAdd() as Feature;

            feature.DefinitionId = id;

            return await feature.AddAsync().ConfigureAwait(false) as Feature;
        }

        public async Task<IFeature> EnableBatchAsync(Guid id)
        {
            return await EnableBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public async Task<IFeature> EnableBatchAsync(Batch batch, Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (this.items.Any(o => o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature was already activated");
            }

            var feature = CreateNewAndAdd() as Feature;
            feature.DefinitionId = id;

            return await feature.AddBatchAsync(batch).ConfigureAwait(false) as Feature;
        }
    }
}
