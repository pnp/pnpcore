using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of List objects
    /// </summary>
    public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>
    {
    }
}