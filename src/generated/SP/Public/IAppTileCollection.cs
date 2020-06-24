using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of AppTile objects
    /// </summary>
    public interface IAppTileCollection : IQueryable<IAppTile>, IDataModelCollection<IAppTile>
    {
    }
}