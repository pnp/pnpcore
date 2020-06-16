using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{

    /// <summary>
    /// Test class for testing the request retry logic
    /// </summary>
    [TestClass]
    public class RetryTests
    {
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";

        [TestMethod]
        [DataRow((HttpStatusCode)429)]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.GatewayTimeout)]
        public async Task SharePointRestRetryTest(HttpStatusCode statusCode)
        {
            MockResponseHandler responseHandler = new MockResponseHandler();
            SharePointRestRetryHandler retryHandler = new SharePointRestRetryHandler(responseHandler, null)
            {
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

                // Check 1: basic retry
                var retryResponse = new HttpResponseMessage(statusCode);
                var secondResponse = new HttpResponseMessage(HttpStatusCode.OK);
                responseHandler.SetHttpResponse(retryResponse, secondResponse);

                var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

                Assert.AreEqual(response, secondResponse);

                // Check 2: bump into max retries
                retryResponse = new HttpResponseMessage(statusCode);
                secondResponse = new HttpResponseMessage(statusCode);
                responseHandler.SetHttpResponse(retryResponse, secondResponse);

                bool exceptionThrown = false;
                try
                {
                    response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                }
                catch (Exception ex)
                {
                    if (ex is ServiceException)
                    {
                        exceptionThrown = true;
                        var serviceException = ex as ServiceException;
                        Assert.IsTrue((serviceException.Error as ServiceError).HttpResponseCode == (int)statusCode);
                    }
                }
                Assert.IsTrue(exceptionThrown);
            }
        }

        [TestMethod]
        [DataRow((HttpStatusCode)429)]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.GatewayTimeout)]
        public async Task MicrosoftGraphRetryTest(HttpStatusCode statusCode)
        {
            MockResponseHandler responseHandler = new MockResponseHandler();
            MicrosoftGraphRetryHandler retryHandler = new MicrosoftGraphRetryHandler(responseHandler, null)
            {
                // Set delay to zero to speed up test case
                DelayInSeconds = 0,
                // Start with testing without retry header
                UseRetryAfterHeader = false,
                // Start with default values for the other settings
                MaxRetries = 10,
                IncrementalDelay = true
            };

            using (HttpMessageInvoker invoker = new HttpMessageInvoker(retryHandler))
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://pnp.com/rocks");

                // Check 1: basic retry
                var retryResponse = new HttpResponseMessage(statusCode);
                var secondResponse = new HttpResponseMessage(HttpStatusCode.OK);
                responseHandler.SetHttpResponse(retryResponse, secondResponse);

                var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

                Assert.AreEqual(response, secondResponse);

                // Check 2: bump into max retries
                retryResponse = new HttpResponseMessage(statusCode);
                secondResponse = new HttpResponseMessage(statusCode);
                responseHandler.SetHttpResponse(retryResponse, secondResponse);

                bool exceptionThrown = false;
                try
                {
                    response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                }
                catch (Exception ex)
                {
                    if (ex is ServiceException)
                    {
                        exceptionThrown = true;
                        var serviceException = ex as ServiceException;
                        Assert.IsTrue((serviceException.Error as ServiceError).HttpResponseCode == (int)statusCode);
                    }
                }
                Assert.IsTrue(exceptionThrown);

                // Check 3: retry-header testing
                retryHandler.UseRetryAfterHeader = true;
                retryResponse = new HttpResponseMessage(statusCode);
                retryResponse.Headers.TryAddWithoutValidation(RETRY_AFTER, "0");
                secondResponse = new HttpResponseMessage(HttpStatusCode.OK);
                responseHandler.SetHttpResponse(retryResponse, secondResponse);

                response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.AreEqual(response, secondResponse);
                response.RequestMessage.Headers.TryGetValues(RETRY_ATTEMPT, out IEnumerable<string> values);
                Assert.AreEqual(values.First(), "1");
            }
        }

    }
}
