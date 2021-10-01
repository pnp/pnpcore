using Microsoft.Extensions.Logging;
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
    internal abstract class RetryHandlerBase : DelegatingHandler
    {
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        internal const int MAXDELAY = 300;

        #region Construction
        public RetryHandlerBase(PnPGlobalSettingsOptions globalSettings)
        {
            GlobalSettings = globalSettings;
        }
        #endregion

        internal PnPGlobalSettingsOptions GlobalSettings { get; private set; }
        internal bool UseRetryAfterHeader { get; set; }
        internal int MaxRetries { get; set; } = 10;
        internal int DelayInSeconds { get; set; } = 3;
        internal bool IncrementalDelay { get; set; } = true;

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (ShouldRetry(response.StatusCode))
            {
                if (GlobalSettings != null && GlobalSettings.Logger != null)
                {
                    GlobalSettings.Logger.LogInformation($"Retrying request {request.RequestUri} due to status code {response.StatusCode}");
                }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing will prevent cloning of the request needed for the retry")]
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                // Drain response content to free responses.
                if (response.Content != null)
                {
#if NET5_0
                    await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);                    
#else
                    await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                }

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, DelayInSeconds, cancellationToken);

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                // do not dispose this request as that breaks the request cloning
                var request = await response.RequestMessage.CloneAsync().ConfigureAwait(false);

                // Increase retryCount and then update Retry-Attempt in request header if needed
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay.ConfigureAwait(false);

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (!ShouldRetry(response.StatusCode))
                {
                    return response;
                }

            }

            throw new ServiceException(ErrorType.TooManyRetries, (int)response.StatusCode,
                string.Format(PnPCoreResources.Exception_ServiceException_MaxRetries, retryCount));
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
                if (int.TryParse(retryAfter, out int delaySeconds))
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
            TimeSpan delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, MAXDELAY));

            return Task.Delay(delayTimeSpan, cancellationToken);
        }

        internal static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return (statusCode == HttpStatusCode.ServiceUnavailable ||
                    statusCode == HttpStatusCode.GatewayTimeout ||
                    statusCode == (HttpStatusCode)429);
        }
    }
}
