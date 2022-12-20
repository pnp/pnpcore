using System;
using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Event containing information about the happened retry
    /// </summary>
    public interface IRetryEvent
    {
        /// <summary>
        /// Request that was retried
        /// </summary>
        Uri Request { get; }

        /// <summary>
        /// Http status code for the retried request
        /// </summary>
        int HttpStatusCode { get; }

        /// <summary>
        /// Wait before the next try in seconds
        /// </summary>
        int WaitTime { get; }

        /// <summary>
        /// SocketException that triggered the retry
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Event property bag
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}
