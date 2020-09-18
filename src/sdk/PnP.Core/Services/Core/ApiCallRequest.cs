namespace PnP.Core.Services
{
    /// <summary>
    /// Class that will be passed along during API overrides
    /// </summary>
    internal class ApiCallRequest
    {
        internal ApiCallRequest(ApiCall api)
        {
            ApiCall = api;
        }

        /// <summary>
        /// The actual API call to override
        /// </summary>
        internal ApiCall ApiCall { get; set; }

        /// <summary>
        /// Cancel this request, if for some reason the underlying API call cannot be made or makes no sense
        /// </summary>
        internal bool Cancelled { get; private set; }

        /// <summary>
        /// Optional reason indicating why the request was cancelled
        /// </summary>
        internal string CancellationReason { get; private set; }

        /// <summary>
        /// Cancel this request
        /// </summary>
        internal void CancelRequest()
        {
            CancelRequest(null);
        }

        /// <summary>
        /// Cancel this request with a reason
        /// </summary>
        /// <param name="cancellationReason">Update cancellation reason</param>
        internal void CancelRequest(string cancellationReason)
        {
            Cancelled = true;
            CancellationReason = cancellationReason;
        }
    }
}
