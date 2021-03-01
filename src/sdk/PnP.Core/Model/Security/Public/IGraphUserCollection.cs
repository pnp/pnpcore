using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of Microsoft 365 users
    /// </summary>
    [ConcreteType(typeof(GraphUserCollection))]
    public interface IGraphUserCollection : IQueryable<IGraphUser>, IAsyncEnumerable<IGraphUser>, IDataModelCollection<IGraphUser>, IDataModelCollectionLoad<IGraphUser>
    {
    }
}
