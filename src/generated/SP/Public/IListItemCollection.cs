using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItem objects
    /// </summary>
    [ConcreteType(typeof(ListItemCollection))]
    public interface IListItemCollection : IQueryable<IListItem>, IDataModelCollection<IListItem>
    {
    }
}