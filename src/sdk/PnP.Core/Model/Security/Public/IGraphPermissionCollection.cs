using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(GraphPermissionCollection))]
    public interface IGraphPermissionCollection : IDataModelCollection<IGraphPermission>
    {
    }
}
