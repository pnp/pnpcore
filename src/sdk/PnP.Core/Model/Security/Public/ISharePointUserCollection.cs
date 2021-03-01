using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint users
    /// </summary>
    [ConcreteType(typeof(SharePointUserCollection))]
    public interface ISharePointUserCollection : IQueryable<ISharePointUser>, IAsyncEnumerable<ISharePointUser>, IDataModelCollection<ISharePointUser>, IDataModelCollectionLoad<ISharePointUser>
    {
    }
}
