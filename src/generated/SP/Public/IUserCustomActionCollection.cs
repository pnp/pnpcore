using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of UserCustomAction objects
    /// </summary>
    [ConcreteType(typeof(UserCustomActionCollection))]
    public interface IUserCustomActionCollection : IQueryable<IUserCustomAction>, IDataModelCollection<IUserCustomAction>
    {
    }
}