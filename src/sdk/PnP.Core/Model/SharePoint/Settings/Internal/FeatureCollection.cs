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

        public void Disable(Guid id)
        {
            Disable(PnPContext.CurrentBatch, id);
        }

        public void Disable(Batch batch, Guid id)
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
                ftr.RemoveAsync().ConfigureAwait(false);
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

        public IFeature Enable(Guid id)
        {
            return Enable(PnPContext.CurrentBatch, id);
        }

        public IFeature Enable(Batch batch, Guid id)
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

            return feature.Add(batch) as Feature;
        }
    }
}
