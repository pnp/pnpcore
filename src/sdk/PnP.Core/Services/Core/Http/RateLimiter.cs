using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Rate limiter class, will delay outgoing requests based upon ratelimit headers received from previous requests. 
    /// Goal of the delaying is to prevent getting throttled, resulting in a better overall throughput
    /// </summary>
    internal sealed class RateLimiter
    {
        internal const string RATELIMIT_LIMIT = "RateLimit-Limit";
        internal const string RATELIMIT_REMAINING = "RateLimit-Remaining";
        internal const string RATELIMIT_RESET = "RateLimit-Reset";        

        /// <summary>
        /// Lock for controlling Read/Write access to the variables.
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Maximum number of requests per window
        /// </summary>
        private int limit;

        /// <summary>
        /// The time, in <see cref="TimeSpan.Seconds"/>, when the current window gets reset
        /// </summary>
        private int reset;

        /// <summary>
        /// The timestamp when current window will be reset, in <see cref="TimeSpan.Ticks"/>.
        /// </summary>
        private long nextReset;

        /// <summary>
        /// The remaining requests in the current window.
        /// </summary>
        private int remaining;

        /// <summary>
        /// Minimum % of requests left before the next request will get delayed until the current window is reset. Defaults to 0, 
        /// use configuration RateLimiterMinimumCapacityLeft to activate
        /// </summary>
        //private readonly int minimumCapacityLeft = 0;

        internal PnPGlobalSettingsOptions GlobalSettings { get; private set; }

        /// <summary>
        /// Minimum % of requests left before the next request will get delayed until the current window is reset. Defaults to 0, 
        /// use configuration RateLimiterMinimumCapacityLeft to activate
        /// </summary>
        internal int MinimumCapacityLeft { get; set; } = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RateLimiter(ILogger<RetryHandlerBase> log, IOptions<PnPGlobalSettingsOptions> globalSettings)
        {
            GlobalSettings = globalSettings?.Value;
            if (GlobalSettings != null && GlobalSettings.Logger == null)
            {
                GlobalSettings.Logger = log;
            }
            
            if (GlobalSettings != null)
            {
                MinimumCapacityLeft = GlobalSettings.HttpRateLimiterMinimumCapacityLeft;
            }

            readerWriterLock.EnterWriteLock();
            try
            {
                _ = Interlocked.Exchange(ref limit, -1);
                _ = Interlocked.Exchange(ref remaining, -1);
                _ = Interlocked.Exchange(ref reset, -1);
                _ = Interlocked.Exchange(ref nextReset, DateTime.UtcNow.Ticks);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }
        
        internal async Task WaitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // We're not using the rate limiter
            if (MinimumCapacityLeft == 0)
            {
                return;
            }

            long delayInTicks = 0;
            float capacityLeft = 0;
            readerWriterLock.EnterReadLock();
            try
            {                                
                // Remaining = 0 means the request is throttled and there's a retry-after header that will be used
                if (limit > 0 && remaining > 0)
                {
                    // Calculate percentage requests left in the current window
                    capacityLeft = (float)remaining / limit * 100;

                    // If getting below the minimum required capacity then lets wait until the current window is reset
                    if (capacityLeft <= MinimumCapacityLeft)
                    {
                        delayInTicks = nextReset - DateTime.UtcNow.Ticks;
                    }
                }
            }
            finally
            {
                readerWriterLock.ExitReadLock();
            }

            if (delayInTicks > 0)
            {
                if (GlobalSettings != null && GlobalSettings.Logger != null)
                {
                    GlobalSettings.Logger.LogInformation($"Delaying request for {new TimeSpan(delayInTicks).Seconds} seconds because remaining request capacity for the current window is at {capacityLeft}%, so below the {MinimumCapacityLeft}% threshold.");
                }
                
                await Task.Delay(new TimeSpan(delayInTicks), cancellationToken).ConfigureAwait(false);
            }
        }

        internal void UpdateWindow(HttpResponseMessage response)
        {
            int rateLimit = -1;
            int rateRemaining = -1;
            int rateReset = -1;

            // We're not using the rate limiter
            if (MinimumCapacityLeft == 0)
            {
                return;
            }

            if (response != null)
            {
                if (response.Headers.TryGetValues(RATELIMIT_LIMIT, out IEnumerable<string> limitValues))
                {
                    string rateString = limitValues.First();
                    _ = int.TryParse(rateString, out rateLimit);
                }

                if (response.Headers.TryGetValues(RATELIMIT_REMAINING, out IEnumerable<string> remainingValues))
                {
                    string rateString = remainingValues.First();
                    _ = int.TryParse(rateString, out rateRemaining);
                }

                if (response.Headers.TryGetValues(RATELIMIT_RESET, out IEnumerable<string> resetValues))
                {
                    string rateString = resetValues.First();
                    _ = int.TryParse(rateString, out rateReset);
                }

                readerWriterLock.EnterWriteLock();
                try
                {                    
                    _ = Interlocked.Exchange(ref limit, rateLimit);
                    _ = Interlocked.Exchange(ref remaining, rateRemaining);
                    _ = Interlocked.Exchange(ref reset, rateReset);
                    
                    if (rateReset > -1)
                    {
                        // Track when the current window get's reset
                        _ = Interlocked.Exchange(ref nextReset, DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(rateReset).Ticks);
                    }                    
                }
                finally
                {
                    readerWriterLock.ExitWriteLock();
                }                
            }
        }

    }
}
