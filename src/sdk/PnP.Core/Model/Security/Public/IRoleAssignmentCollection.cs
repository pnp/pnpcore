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
    }
}
