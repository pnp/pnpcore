using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(FeatureCollection))]
    public interface IFeatureCollection : IQueryable<IFeatureCollection>, IDataModelCollection<IFeatureCollection>
    {
        IFeature Add(string id, bool force, int featureDefScope);

        IFeature Remove(string id, bool force);
    }
}
