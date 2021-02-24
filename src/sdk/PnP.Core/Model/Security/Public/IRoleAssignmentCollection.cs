using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint groups
    /// </summary>
    [ConcreteType(typeof(RoleAssignmentCollection))]
    public interface IRoleAssignmentCollection : IQueryable<IRoleAssignment>, IDataModelCollection<IRoleAssignment>
    {
        /// <summary>
        /// Adds a role assignment
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefId"></param>
        /// <returns></returns>
        IRoleAssignment Add(int principalId, int roleDefId);

        /// <summary>
        /// Adds a role assignment
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefId"></param>
        /// <returns></returns>
        Task<IRoleAssignment> AddAsync(int principalId, int roleDefId);

        /// <summary>
        /// Removes a role assignment
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefId"></param>
        void Remove(int principalId, int roleDefId);

        /// <summary>
        /// Removes a role assignment
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefIf"></param>
        /// <returns></returns>
        Task RemoveAsync(int principalId, int roleDefIf);
    }
}
