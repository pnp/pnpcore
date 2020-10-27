using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ActivityEntity objects
    /// </summary>
    [ConcreteType(typeof(ActivityEntityCollection))]
    public interface IActivityEntityCollection : IQueryable<IActivityEntity>, IDataModelCollection<IActivityEntity>
    {
    }
}