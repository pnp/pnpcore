using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of List objects
    /// </summary>
    [ConcreteType(typeof(ListCollection))]
    public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>
    {
    }
}