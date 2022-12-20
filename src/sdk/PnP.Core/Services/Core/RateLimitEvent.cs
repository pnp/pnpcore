using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Event containing information about the happened rate limit event
    /// </summary>
    public sealed class RateLimitEvent : IRateLimitEvent
    {
        /// <summary>
        /// Request that was retried
        /// </summary>
        public Uri Request { get; private set; }

        /// <summary>
        /// The time, in <see cref="TimeSpan.Seconds"/>, when the current window gets reset
        /// </summary>
        public int Reset { get; private set; }

        /// <summary>
        /// Maximum number of requests per window
        /// </summary>
        public int Limit { get; private set; }

        /// <summary>
        /// The remaining requests in the current window.
        /// </summary>
        public int Remaining { get; private set; }

        /// <summary>
        /// Event property bag
        /// </summary>
        public IDictionary<string, object> Properties { get; internal set; } = new Dictionary<string, object>();

        /// <summary>
        /// Rate limit event constructor
        /// </summary>
        /// <param name="responseMessage">Request that's retried</param>
        /// <param name="requestMessage">Response for the retried request</param>
        public RateLimitEvent(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            if (requestMessage != null)
            {
                Request = requestMessage.RequestUri;
                ProcessRequestProperties(requestMessage);
            }
            
            ProcessResponseProperties(responseMessage);
        }

        private void ProcessResponseProperties(HttpResponseMessage responseMessage)
        {
            int rateLimit = -1;
            int rateRemaining = -1;
            int rateReset = -1;

            if (responseMessage != null)
            {
                if (responseMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_LIMIT, out IEnumerable<string> limitValues))
                {
                    string rateString = limitValues.First();
                    _ = int.TryParse(rateString, out rateLimit);
                }

                if (responseMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_REMAINING, out IEnumerable<string> remainingValues))
                {
                    string rateString = remainingValues.First();
                    _ = int.TryParse(rateString, out rateRemaining);
                }

                if (responseMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_RESET, out IEnumerable<string> resetValues))
                {
                    string rateString = resetValues.First();
                    _ = int.TryParse(rateString, out rateReset);
                }
            }

            Limit = rateLimit;
            Reset = rateReset;
            Remaining = rateRemaining;
        }

        private void ProcessRequestProperties(HttpRequestMessage requestMessage)
        {
#if NET5_0_OR_GREATER
            if (requestMessage.Options != null)
            {
                foreach(var property in requestMessage.Options)
                {
                    Properties[property.Key] = property.Value;
                }
            }
#else
            if (requestMessage.Properties != null && requestMessage.Properties.Count > 0)
            {
                foreach (var property in requestMessage.Properties)
                {
                    Properties[property.Key] = property.Value;
                }
            }
#endif
        }
    }
}
