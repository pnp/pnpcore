using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(GraphPermission))]
    public interface IGraphPermission : IDataModel<IGraphPermission>, IDataModelGet<IGraphPermission>, IDataModelLoad<IGraphPermission>, IQueryableDataModel
    {
        /// <summary>
        /// 
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        List<string> Roles { get; }

        /// <summary>
        /// 
        /// </summary>
        string ShareId { get; }

        /// <summary>
        /// 
        /// </summary>
        string ExpirationDateTime { get; }

        /// <summary>
        /// 
        /// </summary>
        bool HasPassword { get; }
    }
}
