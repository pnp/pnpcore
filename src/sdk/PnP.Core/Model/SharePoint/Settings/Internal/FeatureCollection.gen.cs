using PnP.Core.QueryModel.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class FeatureCollection : QueryableDataModelCollection<IFeature>, IFeatureCollection
    {
       
        public IFeature Add(string id, bool force, int featureDefScope)
        {
            throw new NotImplementedException();
        }

        public IFeature Get(string id)
        {
            throw new NotImplementedException();
        }

        
        public IFeature Remove(string id, bool force)
        {
            throw new NotImplementedException();
        }

        IEnumerator<IFeatureCollection> IEnumerable<IFeatureCollection>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
