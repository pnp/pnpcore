using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class FeatureCollection
    {
        public Task<IFeature> GetByIdAsync(Guid Id, params Expression<Func<IFeature, object>>[] expressions)
        {
            throw new NotImplementedException();
        }

        public async Task DisableAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!this.items.Any(o => o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature not activated");
            }

            var feature = this.items.FirstOrDefault(o => o.DefinitionId == id);

            if(feature != default)
            {
                var ftr = feature as Feature;
                await ftr.RemoveAsync().ConfigureAwait(false);
                this.items.Remove(feature);
            }            
        }

        /// <summary>
        /// Enabled a feature within the site or web
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IFeature> EnableAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if(this.items.Any(o=>o.DefinitionId == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Feature already activated");
            }

            var feature = CreateNewAndAdd() as Feature;

            feature.DefinitionId = id;

            return await feature.AddAsync().ConfigureAwait(false) as Feature;
        }


    }
}
