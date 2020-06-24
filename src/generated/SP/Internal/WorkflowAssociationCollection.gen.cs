using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of WorkflowAssociation Domain Model objects
    /// </summary>
    internal partial class WorkflowAssociationCollection : QueryableDataModelCollection<IWorkflowAssociation>, IWorkflowAssociationCollection
    {
        public WorkflowAssociationCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}