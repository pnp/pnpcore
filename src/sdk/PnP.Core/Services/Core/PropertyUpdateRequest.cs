namespace PnP.Core.Services
{
    /// <summary>
    /// Class used to provide information on a field update request
    /// </summary>
    internal class PropertyUpdateRequest
    {

        internal PropertyUpdateRequest(string fieldName, object fieldValue)
        {
            PropertyName = fieldName;
            Value = fieldValue;
        }

        /// <summary>
        /// Information about the property being updated
        /// </summary>
        internal string PropertyName { get; private set; }

        /// <summary>
        /// Value being set to the property
        /// </summary>
        internal object Value { get; set; }

        /// <summary>
        /// Cancel this update, if for some reason the property update should not continue
        /// </summary>
        internal bool Cancelled { get; private set; } = false;

        /// <summary>
        /// Optional reason indicating why the update was cancelled
        /// </summary>
        internal string CancellationReason { get; private set; }

        /// <summary>
        /// Cancel this update
        /// </summary>
        internal void CancelUpdate()
        {
            CancelUpdate(null);
        }

        /// <summary>
        /// Cancel this update with a reason
        /// </summary>
        /// <param name="cancellationReason">Update cancellation reason</param>
        internal void CancelUpdate(string cancellationReason)
        {
            Cancelled = true;
            CancellationReason = cancellationReason;
        }

    }
}
