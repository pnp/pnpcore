using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class that defines general properties of an event receiver for lists and webs
    /// </summary>
    [ConcreteType(typeof(EventReceiverDefinition))]
    public interface IEventReceiverDefinition : IDataModel<IEventReceiverDefinition>, IDataModelGet<IEventReceiverDefinition>, IDataModelLoad<IEventReceiverDefinition>, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// The identifier of the receiver for the event.
        /// </summary>
        public Guid ReceiverId { get; }

        /// <summary>
        /// The fully-qualified name of the assembly that contains the event receiver for the feature.
        /// </summary>
        public string ReceiverAssembly { get; }

        /// <summary>
        /// The name of the receiver for the event.
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// The class name of the event receiver for the feature.
        /// </summary>
        public string ReceiverClass { get; }

        /// <summary>
        /// The integer that represents the relative sequence of the event.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Enumeration that specifies the synchronization state for the event receiver.
        /// </summary>
        public EventReceiverSynchronization Synchronization { get; set; }

        /// <summary>
        /// The types of events that can be handled by event receivers.
        /// </summary>
        public EventReceiverType EventType { get; set; }

        /// <summary>
        /// The URL of the receiver for the event.
        /// </summary>
        public string ReceiverUrl { get; set; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }
    }
}
