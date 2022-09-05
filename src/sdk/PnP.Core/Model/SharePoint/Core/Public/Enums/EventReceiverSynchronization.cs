namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the synchronization state for the specified event receiver.
    /// </summary>
    public enum EventReceiverSynchronization
    {
        /// <summary>
        /// Indicates to run the event receiver synchronously if it is a Before event. Indicates to run the event receiver asynchronously if it is an After event.
        /// </summary>
        DefaultSynchronization = 0,

        /// <summary>
        /// Indicates to run the event receiver synchronously.
        /// </summary>
        Synchronous = 1,

        /// <summary>
        /// Indicates to run the event receiver asynchronously.
        /// </summary>
        Asynchronous = 2,
    }
}
