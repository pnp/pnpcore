using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItem objects
    /// </summary>
    public interface IListItemCollection : IQueryable<IListItem>, IDataModelCollection<IListItem>
    {
    }
}