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

        public Task<IFeature> EnableAsync(Guid id);

        public IFeature Enable(Guid id);

        public IFeature Enable(Batch batch, Guid id);

        public Task DisableAsync(Guid id);

        public void Disable(Guid id);

        public void Disable(Batch batch, Guid id);

    }
}
