using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItemVersion objects
    /// </summary>
    [ConcreteType(typeof(ListItemVersionCollection))]
    public interface IListItemVersionCollection : IQueryable<IListItemVersion>, IDataModelCollection<IListItemVersion>
    {
    }
}