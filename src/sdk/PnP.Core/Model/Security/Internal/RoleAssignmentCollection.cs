using PnP.Core.QueryModel;
using PnP.Core.Services;
using System.Threading.Tasks;
using System.Linq;

namespace PnP.Core.Model.Security
{
    internal partial class RoleAssignmentCollection : QueryableDataModelCollection<IRoleAssignment>, IRoleAssignmentCollection
    {
        public RoleAssignmentCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public IRoleAssignment Add(int principalId, int roleDefId)
        {
            return AddAsync(principalId, roleDefId).GetAwaiter().GetResult();
        }

        public async Task<IRoleAssignment> AddAsync(int principalId, int roleDefId)
        {
            var roleAssignment = CreateNewAndAdd() as RoleAssignment;
            roleAssignment.PrincipalId = principalId;

            return await roleAssignment.AddAsync(new System.Collections.Generic.Dictionary<string, object> { { "principalId", principalId }, { "roleDefId", roleDefId } }).ConfigureAwait(false) as RoleAssignment;
        }

        public void Remove(int principalId, int roleDefId)
        {
            RemoveAsync(principalId, roleDefId).ConfigureAwait(false);
        }

        public Task RemoveAsync(int principalId, int roleDefId)
        {
            var item = this.items.FirstOrDefault(i => i.PrincipalId == principalId);
            base.Remove(item);

            return item.DeleteAsync();
        }
    }
}
