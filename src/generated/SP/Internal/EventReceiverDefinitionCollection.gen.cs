using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of EventReceiverDefinition Domain Model objects
    /// </summary>
    internal partial class EventReceiverDefinitionCollection : QueryableDataModelCollection<IEventReceiverDefinition>, IEventReceiverDefinitionCollection
    {
        public EventReceiverDefinitionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}