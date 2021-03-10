using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint RoleDefinitions
    /// </summary>
    [ConcreteType(typeof(RoleDefinitionCollection))]
    public interface IRoleDefinitionCollection : IQueryable<IRoleDefinition>, IAsyncEnumerable<IRoleDefinition>, IDataModelCollection<IRoleDefinition>, IDataModelCollectionLoad<IRoleDefinition>
    {
        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        IRoleDefinition Add(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);

        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        IRoleDefinition AddBatch(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);

        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        IRoleDefinition AddBatch(Batch batch, string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);

        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<IRoleDefinition> AddAsync(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);

        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<IRoleDefinition> AddBatchAsync(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);

        /// <summary>
        /// Adds a new role definition
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="name"></param>
        /// <param name="roleTypeKind"></param>
        /// <param name="permissions"></param>
        /// <param name="description"></param>
        /// <param name="hidden"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<IRoleDefinition> AddBatchAsync(Batch batch, string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0);
    }
}
