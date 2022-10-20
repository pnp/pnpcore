using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Services.Core;
using PnP.Core.Test.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private static readonly string graphRetryError = "{\"error\":{\"code\":\"activityLimitReached\",\"message\":\"Application has been throttled.\"}}";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        [DataRow((HttpStatusCode)429)]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.GatewayTimeout)]
        public async Task SharePointRestRetryTest(HttpStatusCode statusCode)
        {
            MockResponseHandler responseHandler = new MockResponseHandler();

            bool retryEventSent = false;
            EventHub events = new EventHub();
            events.RequestRetry = (retryEvent) =>
            {
                retryEventSent = true;
            };

            SharePointRestRetryHandler retryHandler = new SharePointRestRetryHandler(null, null, events, null)
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
                Assert.IsTrue(retryEventSent);
            }
        }

        [TestMethod]
        [DataRow((HttpStatusCode)429)]
        [DataRow(HttpStatusCode.ServiceUnavailable)]
        [DataRow(HttpStatusCode.GatewayTimeout)]
        public async Task MicrosoftGraphRetryTest(HttpStatusCode statusCode)
        {
            MockResponseHandler responseHandler = new MockResponseHandler();
            bool retryEventSent = false;
            EventHub events = new EventHub();
            events.RequestRetry = (retryEvent) =>
            {
                retryEventSent = true;
            };

            MicrosoftGraphRetryHandler retryHandler = new MicrosoftGraphRetryHandler(null, null, events, null)
            {
                InnerHandler = responseHandler,
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
                Assert.IsTrue(retryEventSent);
            }
        }

        [TestMethod]
        public async Task MicrosoftGraphBatchRetryFirstTest()
        {
            bool firstAttempt = true;

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Set delay to 0 to speed up tests
                context.BatchClient.HttpMicrosoftGraphDelayInSeconds = 0;

                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {
                    if (!firstAttempt)
                    {
                        return input;
                    }

                    if (TestManager.IsMicrosoftGraphMockData(input))
                    {
                        firstAttempt = false;

                        if (TestCommon.Instance.Mocking)
                        {
                            return input;
                        }
                        else
                        {
                            // deserialize the mock data
                            JsonSerializerOptions options = new JsonSerializerOptions()
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            var graphBatchResponses = JsonSerializer.Deserialize<BatchClient.GraphBatchResponses>(input, options);

                            // Update response to contain a 429 for the first request
                            var firstResponse = graphBatchResponses.Responses.FirstOrDefault();
                            if (firstResponse != null)
                            {
                                // Update response to be an error: status, header and body are updated
                                firstResponse.Status = (HttpStatusCode)429;
                                firstResponse.Headers.Add(RETRY_AFTER, "5");
                                firstResponse.Body["body"] = JsonSerializer.Deserialize<JsonElement>(graphRetryError);

                                string rewrittenMockData = JsonSerializer.Serialize(graphBatchResponses, options);
                                return rewrittenMockData;
                            }
                        }
                    }

                    return input;
                };

                var team = await context.Team.GetAsync(p => p.DisplayName, p => p.Channels);

                Assert.IsTrue(team.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(team.DisplayName));
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }

        [TestMethod]
        public async Task MicrosoftGraphBatchRetryLastTest()
        {
            bool firstAttempt = true;

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Set delay to 0 to speed up tests
                context.BatchClient.HttpMicrosoftGraphDelayInSeconds = 0;

                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {
                    if (!firstAttempt)
                    {
                        return input;
                    }

                    if (TestManager.IsMicrosoftGraphMockData(input))
                    {
                        firstAttempt = false;

                        if (TestCommon.Instance.Mocking)
                        {
                            return input;
                        }
                        else
                        {
                            // deserialize the mock data
                            JsonSerializerOptions options = new JsonSerializerOptions()
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            var graphBatchResponses = JsonSerializer.Deserialize<BatchClient.GraphBatchResponses>(input, options);

                            // Update response to contain a 429 for the last request
                            var lastResponse = graphBatchResponses.Responses.LastOrDefault();
                            if (lastResponse != null)
                            {
                                // Update response to be an error: status, header and body are updated
                                lastResponse.Status = (HttpStatusCode)429;
                                lastResponse.Headers.Add(RETRY_AFTER, "5");
                                lastResponse.Body["body"] = JsonSerializer.Deserialize<JsonElement>(graphRetryError);

                                string rewrittenMockData = JsonSerializer.Serialize(graphBatchResponses, options);
                                return rewrittenMockData;
                            }
                        }
                    }

                    return input;
                };

                var team = await context.Team.GetAsync(p => p.DisplayName, p => p.Channels);

                Assert.IsTrue(team.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(team.DisplayName));
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }

        [TestMethod]
        public async Task MicrosoftGraphBatchRetryAllTest()
        {
            bool firstAttempt = true;

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Set delay to 0 to speed up tests
                context.BatchClient.HttpMicrosoftGraphDelayInSeconds = 0;

                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {
                    if (!firstAttempt)
                    {
                        return input;
                    }

                    if (TestManager.IsMicrosoftGraphMockData(input))
                    {
                        firstAttempt = false;

                        if (TestCommon.Instance.Mocking)
                        {
                            return input;
                        }
                        else
                        {
                            // deserialize the mock data
                            JsonSerializerOptions options = new JsonSerializerOptions()
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            var graphBatchResponses = JsonSerializer.Deserialize<BatchClient.GraphBatchResponses>(input, options);

                            // Update response to contain a 429 for the first request
                            foreach (var response in graphBatchResponses.Responses)
                            {
                                // Update response to be an error: status, header and body are updated
                                response.Status = (HttpStatusCode)429;
                                response.Headers.Add(RETRY_AFTER, "5");
                                response.Body["body"] = JsonSerializer.Deserialize<JsonElement>(graphRetryError);
                            }
                            string rewrittenMockData = JsonSerializer.Serialize(graphBatchResponses, options);
                            return rewrittenMockData;
                        }
                    }

                    return input;
                };

                var team = await context.Team.GetAsync(p => p.DisplayName, p => p.Channels);

                Assert.IsTrue(team.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(team.DisplayName));
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }

        [TestMethod]
        public async Task MicrosoftGraphBatchRetryMaxRetriesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Set delay to 0 to speed up tests
                context.BatchClient.HttpMicrosoftGraphDelayInSeconds = 0;

                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {
                    // Keep on returning 429 for the first request in the batch...
                    if (TestManager.IsMicrosoftGraphMockData(input))
                    {
                        if (TestCommon.Instance.Mocking)
                        {
                            return input;
                        }
                        else
                        {
                            // deserialize the mock data
                            JsonSerializerOptions options = new JsonSerializerOptions()
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            };
                            var graphBatchResponses = JsonSerializer.Deserialize<BatchClient.GraphBatchResponses>(input, options);

                            // Update response to contain a 429 for the first request
                            var firstResponse = graphBatchResponses.Responses.FirstOrDefault();
                            if (firstResponse != null)
                            {
                                // Update response to be an error: status, header and body are updated
                                firstResponse.Status = (HttpStatusCode)429;
                                firstResponse.Headers.Add(RETRY_AFTER, "5");
                                firstResponse.Body["body"] = JsonSerializer.Deserialize<JsonElement>(graphRetryError);

                                string rewrittenMockData = JsonSerializer.Serialize(graphBatchResponses, options);
                                return rewrittenMockData;
                            }
                        }
                    }

                    return input;
                };

                bool serviceExceptionThrown = false;
                try
                {
                    var team = await context.Team.GetAsync(p => p.DisplayName, p => p.Channels);
                }
                catch (Exception ex)
                {
                    if (ex is ServiceException)
                    {
                        serviceExceptionThrown = true;
                    }
                }

                Assert.IsTrue(serviceExceptionThrown);
                Assert.IsFalse(context.Team.Requested);
                Assert.IsFalse(context.Team.Channels.Requested);
            }
        }


    }
}
