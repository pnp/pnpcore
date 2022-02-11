using System;

namespace PnP.Core.Services.Core
{
    /// <summary>
    /// Event containing information about the happened retry
    /// </summary>
    public sealed class RetryEvent
    {
        /// <summary>
        /// Retry event constructor
        /// </summary>
        /// <param name="request">Request that's retried</param>
        /// <param name="httpStatusCode">Http status code</param>
        /// <param name="errorMessage">Socket exception that triggered the retry</param>
        internal RetryEvent(Uri request, int httpStatusCode, string errorMessage)
        {
            Request = request;
            HttpStatusCode = httpStatusCode;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Request that was retried
        /// </summary>
        public Uri Request { get; private set; }

        /// <summary>
        /// Http status code for the retried request
        /// </summary>
        public int HttpStatusCode { get; private set; }

        /// <summary>
        /// Error message (SocketException) that triggered the retry
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
