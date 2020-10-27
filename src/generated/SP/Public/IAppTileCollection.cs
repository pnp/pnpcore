using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of AppTile objects
    /// </summary>
    [ConcreteType(typeof(AppTileCollection))]
    public interface IAppTileCollection : IQueryable<IAppTile>, IDataModelCollection<IAppTile>
    {
    }
}