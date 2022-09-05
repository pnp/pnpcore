using System;

namespace PnP.Core.Services.Core
{
    /// <summary>
    /// Class that allows a PnP Core SDK consumer to hookup with events being triggered from within PnP Core SDK
    /// </summary>
    public sealed class EventHub
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventHub()
        {
        }

        /// <summary>
        /// Event to subscribe to get notified whenever a request is getting retried due to throttling or an error
        /// </summary>
        public Action<RetryEvent> RequestRetry { get; set; }

    }
}
