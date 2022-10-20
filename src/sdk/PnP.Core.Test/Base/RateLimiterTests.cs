using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Services;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class RateLimiterTests
    {
        private const string RATELIMIT_LIMIT = "RateLimit-Limit";
        private const string RATELIMIT_REMAINING = "RateLimit-Remaining";
        private const string RATELIMIT_RESET = "RateLimit-Reset";

        [TestMethod]
        public async Task RequestLimiterTest()
        {            
            MockResponseHandler responseHandler = new MockResponseHandler();

            RateLimiter rateLimiter = new RateLimiter(null, null)
            {
                MinimumCapacityLeft = 10
            };

            SharePointRestRetryHandler retryHandler = new SharePointRestRetryHandler(null, null, null, rateLimiter)
            {
                InnerHandler = responseHandler,
                // Set delay to zero to speed up test case
                DelayInSeconds = 0,
                // Start with default values for the other settings
                MaxRetries = 10,
                UseRetryAfterHeader = false,
                IncrementalDelay = true
            };

            using (HttpMessageInvoker invoker = new HttpMessageInvoker(retryHandler))
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://pnp.com/rocks");

                // Request with rate limit headers
                var firstResponse = new HttpResponseMessage(HttpStatusCode.OK);
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_LIMIT, "1250");
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_REMAINING, "50");
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_RESET, "1");

                // No rate limit headers are set for the second response
                var secondResponse = new HttpResponseMessage(HttpStatusCode.OK);
                responseHandler.SetHttpResponse(firstResponse, secondResponse);

                var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                
                Stopwatch timer = Stopwatch.StartNew();
                await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                timer.Stop();

                // We should have at least waited 1 second because the remaining requests were below the minimum capacity limit
                // sometimes timer resulting results in slightly less than 1000 milliseconds, so we'll use 900 as the minimum
                Assert.IsTrue(timer.ElapsedMilliseconds >= 900);

                // Request with rate limit headers
                firstResponse = new HttpResponseMessage(HttpStatusCode.OK);
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_LIMIT, "1250");
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_REMAINING, "300");
                firstResponse.Headers.TryAddWithoutValidation(RATELIMIT_RESET, "1");

                // No rate limit headers are set for the second response
                secondResponse = new HttpResponseMessage(HttpStatusCode.OK);
                responseHandler.SetHttpResponse(firstResponse, secondResponse);

                response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

                timer = Stopwatch.StartNew();
                await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                timer.Stop();

                Assert.IsTrue(timer.ElapsedMilliseconds < 1000);
            }
        }

    }
}
