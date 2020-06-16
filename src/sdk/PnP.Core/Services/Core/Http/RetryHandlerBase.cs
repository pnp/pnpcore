using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Retry handler for http requests
    /// Based upon: https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/dev/src/Microsoft.Graph.Core/Extensions/HttpRequestMessageExtensions.cs
    /// </summary>
    internal abstract class RetryHandlerBase: DelegatingHandler
    {
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        private const int MAXDELAY = 300;

        #region Construction
        public RetryHandlerBase(ISettings settingsClient)
        {
            SettingsClient = settingsClient;
        }
        
        public RetryHandlerBase(HttpMessageHandler innerHandler, ISettings settingsClient) : base(innerHandler)
        {
            SettingsClient = settingsClient;
        }
        #endregion

        internal ISettings SettingsClient { get; private set; }
        internal bool UseRetryAfterHeader { get; set; } = false;
        internal int MaxRetries { get; set; } = 10;
        internal int DelayInSeconds { get; set; } = 3;
        internal bool IncrementalDelay { get; set; } = true;

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (ShouldRetry(response))
            {
                // Handle retry handling
                response = await SendRetryAsync(response, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }

        /// <summary>
        /// Retry sending the HTTP request 
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> which is returned and includes the HTTP request needs to be retried.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the retry.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                // Drain response content to free responses.
                if (response.Content != null)
                {
                    await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, DelayInSeconds, cancellationToken);

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                using (var request = await response.RequestMessage.CloneAsync().ConfigureAwait(false))
                {
                    // Increase retryCount and then update Retry-Attempt in request header if needed
                    retryCount++;
                    AddOrUpdateRetryAttempt(request, retryCount);

                    // Delay time
                    await delay.ConfigureAwait(false);

                    // Call base.SendAsync to send the request
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (!ShouldRetry(response))
                    {
                        return response;
                    }
                }
            }

            throw new ServiceException(ErrorType.TooManyRetries, (int)response.StatusCode, $"Request reached it's max retry count of {retryCount}");
        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="retryCount">Retry times</param>
        private void AddOrUpdateRetryAttempt(HttpRequestMessage request, int retryCount)
        {
            if (UseRetryAfterHeader)
            {
                if (request.Headers.Contains(RETRY_ATTEMPT))
                {
                    request.Headers.Remove(RETRY_ATTEMPT);
                }
                request.Headers.Add(RETRY_ATTEMPT, retryCount.ToString());
            }
        }

        private Task Delay(HttpResponseMessage response, int retryCount, int delay, CancellationToken cancellationToken)
        {
            HttpHeaders headers = response.Headers;
            double delayInSeconds = delay;

            if (UseRetryAfterHeader && headers.TryGetValues(RETRY_AFTER, out IEnumerable<string> values))
            {
                // Can we use the provided retry-after header?
                string retryAfter = values.First();
                if (Int32.TryParse(retryAfter, out int delaySeconds))
                {
                    delayInSeconds = delaySeconds;
                }
            }
            else
            {
                // Custom delay
                if (IncrementalDelay)
                {
                    // Incremental delay, the wait time between each delay exponentially gets bigger
                    double power = Math.Pow(2, retryCount);
                    delayInSeconds = power * delay;
                }
                else
                {
                    // Linear delay
                    delayInSeconds = delay;
                }
            }

            // If the delay goes beyond our max wait time for a delay then cap it
            TimeSpan delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, RetryHandlerBase.MAXDELAY));

            return Task.Delay(delayTimeSpan, cancellationToken);
        }

        private static bool ShouldRetry(HttpResponseMessage response)
        {
            return (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.GatewayTimeout ||
                    response.StatusCode == (HttpStatusCode)429);
        }
    }
}
