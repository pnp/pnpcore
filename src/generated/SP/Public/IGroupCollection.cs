using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Group objects
    /// </summary>
    [ConcreteType(typeof(GroupCollection))]
    public interface IGroupCollection : IQueryable<IGroup>, IDataModelCollection<IGroup>
    {
    }
}