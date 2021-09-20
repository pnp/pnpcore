using PnP.Core.QueryModel;
using PnP.Core.Services;
/* Unmerged change from project 'PnP.Core (net5.0)'
Before:
using System.Threading.Tasks;
using System.Linq;
After:
using System.Linq;
using System.Threading.Tasks;
*/


namespace PnP.Core.Model.Security
{
    internal partial class RoleAssignmentCollection : QueryableDataModelCollection<IRoleAssignment>, IRoleAssignmentCollection
    {
        public RoleAssignmentCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
