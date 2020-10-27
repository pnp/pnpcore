using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of User objects
    /// </summary>
    [ConcreteType(typeof(UserCollection))]
    public interface IUserCollection : IQueryable<IUser>, IDataModelCollection<IUser>
    {
    }
}