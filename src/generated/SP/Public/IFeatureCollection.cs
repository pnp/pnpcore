using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Feature objects
    /// </summary>
    public interface IFeatureCollection : IQueryable<IFeature>, IDataModelCollection<IFeature>
    {
    }
}