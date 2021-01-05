using System.Linq;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint groups
    /// </summary>
    [ConcreteType(typeof(RoleAssignmentCollection))]
    public interface IRoleAssignmentCollection : IQueryable<IRoleAssignment>, IDataModelCollection<IRoleAssignment>
    {
    }
}
