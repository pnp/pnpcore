using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItemVersion objects
    /// </summary>
    public interface IListItemVersionCollection : IQueryable<IListItemVersion>, IDataModelCollection<IListItemVersion>
    {
    }
}