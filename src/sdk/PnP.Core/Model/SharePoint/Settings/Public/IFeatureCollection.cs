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
    
    public interface IFeatureCollection : IQueryable<IFeature>,IDataModelCollection<IFeature>
    {

        public Task<IFeature> GetByIdAsync(Guid Id, params Expression<Func<IFeature, object>>[] expressions);

        public Task<IFeature> EnableAsync(Guid id);

        public Task<IFeature> DisableAsync(Guid id);

        //public Task<IFeature> AddAsync(string id);

        //public IFeature Add(Batch batch, string id, bool force, int featureDefScope);

        //public Task<IFeature> AddAsync(string id, bool force, int featureDefScope);


        //public void Remove(string id, bool force);

        //public Task RemoveAsync(string id, bool force);

    }
}
