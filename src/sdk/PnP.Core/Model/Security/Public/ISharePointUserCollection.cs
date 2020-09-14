using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint users
    /// </summary>
    public interface ISharePointUserCollection : IQueryable<ISharePointUser>, IDataModelCollection<ISharePointUser>
    {
    }
}
