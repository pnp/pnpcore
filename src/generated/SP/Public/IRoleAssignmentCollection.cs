using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of RoleAssignment objects
    /// </summary>
    [ConcreteType(typeof(RoleAssignmentCollection))]
    public interface IRoleAssignmentCollection : IQueryable<IRoleAssignment>, IDataModelCollection<IRoleAssignment>
    {
    }
}