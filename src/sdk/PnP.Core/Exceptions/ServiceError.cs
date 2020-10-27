namespace PnP.Core
{
    /// <summary>
    /// Error information for a service error
    /// </summary>
    public class ServiceError : BaseError
    {
        /// <summary>
        /// <see cref="ServiceError"/> constructor using error type and http response code to create a backend service request error
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="httpResponseCode">Http response code of the error</param>
        public ServiceError(ErrorType type, int httpResponseCode) : base(type)
        {
            HttpResponseCode = httpResponseCode;
        }

        /// <summary>
        /// Http response code that was linked to the service error
        /// </summary>
        public int HttpResponseCode { get; private set; }

        /// <summary>
        /// Error message that was linked to the service error
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Client request id header returned in the response.
        /// </summary>
        public string ClientRequestId { get; internal set; }

        /// <summary>
        /// Graph error code
        /// </summary>
        public string Code { get; internal set; }

    }
}
