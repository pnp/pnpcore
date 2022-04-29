using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(GraphPermissionCollection))]
    public interface IGraphPermissionCollection : IQueryable<IGraphPermission>, IAsyncEnumerable<IGraphPermission>, IDataModelCollection<IGraphPermission>, IDataModelCollectionLoad<IGraphPermission>, ISupportModules<IGraphPermissionCollection>
    {
    }
}
