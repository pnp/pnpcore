using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of WorkflowTemplate Domain Model objects
    /// </summary>
    internal partial class WorkflowTemplateCollection : QueryableDataModelCollection<IWorkflowTemplate>, IWorkflowTemplateCollection
    {
        public WorkflowTemplateCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}