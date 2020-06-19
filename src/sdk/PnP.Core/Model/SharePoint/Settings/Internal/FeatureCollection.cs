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

        public Task<IFeature> EnableAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
