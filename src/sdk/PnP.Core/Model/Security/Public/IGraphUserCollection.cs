using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of Microsoft 365 users
    /// </summary>
    public interface IGraphUserCollection : IQueryable<IGraphUser>, IDataModelCollection<IGraphUser>
    {
    }
}
