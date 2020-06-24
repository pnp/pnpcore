using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of EventReceiverDefinition objects
    /// </summary>
    public interface IEventReceiverDefinitionCollection : IQueryable<IEventReceiverDefinition>, IDataModelCollection<IEventReceiverDefinition>
    {
    }
}