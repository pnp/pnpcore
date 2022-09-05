namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options that will have to be filled in when creating a new Event Receiver
    /// </summary>
    public class EventReceiverOptions
    {
        /// <summary>
        /// Type of event receiver to create.
        /// </summary>
        public EventReceiverType EventType { get; set; } = EventReceiverType.InvalidReceiver;

        /// <summary>
        /// Name of the event receiver.
        /// </summary>
        public string ReceiverName { get; set; }

        /// <summary>
        /// Assembly holding the event receiver code
        /// </summary>
        public string ReceiverAssembly { get; set; }

        /// <summary>
        /// Class containing the event receiver code
        /// </summary>
        public string ReceiverClass { get; set; }

        /// <summary>
        /// Url of the event receiver.
        /// </summary>
        public string ReceiverUrl { get; set; }

        /// <summary>
        /// Sequence number in which the event receiver will be executed.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// The synchronization state for the event receiver.
        /// </summary>
        public EventReceiverSynchronization Synchronization { get; set; } = EventReceiverSynchronization.DefaultSynchronization;
    }
}
