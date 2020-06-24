using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ActivityEntity objects
    /// </summary>
    public interface IActivityEntityCollection : IQueryable<IActivityEntity>, IDataModelCollection<IActivityEntity>
    {
    }
}