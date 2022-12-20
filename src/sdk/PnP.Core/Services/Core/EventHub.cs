using System;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Class that allows a PnP Core SDK consumer to hookup with events being triggered from within PnP Core SDK
    /// </summary>
    public sealed class EventHub
    {
        /// <summary>
        /// Delegate for the <see cref="RequestRateLimitWaitAsync"/> event
        /// </summary>
        /// <param name="cancellationToken">Current cancellation token</param>
        /// <returns></returns>
        public delegate Task RequestRateLimitWaitDelegate(CancellationToken cancellationToken);

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventHub()
        {
        }

        /// <summary>
        /// Event to subscribe to get notified whenever a request is getting retried due to throttling or an error
        /// </summary>
        public Action<IRetryEvent> RequestRetry { get; set; }

        /// <summary>
        /// Event so subscribe to for getting event rate limit information
        /// </summary>
        public Action<IRateLimitEvent> RequestRateLimitUpdate { get; set; }

        /// <summary>
        /// Event to subscribe to for implementing a delay due to the rate limit information received via <see cref="RequestRateLimitUpdate"/>.
        /// </summary>        
        public RequestRateLimitWaitDelegate RequestRateLimitWaitAsync { get; set; }
    }
}
