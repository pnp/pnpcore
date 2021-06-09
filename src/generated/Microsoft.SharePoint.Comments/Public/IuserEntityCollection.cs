using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of userEntity objects
    /// </summary>
    [ConcreteType(typeof(userEntityCollection))]
    public interface IuserEntityCollection : IQueryable<IuserEntity>, IDataModelCollection<IuserEntity>
    {
    }
}