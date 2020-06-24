using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of WorkflowTemplate objects
    /// </summary>
    public interface IWorkflowTemplateCollection : IQueryable<IWorkflowTemplate>, IDataModelCollection<IWorkflowTemplate>
    {
    }
}