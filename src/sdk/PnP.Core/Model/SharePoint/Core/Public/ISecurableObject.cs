using PnP.Core.Model.Security;
using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the properties and methods for securable object (Web, List, ListItem)
    /// </summary>
    public interface ISecurableObject
    {
        /// <summary>
        /// Role Assignments defined on this securable object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IRoleAssignmentCollection RoleAssignments { get; }

        /// <summary>
        /// Returns if the securable object has unique role assignments
        /// </summary>
        public bool HasUniqueRoleAssignments { get; }

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        public Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        /// <returns></returns>
        public void BreakRoleInheritanceBatch(Batch batch, bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        /// <returns></returns>
        public Task BreakRoleInheritanceBatchAsync(Batch batch, bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        /// <returns></returns>
        public void BreakRoleInheritanceBatch(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Creates unique role assignments for this securable object.
        /// </summary>
        /// <param name="copyRoleAssignments">Specifies whether to copy the role assignments from the parent securable object. If the value is false, the collection of role assignments must contain only 1 role assignment containing the current user after the operation.</param>
        /// <param name="clearSubscopes">If the securable object is a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects in the current site and in the sites which inherit role assignments from the current site must be cleared and those securable objects will inherit role assignments from the current site after this call. If the securable object is a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged. If the securable object is not a site, and the clearsubscopes parameter is true, the role assignments for all child securable objects must be cleared and those securable objects will inherit role assignments from the current securable object after this call. If the securable object is not a site, and the clearsubscopes parameter is false, the role assignments for all child securable objects which do not inherit role assignments from their parent object must remain unchanged.</param>
        public Task BreakRoleInheritanceBatchAsync(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        public void ResetRoleInheritance();

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        public Task ResetRoleInheritanceAsync();

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <returns></returns>
        public void ResetRoleInheritanceBatch(Batch batch);

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <returns></returns>
        public Task ResetRoleInheritanceBatchAsync(Batch batch);

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        /// <returns></returns>
        public void ResetRoleInheritanceBatch();

        /// <summary>
        /// Removes the local role assignments so that the web, and all its descendant objects, re-inherit role assignments from the parent object.
        /// </summary>
        /// <returns></returns>
        public Task ResetRoleInheritanceBatchAsync();

        /// <summary>
        /// Returns the roles for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <returns></returns>
        public IRoleDefinitionCollection GetRoleDefinitions(int principalId);

        /// <summary>
        /// Returns the roles for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <returns></returns>
        public Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync(int principalId);

        /// <summary>
        /// Adds roles for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <param name="names">Roles to add</param>
        /// <returns></returns>
        public bool AddRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// Adds roles for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <param name="names">Roles to add</param>
        /// <returns></returns>
        public Task<bool> AddRoleDefinitionsAsync(int principalId, params string[] names);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinition(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes roles for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <param name="names">Roles to remove</param>
        /// <returns></returns>
        public bool RemoveRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// Removes role for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId">Id of the user or group</param>
        /// <param name="names">Roles to remove</param>
        /// <returns></returns>
        public Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinition(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition);

    }
}
