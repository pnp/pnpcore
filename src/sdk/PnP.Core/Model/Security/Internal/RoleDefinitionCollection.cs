using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    internal sealed class RoleDefinitionCollection : QueryableDataModelCollection<IRoleDefinition>, IRoleDefinitionCollection
    {

        public RoleDefinitionCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public IRoleDefinition Add(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            return AddAsync(name, roleTypeKind, permissions, description, hidden, order).GetAwaiter().GetResult();
        }

        public IRoleDefinition AddBatch(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, name, roleTypeKind, permissions, description, hidden, order).GetAwaiter().GetResult();
        }

        public IRoleDefinition AddBatch(Batch batch, string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            return AddBatchAsync(batch, name, roleTypeKind, permissions, description, hidden, order).GetAwaiter().GetResult();
        }

        public async Task<IRoleDefinition> AddAsync(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            BuildRoleDefinitionAdd(name, roleTypeKind, permissions, description, hidden, order, out RoleDefinition newRoleDefinition, out BasePermissions basePermissions);

            return await newRoleDefinition.AddAsync(new System.Collections.Generic.Dictionary<string, object> { { "permissions", basePermissions } }).ConfigureAwait(false) as RoleDefinition;
        }

        private void BuildRoleDefinitionAdd(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description, bool hidden, int order, out RoleDefinition newRoleDefinition, out BasePermissions basePermissions)
        {
            newRoleDefinition = CreateNewAndAdd() as RoleDefinition;
            newRoleDefinition.Name = name;
            newRoleDefinition.Description = description;
            newRoleDefinition.Hidden = hidden;
            newRoleDefinition.Order = order;
            newRoleDefinition.RoleTypeKind = roleTypeKind;
            basePermissions = new BasePermissions();
            basePermissions.Set(PermissionKind.EmptyMask);
            foreach (var permission in permissions)
            {
                basePermissions.Set(permission);
            }
        }

        public async Task<IRoleDefinition> AddBatchAsync(string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, roleTypeKind, permissions, description, hidden, order).ConfigureAwait(false);
        }

        public async Task<IRoleDefinition> AddBatchAsync(Batch batch, string name, RoleType roleTypeKind, PermissionKind[] permissions, string description = null, bool hidden = false, int order = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            BuildRoleDefinitionAdd(name, roleTypeKind, permissions, description, hidden, order, out RoleDefinition newRoleDefinition, out BasePermissions basePermissions);

            return await newRoleDefinition.AddBatchAsync(batch, new System.Collections.Generic.Dictionary<string, object> { { "permissions", basePermissions } }).ConfigureAwait(false) as RoleDefinition;
        }
    }
}
