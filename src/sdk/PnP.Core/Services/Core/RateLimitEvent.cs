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
        /// Rate limit event constructor
        /// </summary>
        /// <param name="requestMessage">Request that's retried</param>
        public RateLimitEvent(HttpResponseMessage requestMessage)
        {
            ProcessRequestProperties(requestMessage);
        }

        private void ProcessRequestProperties(HttpResponseMessage requestMessage)
        {
            int rateLimit = -1;
            int rateRemaining = -1;
            int rateReset = -1;

            if (requestMessage != null)
            {
                if (requestMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_LIMIT, out IEnumerable<string> limitValues))
                {
                    string rateString = limitValues.First();
                    _ = int.TryParse(rateString, out rateLimit);
                }

                if (requestMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_REMAINING, out IEnumerable<string> remainingValues))
                {
                    string rateString = remainingValues.First();
                    _ = int.TryParse(rateString, out rateRemaining);
                }

                if (requestMessage.Headers.TryGetValues(RateLimiter.RATELIMIT_RESET, out IEnumerable<string> resetValues))
                {
                    string rateString = resetValues.First();
                    _ = int.TryParse(rateString, out rateReset);
                }
            }

            Limit = rateLimit;
            Reset = rateReset;
            Remaining = rateRemaining;
        }
    }
}
