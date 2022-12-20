using System;
using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Event containing information about the happened rate limit event
    /// </summary>
    public interface IRateLimitEvent
    {
        /// <summary>
        /// Request that was retried
        /// </summary>
        Uri Request { get; }

        /// <summary>
        /// The time, in <see cref="TimeSpan.Seconds"/>, when the current window gets reset
        /// </summary>
        int Reset { get; }

        /// <summary>
        /// Maximum number of requests per window
        /// </summary>
        int Limit { get; }

        /// <summary>
        /// The remaining requests in the current window.
        /// </summary>
        int Remaining { get; }

        /// <summary>
        /// Event property bag
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}
