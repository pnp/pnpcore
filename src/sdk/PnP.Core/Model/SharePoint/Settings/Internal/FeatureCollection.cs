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
        public IFeature Add(string featureId)
        {
            return Add(featureId, false, 2);
        }

        public IFeature Add(string featureId, bool force, int featdefScope)
        {
            throw new NotImplementedException();
        }

        public IFeature Add(Batch batch, string id, bool force, int featureDefScope)
        {
            throw new NotImplementedException();
        }

        public Task<IFeature> AddAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IFeature> AddAsync(string id, bool force, int featureDefScope)
        {
            throw new NotImplementedException();
        }

        public IFeature GetById(Guid Id, params Expression<Func<IFeature, object>>[] expressions)
        {
            throw new NotImplementedException();
        }

        public Task<IFeature> GetByIdAsync(Guid Id, params Expression<Func<IFeature, object>>[] expressions)
        {
            throw new NotImplementedException();
        }

        public IFeature Remove(string id, bool force)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string id, bool force)
        {
            throw new NotImplementedException();
        }

        void IFeatureCollection.Remove(string id, bool force)
        {
            throw new NotImplementedException();
        }
    }
}
