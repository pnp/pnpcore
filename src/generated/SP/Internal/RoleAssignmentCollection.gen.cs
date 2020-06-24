using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of RoleAssignment Domain Model objects
    /// </summary>
    internal partial class RoleAssignmentCollection : QueryableDataModelCollection<IRoleAssignment>, IRoleAssignmentCollection
    {
        public RoleAssignmentCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}