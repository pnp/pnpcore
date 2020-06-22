using PnP.Core.Services;
using System;
using System.Collections.Generic;
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

        public Task<IFeature> DisableAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IFeature> EnableAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var feature = CreateNewAndAdd() as Feature;

            feature.DefinitionId = id;

            return await feature.AddAsync().ConfigureAwait(false) as Feature;
        }


    }
}
