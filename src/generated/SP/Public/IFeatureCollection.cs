using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Feature objects
    /// </summary>
    [ConcreteType(typeof(FeatureCollection))]
    public interface IFeatureCollection : IQueryable<IFeature>, IDataModelCollection<IFeature>
    {
    }
}