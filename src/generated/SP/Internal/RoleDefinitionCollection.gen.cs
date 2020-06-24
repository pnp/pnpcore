using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of RoleDefinition Domain Model objects
    /// </summary>
    internal partial class RoleDefinitionCollection : QueryableDataModelCollection<IRoleDefinition>, IRoleDefinitionCollection
    {
        public RoleDefinitionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}