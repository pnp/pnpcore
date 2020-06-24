using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of User objects
    /// </summary>
    public interface IUserCollection : IQueryable<IUser>, IDataModelCollection<IUser>
    {
    }
}