using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of WorkflowAssociation objects
    /// </summary>
    public interface IWorkflowAssociationCollection : IQueryable<IWorkflowAssociation>, IDataModelCollection<IWorkflowAssociation>
    {
    }
}