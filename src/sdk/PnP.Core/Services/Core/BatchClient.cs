using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Client that's reponsible for creating and processing batch requests
    /// </summary>
    internal class BatchClient
    {
        // Simple counter used to construct the batch key used for test mocking
        private int testUseCounter = 0;

        // Handles sending telemetry events
        private readonly TelemetryManager telemetryManager;

        // Collection of current batches
        private readonly Dictionary<Guid, Batch> batches = new Dictionary<Guid, Batch>();

    #region Embedded classes

        #region Classes used for Graph batch (de)serialization

        internal class GraphBatchRequest
        {
            public string Id { get; set; }

            public string Method { get; set; }

            public string Url { get; set; }

            public string Body { get; set; }

            public Dictionary<string, string> Headers { get; set; }
        }

        internal class GraphBatchRequests
        {
            public IList<GraphBatchRequest> Requests { get; set; } = new List<GraphBatchRequest>();
        }

        internal class GraphBatchResponse
        {
            public string Id { get; set; }

            public HttpStatusCode Status { get; set; }

            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

            [JsonExtensionData]
            public Dictionary<string, JsonElement> Body { get; set; } = new Dictionary<string, JsonElement>();
        }

        internal class GraphBatchResponses
        {
            public IList<GraphBatchResponse> Responses { get; set; } = new List<GraphBatchResponse>();
        }

        #endregion

        #region Classes used to support REST batch handling
        
        internal class SPORestBatch
        {
            public SPORestBatch(Uri site)
            {
                Site = site;
            }

            public Batch Batch { get; set; }

            public Uri Site { get; set; }
        }

        #endregion

        internal class BatchResultMerge
        {
            internal object Model { get; set; }

            internal Type ModelType { get; set; }

            internal object KeyValue { get; set; }

            internal List<BatchRequest> Requests { get; set; } = new List<BatchRequest>();
        }

    #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">PnP Context</param>
        internal BatchClient(PnPContext context, ISettings settingsClient, TelemetryClient telemetryClient)
        {
            PnPContext = context;
            telemetryManager = new TelemetryManager(telemetryClient, settingsClient);

            HttpMicrosoftGraphMaxRetries = settingsClient.HttpMicrosoftGraphMaxRetries;
            HttpMicrosoftGraphDelayInSeconds = settingsClient.HttpMicrosoftGraphDelayInSeconds;
            HttpMicrosoftGraphUseIncrementalDelay = settingsClient.HttpMicrosoftGraphUseIncrementalDelay;
        }

        /// <summary>
        /// PnP Context
        /// </summary>
        internal PnPContext PnPContext { get; private set; }

        /// <summary>
        /// Max requests in a single Microsoft Graph batch
        /// </summary>
        internal static int MaxRequestsInGraphBatch => 20;

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        internal int HttpMicrosoftGraphMaxRetries { get; set; }

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        internal int HttpMicrosoftGraphDelayInSeconds { get; set; }

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        internal bool HttpMicrosoftGraphUseIncrementalDelay { get; set; }

#if DEBUG
        /// <summary>
        /// Handler that can be used to rewrite mocking files before they're used
        /// </summary>
        internal Func<string, string> MockingFileRewriteHandler { get; set; } = null;
#endif

        /// <summary>
        /// Creates a new batch
        /// </summary>
        /// <returns>Newly created batch</returns>
        internal Batch EnsureBatch()
        {
            return EnsureBatch(Guid.NewGuid());
        }

        /// <summary>
        /// Gets or creates a new batch
        /// </summary>
        /// <param name="id">Id for the batch to get or create</param>
        /// <returns>Ensured batch</returns>
        private Batch EnsureBatch(Guid id)
        {
            if (ContainsBatch(id))
            {
                return batches[id];
            }
            else
            {
                var batch = new Batch(id);
                batches.Add(id, batch);
                return batch;
            }
        }

        /// <summary>
        /// Checks if a given batch is still listed for this batch client
        /// </summary>
        /// <param name="id">Id of the batch to check for</param>
        /// <returns>True if still listed, false otherwise</returns>
        internal bool ContainsBatch(Guid id)
        {
            return batches.ContainsKey(id);
        }

        /// <summary>
        /// Gets a batch via the given id
        /// </summary>
        /// <param name="id">Id of the batch to get</param>
        /// <returns>The found batch, null otherwise</returns>
        internal Batch GetBatchById(Guid id)
        {
            if (ContainsBatch(id))
            {
                return batches[id];
            }

            return null;
        }

        /// <summary>
        /// Executes a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <returns></returns>
        internal async Task ExecuteBatch(Batch batch)
        {
            // Before we start running this new batch let's 
            // clean previous batch execution data
            RemoveProcessedBatches();

            // Batch can be empty, so bail out
            if (batch.Requests.Count == 0)
            {
                return;
            }

            // Remove duplicate batch requests
            DedupBatchRequests(ref batch);

            if (batch.HasMixedApiTypes)
            {
                if (batch.CanFallBackToSPORest)
                {
                    // set the backup api call to be the actual api call for the api calls marked as graph
                    batch.MakeSPORestOnlyBatch();
                    await ExecuteSharePointRestBatchAsync(batch).ConfigureAwait(false);
                }
                else
                {
                    // implement logic to split batch in a rest batch and a graph batch
                    (Batch spoRestBatch, Batch graphBatch) = SplitIntoGraphAndRestBatch(batch);
                    // execute the 2 batches
                    await ExecuteSharePointRestBatchAsync(spoRestBatch).ConfigureAwait(false);
                    await ExecuteMicrosoftGraphBatchAsync(graphBatch).ConfigureAwait(false);
                }
            }
            else
            {
                if (batch.UseGraphBatch)
                {
                    await ExecuteMicrosoftGraphBatchAsync(batch).ConfigureAwait(false);
                }
                else
                {
                    await ExecuteSharePointRestBatchAsync(batch).ConfigureAwait(false);
                }
            }

            // Executing a batch might have resulted in a mismatch between the model and the data in SharePoint:
            // Getting entities can result in duplicate entities (e.g. 2 lists when getting the same list twice in a single batch)
            // Adding entities can result in an entity in the model that does not have the proper key value set (as that value is only retrievable after the add in SharePoint)
            // Deleting entities can result in an entity in the model that also should have been deleted
            MergeBatchResultsWithModel(batch);
        }

        #region Graph batching

        private static List<Batch> MicrosoftGraphBatchSplitting(Batch batch)
        {
            List<Batch> batches = new List<Batch>();

            // Only split if we have more than 20 requests in a single batch
            if (batch.Requests.Count <= MaxRequestsInGraphBatch)
            {
                batches.Add(batch);
                return batches;
            }

            int counter = 0;
            int order = 0;
            Batch currentBatch = new Batch();
            foreach (var request in batch.Requests.OrderBy(p => p.Value.Order))
            {
                currentBatch.Requests.Add(order, request.Value);
                order++;

                counter++;
                if (counter % MaxRequestsInGraphBatch == 0)
                {
                    order = 0;
                    batches.Add(currentBatch);
                    currentBatch = new Batch();
                }
            }

            // Add the last part
            if (currentBatch.Requests.Count > 0)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }

        private async Task ExecuteMicrosoftGraphBatchAsync(Batch batch)
        {
            // Split the provided batch in multiple batches if needed. Possible split reasons are:
            // - too many requests
            var graphBatches = MicrosoftGraphBatchSplitting(batch);

            foreach (var graphBatch in graphBatches)
            {
                if (!await ExecuteMicrosoftGraphBatchRequestAsync(graphBatch).ConfigureAwait(false))
                {
                    // If a request did not succeed and the returned error indicated a retry then try again
                    int retryCount = 0;
                    bool success = false;

                    while (retryCount < HttpMicrosoftGraphMaxRetries)
                    {
                        // Call Delay method to get delay time 
                        Task delay = Delay(retryCount, HttpMicrosoftGraphDelayInSeconds, HttpMicrosoftGraphUseIncrementalDelay);

                        await delay.ConfigureAwait(false);

                        // Increase retryCount 
                        retryCount++;

                        success = await ExecuteMicrosoftGraphBatchRequestAsync(graphBatch).ConfigureAwait(false);
                        if (success)
                        {
                            break;
                        }
                    }

                    if (!success)
                    {
                        // We passed the max retries...time to throw an error
                        throw new ServiceException(ErrorType.TooManyBatchRetries, 0, $"Requests inside a batch reached their max retry count of {retryCount}");
                    }
                }
            }

            // set the original batch to executed
            batch.Executed = true;
        }

        private static Task Delay(int retryCount, int delay, bool incrementalDelay)
        {
            double delayInSeconds;

            // Custom delay
            if (incrementalDelay)
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

            // If the delay goes beyond our max wait time for a delay then cap it
            TimeSpan delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, RetryHandlerBase.MAXDELAY));

            return Task.Delay(delayTimeSpan);
        }

        private async Task<bool> ExecuteMicrosoftGraphBatchRequestAsync(Batch batch)
        {
            string graphEndpoint = DetermineGraphEndpoint(batch);

            (string requestBody, string requestKey) = BuildMicrosoftGraphBatchRequestContent(batch);

            PnPContext.Logger.LogDebug($"{graphEndpoint} : {requestBody}");

            // Make the batch call
            using StringContent content = new StringContent(requestBody);

            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{graphEndpoint}/$batch"))
            {
                // Remove the default Content-Type content header
                if (content.Headers.Contains("Content-Type"))
                {
                    content.Headers.Remove("Content-Type");
                }
                // Add the batch Content-Type header
                content.Headers.Add($"Content-Type", "application/json");

                // Connect content to request
                request.Content = content;

#if DEBUG
                // Test recording
                if (PnPContext.Mode == TestMode.Record && PnPContext.GenerateTestMockingDebugFiles)
                {
                    // Write request
                    TestManager.RecordRequest(PnPContext, requestKey, content.ReadAsStringAsync().Result);
                }

                // If we are not mocking data, or if the mock data is not yet available
                if (PnPContext.Mode != TestMode.Mock)
                {
#endif
                    // Ensure the request contains authentication information
                    await PnPContext.AuthenticationProvider.AuthenticateRequestAsync(PnPConstants.MicrosoftGraphBaseUri, request).ConfigureAwait(false);

                    // Send the request
                    HttpResponseMessage response = await PnPContext.GraphClient.Client.SendAsync(request).ConfigureAwait(false);

                    // Process the request response
                    if (response.IsSuccessStatusCode)
                    {
                        // Get the response string
                        string batchResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

#if DEBUG
                        if (PnPContext.Mode == TestMode.Record)
                        {
                            // Call out to the rewrite handler if that one is connected
                            if (MockingFileRewriteHandler != null)
                            {
                                batchResponse = MockingFileRewriteHandler(batchResponse);
                            }
                            
                            // Write response
                            TestManager.RecordResponse(PnPContext, requestKey, batchResponse);
                        }

#endif

                        var ready = await ProcessGraphRestBatchResponse(batch, batchResponse).ConfigureAwait(false);
                        if (!ready)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Something went wrong...
                        throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, (int)response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
#if DEBUG
                }
                else
                {
                    string batchResponse = TestManager.MockResponse(PnPContext, requestKey);

                    var ready = await ProcessGraphRestBatchResponse(batch, batchResponse).ConfigureAwait(false);
                    if (!ready)
                    {
                        return false;
                    }
                }
#endif
            }

            // Mark batch as executed
            batch.Executed = true;

            // All good so it seams
            return true;
        }

        private string DetermineGraphEndpoint(Batch graphBatch)
        {
            // If a request is in the batch it means we allowed to use Graph beta. To
            // maintain batch integretity we move a complete batch to beta if there's one
            // of the requests in the batch requiring Graph beta.
            if (PnPContext.GraphAlwaysUseBeta)
            {
                return PnPConstants.GraphBetaEndpoint;
            }
            else
            {
                if (graphBatch.Requests.Any(p => p.Value.ApiCall.Type == ApiType.GraphBeta))
                {
                    return PnPConstants.GraphBetaEndpoint;
                }
                else
                {
                    return PnPConstants.GraphV1Endpoint;
                }
            }
        }

        private async Task<bool> ProcessGraphRestBatchResponse(Batch batch, string batchResponse)
        {
            // Deserialize the graph batch response json
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var graphBatchResponses = JsonSerializer.Deserialize<GraphBatchResponses>(batchResponse, options);

            // Was there any request that's eligible for a retry?
            bool retryNeeded = false;
            foreach (var graphBatchResponse in graphBatchResponses.Responses)
            {
                if (int.TryParse(graphBatchResponse.Id, out int id))
                {
                    // Get the original request, requests use 0 based ordering
                    var batchRequest = batch.GetRequest(id - 1);

                    if (RetryHandlerBase.ShouldRetry(graphBatchResponse.Status))
                    {
                        retryNeeded = true;
                        batchRequest.FlagForRetry(graphBatchResponse.Status, graphBatchResponse.Headers);
                    }
                    else
                    {
                        if (graphBatchResponse.Body.TryGetValue("body", out JsonElement bodyContent))
                        {
                            // If one of the requests in the batch failed then throw an exception
                            if (!HttpRequestSucceeded(graphBatchResponse.Status))
                            {
                                throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, (int)graphBatchResponse.Status, bodyContent);
                            }
                            // All was good, connect response to the original request
                            batchRequest.AddResponse(bodyContent.ToString(), graphBatchResponse.Status, graphBatchResponse.Headers);

                            // Commit succesful updates in our model
                            if (batchRequest.Method == HttpMethod.Patch)
                            {
                                if (batchRequest.Model is TransientObject)
                                {
                                    (batchRequest.Model as TransientObject).Commit();
                                }
                            }
                        }
                    }
                }
            }

            if (retryNeeded)
            {
                return false;
            }

            using (var tracer = Tracer.Track(PnPContext.Logger, "ExecuteSharePointGraphBatchAsync-JSONToModel"))
            {
                // A raw request does not require loading of the response into the model
                if (batch.Raw)
                {
                    return true;
                }

                // Map the retrieved JSON to our domain model
                foreach (var batchRequest in batch.Requests.Values)
                {
                    await JsonMappingHelper.MapJsonToModel(batchRequest).ConfigureAwait(false);
                }
            }

            return true;
        }

        private Tuple<string, string> BuildMicrosoftGraphBatchRequestContent(Batch batch)
        {
            // See
            // - https://docs.microsoft.com/en-us/graph/json-batching?context=graph%2Fapi%2F1.0&view=graph-rest-1.0
            
            StringBuilder batchKey = new StringBuilder();

#if DEBUG
            if (PnPContext.Mode != TestMode.Default)
            {
                batchKey.Append($"{testUseCounter}@@");
                testUseCounter++;
            }
#endif

            GraphBatchRequests graphRequests = new GraphBatchRequests();
            Dictionary<int, string> bodiesToReplace = new Dictionary<int, string>();
            int counter = 1;
            foreach (var request in batch.Requests.Values)
            {
                if (request.ExecutionNeeded)
                {
                    var graphRequest = new GraphBatchRequest()
                    {
                        Id = counter.ToString(CultureInfo.InvariantCulture),
                        Method = request.Method.ToString(),
                        Url = request.ApiCall.Request,
                    };

                    if (!string.IsNullOrEmpty(request.ApiCall.JsonBody))
                    {
                        bodiesToReplace.Add(counter, request.ApiCall.JsonBody);
                        graphRequest.Body = $"@#|Body{counter}|#@";
                        graphRequest.Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    };
                    };

                    graphRequests.Requests.Add(graphRequest);

#if DEBUG
                    if (PnPContext.Mode != TestMode.Default)
                    {
                        batchKey.Append($"{request.Method}|{request.ApiCall.Request}@@");
                    }
#endif
                }

                counter++;

                telemetryManager.LogServiceRequest(batch, request, PnPContext);
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string stringContent = JsonSerializer.Serialize(graphRequests, options);

            foreach (var bodyToReplace in bodiesToReplace)
            {
                stringContent = stringContent.Replace($"\"@#|Body{bodyToReplace.Key}|#@\"", bodyToReplace.Value);
            }

            return new Tuple<string, string>(stringContent, batchKey.ToString());
        }

#endregion

#region SharePoint REST batching

        private static List<SPORestBatch> SharePointRestBatchSplitting(Batch batch)
        {
            List<SPORestBatch> batches = new List<SPORestBatch>();
            
            foreach(var request in batch.Requests.OrderBy(p => p.Value.Order))
            {
                // Group batched based up on the site url, a single batch must be scoped to a single web
                Uri site = new Uri(request.Value.ApiCall.Request.Substring(0, request.Value.ApiCall.Request.IndexOf("/_api/", 0)));

                var restBatch = batches.FirstOrDefault(b => b.Site == site);
                if (restBatch == null)
                {
                    // Create a new batch
                    restBatch = new SPORestBatch(site)
                    {
                        Batch = new Batch()
                        {
                            Raw = batch.Raw
                        }
                    };

                    batches.Add(restBatch);
                }

                // Add request to existing batch, we're adding the original request which ensures that once 
                // we update the new batch with results these results are also part of the original batch
                restBatch.Batch.Requests.Add(request.Value.Order, request.Value);
            }

            return batches;
        }
       
        private async Task ExecuteSharePointRestBatchAsync(Batch batch)
        {
            // A batch can only combine requests for the same web, if needed we need to split the incoming batch in batches per web
            var restBatches = SharePointRestBatchSplitting(batch);

            // Execute the batches
            foreach (var restBatch in restBatches)
            {
                (string requestBody, string requestKey) = BuildSharePointRestBatchRequestContent(restBatch.Batch);

                PnPContext.Logger.LogDebug(requestBody);

                // Make the batch call
                using (StringContent content = new StringContent(requestBody))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"{restBatch.Site.ToString().TrimEnd(new char[] { '/' })}/_api/$batch"))
                    {
                        // Remove the default Content-Type content header
                        if (content.Headers.Contains("Content-Type"))
                        {
                            content.Headers.Remove("Content-Type");
                        }
                        // Add the batch Content-Type header
                        content.Headers.Add($"Content-Type", $"multipart/mixed; boundary=batch_{restBatch.Batch.Id}");

                        // Connect content to request
                        request.Content = content;

#if DEBUG
                        // Test recording
                        if (PnPContext.Mode == TestMode.Record && PnPContext.GenerateTestMockingDebugFiles)
                        {
                            // Write request
                            TestManager.RecordRequest(PnPContext, requestKey, content.ReadAsStringAsync().Result);
                        }

                        // If we are not mocking or if there is no mock data
                        if (PnPContext.Mode != TestMode.Mock) // || !TestManager.IsMockAvailable(PnPContext, requestKey))
                        {
#endif
                            // Ensure the request contains authentication information
                            await PnPContext.AuthenticationProvider.AuthenticateRequestAsync(restBatch.Site, request).ConfigureAwait(false);

                            // Send the request
                            HttpResponseMessage response = await PnPContext.RestClient.Client.SendAsync(request).ConfigureAwait(false);

                            // Process the request response
                            if (response.IsSuccessStatusCode)
                            {
                                // Get the response string
                                string batchResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

#if DEBUG
                                if (PnPContext.Mode == TestMode.Record)
                                {
                                    // Call out to the rewrite handler if that one is connected
                                    if (MockingFileRewriteHandler != null)
                                    {
                                        batchResponse = MockingFileRewriteHandler(batchResponse);
                                    }

                                    // Write response
                                    TestManager.RecordResponse(PnPContext, requestKey, batchResponse);
                                }
#endif

                                await ProcessSharePointRestBatchResponse(restBatch, batchResponse).ConfigureAwait(false);
                            }
                            else
                            {
                                // Something went wrong...
                                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, (int)response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                            }
#if DEBUG
                        }
                        else
                        {
                            string batchResponse = TestManager.MockResponse(PnPContext, requestKey);

                            await ProcessSharePointRestBatchResponse(restBatch, batchResponse).ConfigureAwait(false);
                        }

                        // Mark batch as executed
                        restBatch.Batch.Executed = true;
#endif
                    }
                }
        }

            // Mark the original (non split) batch as complete
            batch.Executed = true;
        }

        /// <summary>
        /// Constructs the content of the batch request to be sent
        /// </summary>
        /// <param name="batch">Batch to create the request content for</param>
        /// <returns>StringBuilder holding the created batch request content</returns>
        private Tuple<string, string> BuildSharePointRestBatchRequestContent(Batch batch)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder batchKey = new StringBuilder();

#if DEBUG
            if (PnPContext.Mode != TestMode.Default)
            {
                batchKey.Append($"{testUseCounter}@@");
                testUseCounter++;
            }
#endif

            // See:
            // - https://www.odata.org/documentation/odata-version-3-0/batch-processing/
            // - https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/make-batch-requests-with-the-rest-apis
            // - https://www.andrewconnell.com/blog/part-1-sharepoint-rest-api-batching-understanding-batching-requests
            // - http://connell59.rssing.com/chan-9164895/all_p12.html (scroll to Part 2 - SharePoint REST API Batching - Exploring Batch Requests, Responses and Changesets)

            foreach (var request in batch.Requests.Values)
            {
                if (request.Method == HttpMethod.Get)
                {
                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{HttpMethod.Get.Method} {request.ApiCall.Request} HTTP/1.1");
                    sb.AppendLine("Accept: application/json;odata=verbose");
                    sb.AppendLine();
                }
                else if (request.Method == HttpMethod.Patch || request.Method == HttpMethod.Post)
                {
                    var changesetId = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture);

                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"changeset_{changesetId}\"");
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{request.Method.Method} {request.ApiCall.Request} HTTP/1.1");
                    sb.AppendLine("Accept: application/json;odata=verbose");
                    sb.AppendLine("Content-Type: application/json;odata=verbose");
                    if (!string.IsNullOrEmpty(request.ApiCall.JsonBody))
                    {
                        sb.AppendLine($"Content-Length: {request.ApiCall.JsonBody.Length}");
                    }
                    else
                    {
                        sb.AppendLine($"Content-Length: 0");
                    }
                    sb.AppendLine($"If-Match: *"); // TODO: Here we need the E-Tag or something to specify to use *
                    sb.AppendLine();
                    sb.AppendLine(request.ApiCall.JsonBody);
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}--");
                }
                else if (request.Method == HttpMethod.Delete)
                {
                    var changesetId = Guid.NewGuid().ToString("d", System.Globalization.CultureInfo.InvariantCulture);

                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"changeset_{changesetId}\"");
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{request.Method} {request.ApiCall.Request} HTTP/1.1");
                    sb.AppendLine("Accept: application/json;odata=verbose");
                    sb.AppendLine("Content-Type: application/json;odata=verbose");
                    sb.AppendLine($"IF-MATCH: *"); // TODO: Here we need the E-Tag or something to specify to use *
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}--");
                }
                else
                {
                    // TODO: Figure out what to do ...
                }

#if DEBUG
                if (PnPContext.Mode != TestMode.Default)
                {
                    batchKey.Append($"{request.Method}|{request.ApiCall.Request}@@");
                }
#endif

                telemetryManager.LogServiceRequest(batch, request, PnPContext);
            }

            // Batch closing
            sb.AppendLine($"--batch_{batch.Id}--");
            return new Tuple<string, string>(sb.ToString(), batchKey.ToString());
        }

        /// <summary>
        /// Provides initial processing of a response for a SharePoint REST batch request
        /// </summary>
        /// <param name="restBatch">The batch request to process</param>
        /// <param name="batchResponse">The raw content of the response</param>
        /// <returns></returns>
        private async Task ProcessSharePointRestBatchResponse(SPORestBatch restBatch, string batchResponse)
        {
            using (var tracer = Tracer.Track(PnPContext.Logger, "ExecuteSharePointRestBatchAsync-JSONToModel"))
            {
                // Process the batch response, assign each response to it's request 
                ProcessSharePointRestBatchResponseContent(restBatch.Batch, batchResponse);

                // A raw request does not require loading of the response into the model
                if (restBatch.Batch.Raw)
                {
                    return;
                }

                // Map the retrieved JSON to our domain model
                foreach (var batchRequest in restBatch.Batch.Requests.Values)
                {
                    if (!string.IsNullOrEmpty(batchRequest.ResponseJson))
                    {
                        await JsonMappingHelper
                            .MapJsonToModel(batchRequest).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Process the received batch response and connect the responses to the original requests in this batch
        /// </summary>
        /// <param name="batch">Batch that we're processing</param>
        /// <param name="batchResponse">Batch response received from the server</param>
        private static void ProcessSharePointRestBatchResponseContent(Batch batch, string batchResponse)
        {
            var responseLines = batchResponse.Split(new char[] { '\n' });
            int counter = 0;
            var httpStatusCode = HttpStatusCode.Continue;

            foreach (var line in responseLines)
            {
                if (line.StartsWith("HTTP/1.1 "))
                {
                    if (int.TryParse(line.Substring(9, 3), out int parsedHttpStatusCode))
                    {
                        httpStatusCode = (HttpStatusCode)parsedHttpStatusCode;
                    }
                    else
                    {
                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 0, "Unexpected HTTP result value in returned batch response");
                    }
                }

                if (line.StartsWith("{") || httpStatusCode == HttpStatusCode.NoContent)
                {
                    var batchRequest = batch.GetRequest(counter);

                    if (!HttpRequestSucceeded(httpStatusCode))
                    {
                        // Todo: parsing json in line to provide a more structured error message + add to logging
                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, (int)httpStatusCode, line);
                    }

                    if (httpStatusCode == HttpStatusCode.NoContent)
                    {
                        batchRequest.AddResponse("", httpStatusCode);
                    }
                    else
                    {
                        batchRequest.AddResponse(line, httpStatusCode);
                    }

                    // Commit succesful updates in our model
                    if (batchRequest.Method == HttpMethod.Patch)
                    {
                        if (batchRequest.Model is TransientObject)
                        {
                            (batchRequest.Model as TransientObject).Commit();
                        }
                    }

                    counter++;
                    httpStatusCode = 0;
                }
            }
        }

        #endregion

        /// <summary>
        /// Splits a batch that contains rest and graph calls in two batches, one containing the rest calls, one containing the graph calls
        /// </summary>
        /// <param name="input">Batch to split</param>
        /// <returns>A rest batch and graph batch</returns>
        private static Tuple<Batch, Batch> SplitIntoGraphAndRestBatch(Batch input)
        {
            Batch restBatch = new Batch();
            Batch graphBatch = new Batch();

            foreach(var request in input.Requests)
            {
                var br = request.Value;
                if (br.ApiCall.Type == ApiType.SPORest)
                {
                    // PAOLO: Why not using a method with the BatchRequest object as the input?
                    restBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson);
                }
                else
                {
                    // PAOLO: Why not using a method with the BatchRequest object as the input?
                    graphBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson);
                }
            }

            return new Tuple<Batch, Batch>(restBatch, graphBatch);
        }

        /// <summary>
        /// Deduplicates GET requests in a batch...if for some reason the same request for the same model instance is there twice then it will be removed
        /// </summary>
        /// <param name="batch">Batch to deduplicate</param>
        private static void DedupBatchRequests(ref Batch batch)
        {
            List<Tuple<TransientObject, ApiCall>> queries = new List<Tuple<TransientObject, ApiCall>>();

            // Only dedup get requests, a batch can contain multiple identical add requests
            var requestsToDedup = batch.Requests.Where(p => p.Value.Method == HttpMethod.Get);

            if (requestsToDedup.Any())
            {
                foreach (var request in requestsToDedup.ToList())
                {
                    var query = new Tuple<TransientObject, ApiCall>(request.Value.Model, request.Value.ApiCall);
                    if (!queries.Contains(query))
                    {
                        queries.Add(query);
                    }
                    else
                    {
                        // duplicate request, let's drop it
                        batch.Requests.Remove(request.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Executing a batch might have resulted in a mismatch between the model and the data in SharePoint:
        /// Getting entities can result in duplicate entities (e.g. 2 lists when getting the same list twice in a single batch while using a different query)
        /// Adding entities can result in an entity in the model that does not have the proper key value set (as that value is only retrievable after the add in SharePoint)
        /// Deleting entities can result in an entity in the model that also should have been deleted
        /// </summary>
        /// <param name="batch">Batch to process</param>
        private static void MergeBatchResultsWithModel(Batch batch)
        {
            bool useGraphBatch = batch.UseGraphBatch;

            // Consolidate GET requests
            List<BatchResultMerge> getConsolidation = new List<BatchResultMerge>();
            
            // Step 1: group the requests that have values with the same id (=keyfield) value
            foreach (var request in batch.Requests.Values.Where(p => p.Method == HttpMethod.Get))
            {
                EntityFieldInfo keyField = useGraphBatch ? request.EntityInfo.GraphKeyField : request.EntityInfo.SharePointKeyField;

                // Consolidation can only happen when there's a keyfield set in the model
                if (keyField != null && (request.Model as TransientObject).HasValue(keyField.Name))
                {
                    // Get the value of the model's keyfield property, since we always ask to load the keyfield property this should work
                    var keyFieldValue = GetDynamicProperty(request.Model, keyField.Name);
                    if (keyFieldValue != null)
                    {
                        var modelType = request.Model.GetType();
                        var consolidation = getConsolidation.FirstOrDefault(p => p.ModelType == modelType && p.KeyValue.Equals(keyFieldValue) && !p.Model.Equals(request.Model));
                        if (consolidation == null)
                        {
                            consolidation = new BatchResultMerge()
                            {
                                Model = request.Model,
                                KeyValue = keyFieldValue,
                                ModelType = modelType,
                            };
                            getConsolidation.Add(consolidation);
                        }

                        consolidation.Requests.Add(request);
                    }
                }
            }

            // Step 2: consolidate when we have multiple requests for the same key field
            foreach (var consolidation in getConsolidation.Where(p => p.Requests.Count > 1))
            {
                var firstRequest = consolidation.Requests.OrderBy(p => p.Order).First();

                bool first = true;
                foreach (var request in consolidation.Requests.OrderBy(p => p.Order))
                {
                    if (!first)
                    {
                        // Merge properties/collections
                        (firstRequest.Model as TransientObject).Merge(request.Model as TransientObject);

                        // Mark deleted objects as deleted and remove from their respective parent collection
                        (request.Model as TransientObject).RemoveFromParentCollection();
                    }
                    else
                    {
                        first = false;
                    }
                }
            }

            // Mark deleted objects as deleted and remove from their respective parent collection
            foreach (var request in batch.Requests.Values.Where(p => p.Method == HttpMethod.Delete))
            {
                (request.Model as TransientObject).RemoveFromParentCollection();
            }
        }

        private static bool HttpRequestSucceeded(HttpStatusCode httpStatusCode)
        {
            // See https://restfulapi.net/http-status-codes/
            // For now let's fail all except the 200 range
            return (httpStatusCode >= HttpStatusCode.OK && 
                httpStatusCode < HttpStatusCode.Ambiguous);
        }

        /// <summary>
        /// Remove processed batches to avoid unneeded memory consumption
        /// </summary>
        private void RemoveProcessedBatches()
        {
            // Retrieve the executed batches
            var keysToRemove = (from b in batches
                                where b.Value.Executed
                                select b.Key).ToList();
            
            // And remove them from the current collection
            foreach (var key in keysToRemove)
            {
                batches.Remove(key);
            }
        }

        private static object GetDynamicProperty(object model, string propertyName)
        {
            var pnpObjectType = model.GetType();
            var propertyToGet = pnpObjectType.GetProperty(propertyName);
            return propertyToGet.GetValue(model);
        }
    }
}
