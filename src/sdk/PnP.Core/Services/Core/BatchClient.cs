using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Client that's reponsible for creating and processing batch requests
    /// </summary>
    internal sealed class BatchClient
    {
#if DEBUG
        // Simple counter used to construct the batch key used for test mocking
        private int testUseCounter;
#endif

        // Handles sending telemetry events
        private readonly TelemetryManager telemetryManager;

        // Collection of current batches, ensure thread safety via a concurrent dictionary
        private readonly ConcurrentDictionary<Guid, Batch> batches = new ConcurrentDictionary<Guid, Batch>();

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

        #region Classes used to support CSOM batch handling

        internal class CsomBatch
        {
            public CsomBatch(Uri site)
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
        /// <param name="globalOptions">Global settings to use</param>
        /// <param name="telemetry">Telemetry manager</param>
        internal BatchClient(PnPContext context, PnPGlobalSettingsOptions globalOptions, TelemetryManager telemetry)
        {
            PnPContext = context;
            telemetryManager = telemetry;

            HttpMicrosoftGraphMaxRetries = globalOptions.HttpMicrosoftGraphMaxRetries;
            HttpMicrosoftGraphDelayInSeconds = globalOptions.HttpMicrosoftGraphDelayInSeconds;
            HttpMicrosoftGraphUseIncrementalDelay = globalOptions.HttpMicrosoftGraphUseIncrementalDelay;
        }

        /// <summary>
        /// PnP Context
        /// </summary>
        internal PnPContext PnPContext { get; private set; }

        /// <summary>
        /// Max requests in a single SharePointRest batch
        /// </summary>
        internal static int MaxRequestsInSharePointRestBatch => 100;

        /// <summary>
        /// Max requests in a single Csom batch
        /// </summary>
        internal static int MaxRequestsInCsomBatch => 50;

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
        internal Func<string, string> MockingFileRewriteHandler { get; set; }
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
                if (batches.TryAdd(id, batch))
                {
                    return batch;
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_CannotAddBatch);
                }
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

        internal Batch GetBatchByBatchRequestId(Guid id)
        {
            foreach(var batch in batches.Values)
            {
                if (batch.ContainsRequest(id))
                {
                    return batch;
                }
            }

            throw new ClientException(string.Format(PnPCoreResources.Exception_BatchClient_BatchRequestIdNotFound, id));
        }

        /// <summary>
        /// Executes a given batch
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <returns></returns>
        internal async Task<List<BatchResult>> ExecuteBatch(Batch batch)
        {
            bool anyPageToLoad;

            // Clear all collections
            foreach (var request in batch.Requests.Values)
            {

                if (request.ApiCall.SkipCollectionClearing || request.ApiCall.ExecuteRequestApiCall)
                {
                    continue;
                }

                foreach (var fieldInfo in request.EntityInfo.Fields.Where(f => f.Load && request.Model.HasValue(f.Name)))
                {
                    // Get the collection
                    var property = fieldInfo.PropertyInfo.GetValue(request.Model);
                    var requestableCollection = property as IRequestableCollection;
                    requestableCollection?.Clear();
                }
            }

            // Clear batch result collection
            batch.Results.Clear();

            // Before we start running this new batch let's 
            // clean previous batch execution data
            RemoveProcessedBatches();

            var doneRequests = new List<BatchRequest>();
            do
            {
                // Verify batch requests do not contain unresolved tokens
                CheckForUnresolvedTokens(batch);

                if (batch.HasInteractiveRequest)
                {
                    if (batch.Requests.Count > 1)
                    {
                        throw new ClientException(ErrorType.Unsupported,
                            PnPCoreResources.Exception_Unsupported_InteractiveRequestBatch);
                    }

                    if (batch.Requests.First().Value.ApiCall.Type == ApiType.SPORest)
                    {
                        await ExecuteSharePointRestInteractiveAsync(batch).ConfigureAwait(false);
                    }
                    else if (batch.Requests.First().Value.ApiCall.Type == ApiType.Graph || batch.Requests.First().Value.ApiCall.Type == ApiType.GraphBeta)
                    {
                        await ExecuteMicrosoftGraphInteractiveAsync(batch).ConfigureAwait(false);
                    }
                }
                else
                {
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
                            (Batch spoRestBatch, Batch graphBatch, Batch graphBetaBatch, Batch csomBatch) = SplitIntoBatchesPerApiType(batch, PnPContext.GraphAlwaysUseBeta);

                            // execute the 4 batches
                            await ExecuteSharePointRestBatchAsync(spoRestBatch).ConfigureAwait(false);
                            if (!PnPContext.GraphAlwaysUseBeta)
                            {
                                await ExecuteMicrosoftGraphBatchAsync(graphBatch).ConfigureAwait(false);
                            }
                            await ExecuteMicrosoftGraphBatchAsync(graphBetaBatch).ConfigureAwait(false);
                            await ExecuteCsomBatchAsync(csomBatch).ConfigureAwait(false);

                            // Aggregate batch results from the executed batches
                            batch.Results.AddRange(spoRestBatch.Results);
                            if (!PnPContext.GraphAlwaysUseBeta)
                            {
                                batch.Results.AddRange(graphBatch.Results);
                            }
                            batch.Results.AddRange(graphBetaBatch.Results);
                            batch.Results.AddRange(csomBatch.Results);
                            batch.Executed = true;
                        }
                    }
                    else
                    {
                        if (batch.UseGraphBatch)
                        {
                            await ExecuteMicrosoftGraphBatchAsync(batch).ConfigureAwait(false);
                        }
                        else if (batch.UseCsomBatch)
                        {
                            await ExecuteCsomBatchAsync(batch).ConfigureAwait(false);
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

                // Reset flag
                anyPageToLoad = false;

                // Make a copy of requests which need to load also other pages
                var requestWithLoadPages = batch.Requests.Values.Where(r => r.ApiCall.LoadPages).ToArray();

                // Temporary keep requests into a final list
                doneRequests.AddRange(batch.Requests.Values);
                // Remove current requests in order to create another set
                batch.Requests.Clear();

                foreach (var requestWithPages in requestWithLoadPages)
                {
                    foreach (var fieldInfo in requestWithPages.EntityInfo.Fields.Where(f => f.Load && requestWithPages.Model.HasValue(f.Name)))
                    {
                        var property = fieldInfo.PropertyInfo.GetValue(requestWithPages.Model);
                        // Check if collection supports pagination
                        var typedCollection = property as ISupportPaging;
                        if (typedCollection == null || !typedCollection.CanPage) continue;

                        // Prepare api call
                        IMetadataExtensible metadataExtensible = (IMetadataExtensible)typedCollection;
                        (var nextLink, var nextLinkApiType) =
                            QueryClient.BuildNextPageLink(metadataExtensible);

                        // Clear the MetaData paging links to avoid loading the collection again via paging
                        metadataExtensible.Metadata.Remove(PnPConstants.GraphNextLink);
                        metadataExtensible.Metadata.Remove(PnPConstants.SharePointRestListItemNextLink);

                        // Create a request for the next page
                        batch.Add(
                            requestWithPages.Model,
                            requestWithPages.EntityInfo,
                            HttpMethod.Get,
                            new ApiCall
                            {
                                Type = nextLinkApiType,
                                ReceivingProperty = fieldInfo.Name,
                                Request = nextLink,
                                LoadPages = true
                            },
                            default,
                            requestWithPages.FromJsonCasting,
                            requestWithPages.PostMappingJson,
                            "GetNextPage"
                        );
                        anyPageToLoad = true;
                    }
                }
                // Loop until there is no other pages to load
            } while (anyPageToLoad);

            // Restore all requests done
            batch.Requests.Clear();
            // Rearrange order sequence
            doneRequests.ForEach(r =>
            {
                r.Order = batch.Requests.Count;
                batch.Requests.Add(r.Order, r);
            });

            // If there's an event handler attached then invoke it
            batch.BatchExecuted?.Invoke();

            return batch.Results;
        }

        #region Graph batching

        private static List<Batch> MicrosoftGraphBatchSplitting(Batch batch)
        {
            List<Batch> batches = new List<Batch>();

            int counter = 0;
            int order = 0;
            Batch currentBatch = new Batch()
            {
                ThrowOnError = batch.ThrowOnError
            };

            foreach (var request in batch.Requests.OrderBy(p => p.Value.Order))
            {
                currentBatch.Requests.Add(order, request.Value);
                order++;

                counter++;
                if (counter % MaxRequestsInGraphBatch == 0)
                {
                    order = 0;
                    batches.Add(currentBatch);
                    currentBatch = new Batch()
                    {
                        ThrowOnError = batch.ThrowOnError
                    };
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
            // Due to previous splitting we can see empty batches...
            if (!batch.Requests.Any(p => p.Value.ExecutionNeeded))
            {
                return;
            }

            // Split the provided batch in multiple batches if needed. Possible split reasons are:
            // - too many requests
            var graphBatches = MicrosoftGraphBatchSplitting(batch);

            foreach (var graphBatch in graphBatches)
            {
                // If the code explicitely used a batch method than honor that as otherwise we would have breaking changes
                if (graphBatch.Requests.Count == 1 && graphBatch.Requests.First().Value.ApiCall.AddedViaBatchMethod == false)
                {
                    await ExecuteMicrosoftGraphInteractiveAsync(graphBatch).ConfigureAwait(false);
                }
                else
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
                                // Copy the results collection to the upper batch
                                batch.Results.AddRange(graphBatch.Results);

                                break;
                            }
                        }

                        if (!success)
                        {
                            // We passed the max retries...time to throw an error
                            throw new ServiceException(ErrorType.TooManyBatchRetries, 0,
                                string.Format(PnPCoreResources.Exception_ServiceException_BatchMaxRetries, retryCount));
                        }
                    }
                    else
                    {
                        // Copy the results collection to the upper batch
                        batch.Results.AddRange(graphBatch.Results);
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
                // Add custom PnPContext properties to HttpRequest if needed
                AddHttpRequestMessageProperties(request, batch);

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
                    await PnPContext.AuthenticationProvider.AuthenticateRequestAsync(CloudManager.GetGraphBaseUri(PnPContext), request).ConfigureAwait(false);

                    // Send the request
                    HttpResponseMessage response = await PnPContext.GraphClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, PnPContext.CancellationToken).ConfigureAwait(false);

                    try
                    {
                        // Process the request response
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the response string, using HttpCompletionOption.ResponseHeadersRead and ReadAsStreamAsync to lower the memory
                            // pressure when processing larger responses + performance is better
                            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                using (StreamReader reader = new StreamReader(streamToReadFrom))
                                {
                                    string batchResponse = await reader.ReadToEndAsync().ConfigureAwait(false);

#if DEBUG
                                    if (PnPContext.Mode == TestMode.Record)
                                    {
                                        // Call out to the rewrite handler if that one is connected
                                        if (MockingFileRewriteHandler != null)
                                        {
                                            batchResponse = MockingFileRewriteHandler(batchResponse);
                                        }

                                        // Write response
                                        TestManager.RecordResponse(PnPContext, requestKey, batchResponse, response.IsSuccessStatusCode, (int)response.StatusCode, MicrosoftGraphResposeHeadersToPropagate(response?.Headers));
                                    }
#endif

                                    var ready = await ProcessGraphRestBatchResponse(batch, batchResponse).ConfigureAwait(false);
                                    if (!ready)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Something went wrong...
                            throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, 
                                                                      (int)response.StatusCode, 
                                                                      await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        }
                    }
                    finally
                    {
                        response.Dispose();
                    }
#if DEBUG
                }
                else
                {
                    var testResponse = TestManager.MockResponse(PnPContext, requestKey);
                    string batchResponse = testResponse.Response;

                    // Call out to the rewrite handler if that one is connected
                    if (MockingFileRewriteHandler != null)
                    {
                        batchResponse = MockingFileRewriteHandler(batchResponse);
                    }

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
            var graphBatchResponses = JsonSerializer.Deserialize<GraphBatchResponses>(batchResponse, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);

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
                                if (batch.ThrowOnError)
                                {
                                    throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, (int)graphBatchResponse.Status, bodyContent);
                                }
                                else
                                {
                                    batch.AddBatchResult(batchRequest, graphBatchResponse.Status, bodyContent.ToString(),
                                                         new MicrosoftGraphError(ErrorType.GraphServiceError, (int)graphBatchResponse.Status, bodyContent));
                                }
                            }
                            else
                            {
                                string responseBody = bodyContent.ToString();

                                // Run request modules if they're connected
                                if (batchRequest.RequestModules != null && batchRequest.RequestModules.Count > 0)
                                {
                                    responseBody = ExecuteMicrosoftGraphRequestModulesOnResponse(graphBatchResponse.Status, graphBatchResponse.Headers, batchRequest, responseBody);
                                }

                                // All was good, connect response to the original request
                                batchRequest.AddResponse(responseBody, graphBatchResponse.Status, graphBatchResponse.Headers);

                                // Commit succesful updates in our model
                                if (batchRequest.Method == new HttpMethod("PATCH") || batchRequest.ApiCall.Commit)
                                {
                                    if (batchRequest.Model is TransientObject)
                                    {
                                        batchRequest.Model.Commit();
                                    }
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
                // Map the retrieved JSON to our domain model
                foreach (var batchRequest in batch.Requests.Values)
                {
                    // A raw request does not require loading of the response into the model
                    if (!batchRequest.ApiCall.RawRequest)
                    {
                        await JsonMappingHelper.MapJsonToModel(batchRequest).ConfigureAwait(false);
                    }
                    // Invoke a delegate (if defined) to trigger processing of raw batch requests
                    batchRequest.ApiCall.RawResultsHandler?.Invoke(batchRequest.ResponseJson, batchRequest.ApiCall);
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

                    if (graphRequest.Headers == null)
                    {
                        graphRequest.Headers = new Dictionary<string, string>();
                    }

                    if (request.ApiCall.Headers != null && request.ApiCall.Headers.Count > 0)
                    {
                        foreach (var key in request.ApiCall.Headers.Keys)
                        {
                            string existingKey = graphRequest.Headers.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                            if (string.IsNullOrWhiteSpace(existingKey))
                            {
                                graphRequest.Headers.Add(key, request.ApiCall.Headers[key]);
                            }
                            else
                            {
                                graphRequest.Headers[existingKey] = request.ApiCall.Headers[key];
                            }
                        }
                    }

                    // Run request modules if they're connected
                    if (request.RequestModules != null && request.RequestModules.Count > 0)
                    {
                        string requestUrl = graphRequest.Url;
                        string requestBody = graphRequest.Body;
                        ExecuteMicrosoftGraphRequestModules(request, graphRequest.Headers, ref requestUrl, ref requestBody);
                        graphRequest.Url = requestUrl;
                        graphRequest.Body = requestBody;
                    }

                    graphRequests.Requests.Add(graphRequest);

#if DEBUG
                    if (PnPContext.Mode != TestMode.Default)
                    {
                        batchKey.Append($"{request.Method}|{request.ApiCall.Request}@@");
                    }
#endif
                }

                counter++;

                telemetryManager?.LogServiceRequest(request, PnPContext);
            }

            string stringContent = JsonSerializer.Serialize(graphRequests, PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase);

            foreach (var bodyToReplace in bodiesToReplace)
            {
                stringContent = stringContent.Replace($"\"@#|Body{bodyToReplace.Key}|#@\"", bodyToReplace.Value);
            }

            return new Tuple<string, string>(stringContent, batchKey.ToString());
        }

        private static Dictionary<string, string> MicrosoftGraphResposeHeadersToPropagate(HttpResponseHeaders headers)
        {
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            if (headers != null && headers.Any())
            {
                foreach (var header in headers)
                {
                    responseHeaders[header.Key] = string.Join(",", header.Value);
                }
            }

            return responseHeaders;
        }

        private static void ExecuteMicrosoftGraphRequestModules(BatchRequest request, Dictionary<string, string> headers, ref string requestUrl, ref string requestBody)
        {
            foreach (var module in request.RequestModules.Where(p => p.ExecuteForMicrosoftGraph))
            {
                if (module.RequestHeaderHandler != null)
                {
                    module.RequestHeaderHandler.Invoke(headers);
                }

                if (module.RequestUrlHandler != null)
                {
                    requestUrl = module.RequestUrlHandler.Invoke(requestUrl);
                }

                if (module.RequestBodyHandler != null)
                {
                    requestBody = module.RequestBodyHandler.Invoke(requestBody);
                }
            }
        }

        private static string ExecuteMicrosoftGraphRequestModulesOnResponse(HttpStatusCode httpStatusCode, Dictionary<string, string> responseHeaders, BatchRequest currentBatchRequest, string responseStringContent)
        {
            foreach (var module in currentBatchRequest.RequestModules.Where(p => p.ExecuteForMicrosoftGraph))
            {
                if (module.ResponseHandler != null)
                {
                    responseStringContent = module.ResponseHandler.Invoke(httpStatusCode, responseHeaders, responseStringContent, currentBatchRequest.Id);
                }
            }

            return responseStringContent;
        }

        private async Task ExecuteMicrosoftGraphInteractiveAsync(Batch batch)
        {
            var graphRequest = batch.Requests.First().Value;
            StringContent content = null;
            ByteArrayContent binaryContent = null;

            try
            {
                string graphEndpoint = DetermineGraphEndpoint(batch);
                string requestUrl = graphRequest.ApiCall.Request;
                string requestBody = graphRequest.ApiCall.JsonBody;

                PnPContext.Logger.LogDebug($"{graphEndpoint} : {requestBody}");

                Dictionary<string, string> headers = new Dictionary<string, string>();

                if (graphRequest.ApiCall.Headers != null && graphRequest.ApiCall.Headers.Count > 0)
                {
                    foreach (var key in graphRequest.ApiCall.Headers.Keys)
                    {
                        string existingKey = headers.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrWhiteSpace(existingKey))
                        {
                            headers.Add(key, graphRequest.ApiCall.Headers[key]);
                        }
                        else
                        {
                            headers[existingKey] = graphRequest.ApiCall.Headers[key];
                        }
                    }
                }

                // Run request modules if they're connected
                if (graphRequest.RequestModules != null && graphRequest.RequestModules.Count > 0)
                {
                    ExecuteMicrosoftGraphRequestModules(graphRequest, headers, ref requestUrl, ref requestBody);
                }

                // Make the request
                string graphRequestUrl = $"{CloudManager.GetGraphBaseUrl(PnPContext)}{graphEndpoint}/{requestUrl}"; ;

                using (var request = new HttpRequestMessage(graphRequest.Method, graphRequestUrl))
                {
                    // Add custom PnPContext properties to HttpRequest if needed
                    AddHttpRequestMessageProperties(request, batch);

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                        request.Content = content;

                        // Remove the default Content-Type content header
                        if (content.Headers.Contains("Content-Type"))
                        {
                            content.Headers.Remove("Content-Type");
                        }

                        content.Headers.Add($"Content-Type", $"application/json");
                        PnPContext.Logger.LogDebug(requestBody);
                    }
                    else if (graphRequest.ApiCall.BinaryBody != null)
                    {
                        binaryContent = new ByteArrayContent(graphRequest.ApiCall.BinaryBody);
                        request.Content = binaryContent;
                    }

                    // Add extra headers
                    foreach (var extraHeader in headers)
                    {
                        if (!request.Headers.Contains(extraHeader.Key))
                        {
                            request.Headers.Add(extraHeader.Key, extraHeader.Value);
                        }
                    }

                    telemetryManager?.LogServiceRequest(graphRequest, PnPContext);

#if DEBUG
                    string batchKey = null;
                    if (PnPContext.Mode != TestMode.Default)
                    {
                        batchKey = $"{testUseCounter}@@{request.Method}|{graphRequest.ApiCall.Request}@@";
                        testUseCounter++;
                    }

                    // Test recording
                    if (PnPContext.Mode == TestMode.Record && PnPContext.GenerateTestMockingDebugFiles)
                    {
                        // Write request
                        TestManager.RecordRequest(PnPContext, batchKey, $"{graphRequest.Method}-{graphRequest.ApiCall.Request}-{(graphRequest.ApiCall.JsonBody ?? "")}");
                    }

                    // If we are not mocking data, or if the mock data is not yet available
                    if (PnPContext.Mode != TestMode.Mock)
                    {
#endif
                        // Ensure the request contains authentication information
                        // Do we need a streaming download?
                        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
                        if (graphRequest.ApiCall.Interactive && !graphRequest.ApiCall.StreamResponse)
                        {                            
                            httpCompletionOption = HttpCompletionOption.ResponseContentRead;
                        }

                        await PnPContext.AuthenticationProvider.AuthenticateRequestAsync(CloudManager.GetGraphBaseUri(PnPContext), request).ConfigureAwait(false);

                        // Send the request
                        HttpResponseMessage response = await PnPContext.GraphClient.Client.SendAsync(request, httpCompletionOption, PnPContext.CancellationToken).ConfigureAwait(false);

                        // Process the request response
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the response string, using HttpCompletionOption.ResponseHeadersRead and ReadAsStreamAsync to lower the memory
                            // pressure when processing larger responses + performance is better
                            Stream requestResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

#if DEBUG
                            if (PnPContext.Mode == TestMode.Record && !graphRequest.ApiCall.StreamResponse)
                            {
                                // Call out to the rewrite handler if that one is connected
                                if (MockingFileRewriteHandler != null)
                                {
                                    string responseStringContent = null;
                                    if (response.StatusCode == HttpStatusCode.NoContent)
                                    {
                                        responseStringContent = "";
                                    }
                                    else
                                    {
                                        using (var streamReader = new StreamReader(requestResponseStream))
                                        {
                                            string requestResponse = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                                            responseStringContent = requestResponse;
                                        }
                                    }

                                    var mockedRewrittenFileString = MockingFileRewriteHandler(responseStringContent);
                                    requestResponseStream = mockedRewrittenFileString.AsStream();
                                }

                                // Write response
                                TestManager.RecordResponse(PnPContext, batchKey, ref requestResponseStream, response.IsSuccessStatusCode, (int)response.StatusCode, MicrosoftGraphResposeHeadersToPropagate(response?.Headers));
                            }
#endif
                            await ProcessMicrosoftGraphInteractiveResponse(graphRequest, response.StatusCode, MicrosoftGraphResposeHeadersToPropagate(response?.Headers), requestResponseStream).ConfigureAwait(false);

                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
                            if (PnPContext.Mode == TestMode.Record)
                            {
                                // Write response
                                TestManager.RecordResponse(PnPContext, batchKey, errorContent, response.IsSuccessStatusCode, (int)response.StatusCode, MicrosoftGraphResposeHeadersToPropagate(response?.Headers));
                            }
#endif

                            // Something went wrong...
                            throw new MicrosoftGraphServiceException(
                                ErrorType.GraphServiceError, 
                                (int)response.StatusCode,
                                errorContent);
                        }
#if DEBUG
                    }
                    else
                    {
                        var testResponse = TestManager.MockResponse(PnPContext, batchKey);

                        if (!testResponse.IsSuccessStatusCode)
                        {
                            throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError,
                                   testResponse.StatusCode,
                                   testResponse.Response);
                        }

                        await ProcessMicrosoftGraphInteractiveResponse(graphRequest, (HttpStatusCode)testResponse.StatusCode, testResponse.Headers, testResponse.Response.AsStream()).ConfigureAwait(false);
                    }
#endif
                    // Mark batch as executed
                    batch.Executed = true;
                }
            }
            finally
            {
                if (content != null)
                {
                    content.Dispose();
                }
                if (binaryContent != null)
                {
                    binaryContent.Dispose();
                }
            }
        }

        private static async Task ProcessMicrosoftGraphInteractiveResponse(BatchRequest graphRequest, HttpStatusCode statusCode, Dictionary<string, string> responseHeaders, Stream responseContent)
        {
            // If a binary response content is expected
            if (graphRequest.ApiCall.ExpectBinaryResponse)
            {                
                // Add it to the request and stop processing the response
                graphRequest.AddResponse(responseContent, statusCode);
                return;
            }

            string responseStringContent = null;
            if (statusCode == HttpStatusCode.NoContent)
            {
                responseStringContent = "";
            }
            else
            {
                using (var streamReader = new StreamReader(responseContent))
                {
                    string requestResponse = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    responseStringContent = requestResponse;
                }
            }

            // Run request modules if they're connected
            if (graphRequest.RequestModules != null && graphRequest.RequestModules.Count > 0)
            {
                responseStringContent = ExecuteMicrosoftGraphRequestModulesOnResponse(statusCode, responseHeaders, graphRequest, responseStringContent);
            }

            // Store the response for further processing
            graphRequest.AddResponse(responseStringContent, statusCode, responseHeaders);

            // Commit succesful updates in our model
            if (graphRequest.Method == new HttpMethod("PATCH") || graphRequest.ApiCall.Commit)
            {
                if (graphRequest.Model is TransientObject)
                {
                    graphRequest.Model.Commit();
                }
            }

            // Update our model by processing the "delete"
            if (graphRequest.Method == HttpMethod.Delete)
            {
                if (graphRequest.Model is TransientObject)
                {
                    graphRequest.Model.RemoveFromParentCollection();
                }
            }

            if (!string.IsNullOrEmpty(graphRequest.ResponseJson))
            {
                // A raw request does not require loading of the response into the model
                if (!graphRequest.ApiCall.RawRequest)
                {
                    await JsonMappingHelper.MapJsonToModel(graphRequest).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region SharePoint REST batching

        private static List<SPORestBatch> SharePointRestBatchSplitting(Batch batch)
        {
            List<SPORestBatch> batches = new List<SPORestBatch>();

            foreach (var request in batch.Requests.OrderBy(p => p.Value.Order))
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
                            ThrowOnError = batch.ThrowOnError
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

        private static List<SPORestBatch> SharePointRestBatchSplittingBySize(SPORestBatch batch)
        {
            List<SPORestBatch> batches = new List<SPORestBatch>();

            // Split in multiple batches
            int counter = 0;
            int order = 0;
            SPORestBatch currentBatch = new SPORestBatch(batch.Site)
            {
                Batch = new Batch()
                {
                    ThrowOnError = batch.Batch.ThrowOnError
                }
            };

            foreach (var request in batch.Batch.Requests.OrderBy(p => p.Value.Order))
            {
                currentBatch.Batch.Requests.Add(order, request.Value);
                order++;

                counter++;
                if (counter % MaxRequestsInSharePointRestBatch == 0)
                {
                    order = 0;
                    batches.Add(currentBatch);
                    currentBatch = new SPORestBatch(batch.Site)
                    {
                        Batch = new Batch()
                        {
                            ThrowOnError = batch.Batch.ThrowOnError
                        }
                    };
                }
            }

            // Add the last part
            if (currentBatch.Batch.Requests.Count > 0)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }


        private async Task ExecuteSharePointRestBatchAsync(Batch batch)
        {
            // Due to previous splitting we can see empty batches...
            if (!batch.Requests.Any(p => p.Value.ExecutionNeeded))
            {
                return;
            }

            // A batch can only combine requests for the same web, if needed we need to split the incoming batch in batches per web
            var restBatches = SharePointRestBatchSplitting(batch);

            // Execute the batches
            foreach (var restBatch in restBatches)
            {
                // If there's only one request in the batch then we don't need to use batching to execute the request. 
                // Non batched executions can use network payload compression, hence we skip batching for single requests.
                
                // If the code explicitely used a batch method than honor that as otherwise we would have breaking changes.
                // We detect being in batch via a flag set on the ApiCall to execute
                if (restBatch.Batch.Requests.Count == 1 && restBatch.Batch.Requests.First().Value.ApiCall.AddedViaBatchMethod == false)
                {
                    await ExecuteSharePointRestInteractiveAsync(restBatch.Batch).ConfigureAwait(false);
                }
                else
                {
                    // A batch can contain more than the maximum number of items in a SharePoint batch, so if needed breakup a batch in multiple batches
                    var splitRestBatches = SharePointRestBatchSplittingBySize(restBatch);

                    foreach (var splitRestBatch in splitRestBatches)
                    {

                        (string requestBody, string requestKey) = BuildSharePointRestBatchRequestContent(splitRestBatch.Batch);

                        PnPContext.Logger.LogDebug(requestBody);

                        // Make the batch call
                        using StringContent content = new StringContent(requestBody);
                        using (var request = new HttpRequestMessage(HttpMethod.Post, $"{splitRestBatch.Site.ToString().TrimEnd(new char[] { '/' })}/_api/$batch"))
                        {

                            // Add custom PnPContext properties to HttpRequest if needed
                            AddHttpRequestMessageProperties(request, splitRestBatch.Batch);

                            // Remove the default Content-Type content header
                            if (content.Headers.Contains("Content-Type"))
                            {
                                content.Headers.Remove("Content-Type");
                            }
                            // Add the batch Content-Type header
                            content.Headers.Add($"Content-Type", $"multipart/mixed; boundary=batch_{splitRestBatch.Batch.Id}");

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
                            if (PnPContext.Mode != TestMode.Mock)
                            {
#endif
                                // Process the authentication headers using the currently
                                // configured instance of IAuthenticationProvider
                                await ProcessSharePointRestAuthentication(splitRestBatch.Site, request).ConfigureAwait(false);

                                // Send the request
                                HttpResponseMessage response = await PnPContext.RestClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, PnPContext.CancellationToken).ConfigureAwait(false);

                                try
                                {
                                    // Process the request response
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Get the response string, using HttpCompletionOption.ResponseHeadersRead and ReadAsStreamAsync to lower the memory
                                        // pressure when processing larger responses + performance is better
                                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                        {
                                            using (StreamReader reader = new StreamReader(streamToReadFrom))
                                            {
                                                string batchResponse = await reader.ReadToEndAsync().ConfigureAwait(false);
#if DEBUG
                                                if (PnPContext.Mode == TestMode.Record)
                                                {
                                                    // Call out to the rewrite handler if that one is connected
                                                    if (MockingFileRewriteHandler != null)
                                                    {
                                                        batchResponse = MockingFileRewriteHandler(batchResponse);
                                                    }

                                                    // Write response
                                                    TestManager.RecordResponse(PnPContext, requestKey, batchResponse, response.IsSuccessStatusCode, (int)response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers));
                                                }
#endif

                                                await ProcessSharePointRestBatchResponse(splitRestBatch, batchResponse, SpoRestResposeHeadersToPropagate(response?.Headers)).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Something went wrong...
                                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError,
                                            (int)response.StatusCode,
                                            await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                                            SpoRestResposeHeadersToPropagate(response.Headers));
                                    }
                                }
                                finally
                                {
                                    response.Dispose();
                                }
#if DEBUG
                            }
                            else
                            {
                                var testResponse = TestManager.MockResponse(PnPContext, requestKey);

                                await ProcessSharePointRestBatchResponse(splitRestBatch, testResponse.Response, testResponse.Headers).ConfigureAwait(false);
                            }
#endif

                            // Mark batch as executed
                            splitRestBatch.Batch.Executed = true;

                            // Copy the results collection to the upper batch
                            restBatch.Batch.Results.AddRange(splitRestBatch.Batch.Results);
                        }
                    }

                    // Copy the results collection to the upper batch
                    batch.Results.AddRange(restBatch.Batch.Results);
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
                Dictionary<string, string> headers = new Dictionary<string, string>();
                if (request.Method == HttpMethod.Get)
                {
                    headers.Add("Accept", "application/json;odata=nometadata");
                }
                else if (request.Method == new HttpMethod("PATCH") || request.Method == HttpMethod.Post || request.Method == HttpMethod.Delete)
                {
                    headers.Add("Accept", "application/json;odata=nometadata");
                    headers.Add("Content-Type", "application/json;odata=verbose");
                }

                if (request.ApiCall.Headers != null && request.ApiCall.Headers.Count > 0)
                {
                    foreach (var key in request.ApiCall.Headers.Keys)
                    {
                        string existingKey = headers.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrWhiteSpace(existingKey))
                        {
                            headers.Add(key, request.ApiCall.Headers[key]);
                        }
                        else
                        {
                            headers[existingKey] = request.ApiCall.Headers[key];
                        }
                    }
                }

                string requestUrl = request.ApiCall.Request;
                string requestBody = request.ApiCall.JsonBody;

                // Run request modules if they're connected
                if (request.RequestModules != null && request.RequestModules.Count > 0)
                {
                    ExecuteSpoRestRequestModules(request, headers, ref requestUrl, ref requestBody);
                }

                if (request.Method == HttpMethod.Get)
                {
                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{HttpMethod.Get.Method} {requestUrl} HTTP/1.1");
                    foreach (var header in headers)
                    {
                        sb.AppendLine($"{header.Key}: {header.Value}");
                    }
                    sb.AppendLine();
                }
                else if (request.Method == new HttpMethod("PATCH") || request.Method == HttpMethod.Post)
                {
                    var changesetId = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture);

                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"changeset_{changesetId}\"");
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{request.Method.Method} {requestUrl} HTTP/1.1");
                    foreach (var header in headers)
                    {
                        sb.AppendLine($"{header.Key}: {header.Value}");
                    }
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        sb.AppendLine($"Content-Length: {requestBody.Length}");
                    }
                    else
                    {
                        sb.AppendLine($"Content-Length: 0");
                    }
                    sb.AppendLine($"If-Match: *"); // TODO: Here we need the E-Tag or something to specify to use *
                    sb.AppendLine();
                    sb.AppendLine(requestBody);
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}--");
                }
                else if (request.Method == HttpMethod.Delete)
                {
                    var changesetId = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture);

                    sb.AppendLine($"--batch_{batch.Id}");
                    sb.AppendLine($"Content-Type: multipart/mixed; boundary=\"changeset_{changesetId}\"");
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}");
                    sb.AppendLine("Content-Type: application/http");
                    sb.AppendLine("Content-Transfer-Encoding:binary");
                    sb.AppendLine();
                    sb.AppendLine($"{request.Method} {requestUrl} HTTP/1.1");
                    foreach (var header in headers)
                    {
                        sb.AppendLine($"{header.Key}: {header.Value}");
                    }
                    sb.AppendLine($"IF-MATCH: *"); // TODO: Here we need the E-Tag or something to specify to use *
                    sb.AppendLine();
                    sb.AppendLine($"--changeset_{changesetId}--");
                }

#if DEBUG
                if (PnPContext.Mode != TestMode.Default)
                {
                    batchKey.Append($"{request.Method}|{request.ApiCall.Request}@@");
                }
#endif

                telemetryManager?.LogServiceRequest(request, PnPContext);
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
        /// <param name="headers">Batch request response headers</param>
        /// <returns></returns>
        private async Task ProcessSharePointRestBatchResponse(SPORestBatch restBatch, string batchResponse, Dictionary<string, string> headers)
        {
            using (var tracer = Tracer.Track(PnPContext.Logger, "ExecuteSharePointRestBatchAsync-JSONToModel"))
            {
                // Process the batch response, assign each response to it's request 
                ProcessSharePointRestBatchResponseContent(restBatch.Batch, batchResponse, headers);

                // Map the retrieved JSON to our domain model
                foreach (var batchRequest in restBatch.Batch.Requests.Values)
                {
                    if (!string.IsNullOrEmpty(batchRequest.ResponseJson))
                    {
                        // A raw request does not require loading of the response into the model
                        if (!batchRequest.ApiCall.RawRequest)
                        {
                            await JsonMappingHelper.MapJsonToModel(batchRequest).ConfigureAwait(false);
                        }

                        // Invoke a delegate (if defined) to trigger processing of raw batch requests
                        batchRequest.ApiCall.RawResultsHandler?.Invoke(batchRequest.ResponseJson, batchRequest.ApiCall);
                    }
                }
            }
        }

        /// <summary>
        /// Process the received batch response and connect the responses to the original requests in this batch
        /// </summary>
        /// <param name="batch">Batch that we're processing</param>
        /// <param name="batchResponse">Batch response received from the server</param>
        /// <param name="responseHeadersToPropagate">Batch request response headers</param>
        private static void ProcessSharePointRestBatchResponseContent(Batch batch, string batchResponse, Dictionary<string, string> responseHeadersToPropagate)
        {
#if !NET5_0_OR_GREATER
            var responseLines = batchResponse.Split(new char[] { '\n' });
#endif
            int counter = -1;
            var httpStatusCode = HttpStatusCode.Continue;

            bool responseContentOpen = false;
            bool collectHeaders = false;            

            Dictionary<string, string> responseHeaders = new Dictionary<string, string>(responseHeadersToPropagate);
            StringBuilder responseContent = new StringBuilder();
#if NET5_0_OR_GREATER
            foreach (ReadOnlySpan<char> line in batchResponse.SplitLines())
#else
            foreach (var line in responseLines)
#endif
            {
                // Signals the start/end of a response
                // --batchresponse_6ed85e4b-869f-428e-90c9-19038f964718
                if (line.StartsWith("--batchresponse_"))
                {
                    // Reponse was closed, let's store the result 
                    if (responseContentOpen)
                    {
                        // responses are in the same order as the request, so use a counter based system
                        BatchRequest currentBatchRequest = batch.GetRequest(counter);

                        if (!HttpRequestSucceeded(httpStatusCode))
                        {
                            if (batch.ThrowOnError)
                            {
                                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, (int)httpStatusCode, responseContent.ToString(), responseHeaders);
                            }
                            else
                            {
                                // Store the batch error
                                batch.AddBatchResult(currentBatchRequest,
                                                     httpStatusCode,
                                                     responseContent.ToString(),
                                                     new SharePointRestError(ErrorType.SharePointRestServiceError, (int)httpStatusCode, responseContent.ToString(), responseHeaders));
                            }
                        }
                        else
                        {
                            string responseStringContent = null;
                            if (httpStatusCode != HttpStatusCode.NoContent)
                            {
                                responseStringContent = responseContent.ToString();
                            }

                            // Run request modules if they're connected
                            if (currentBatchRequest.RequestModules != null && currentBatchRequest.RequestModules.Count > 0)
                            {
                                responseStringContent = ExecuteSpoRestRequestModulesOnResponse(httpStatusCode, responseHeaders, currentBatchRequest, responseStringContent);
                            }

                            if (httpStatusCode == HttpStatusCode.NoContent)
                            {
                                currentBatchRequest.AddResponse("", httpStatusCode, responseHeaders);
                            }
                            else
                            {
                                currentBatchRequest.AddResponse(responseStringContent, httpStatusCode, responseHeaders);
                            }

                            // Commit succesful updates in our model
                            if (currentBatchRequest.Method == new HttpMethod("PATCH") || currentBatchRequest.ApiCall.Commit)
                            {
                                if (currentBatchRequest.Model is TransientObject)
                                {
                                    currentBatchRequest.Model.Commit();
                                }
                            }
                        }

                        httpStatusCode = 0;
                        responseContentOpen = false;
                        responseContent = new StringBuilder();
                    }

                    collectHeaders = false;
                    responseHeaders = new Dictionary<string, string>(responseHeadersToPropagate);

                    counter++;
                }
                // Response status code
                else if (line.StartsWith("HTTP/1.1 "))
                {
                    // HTTP/1.1 200 OK
#if NET5_0_OR_GREATER
                    if (int.TryParse(line.Slice(9, 3), out int parsedHttpStatusCode))
#else
                    if (int.TryParse(line.Substring(9, 3), out int parsedHttpStatusCode))
#endif
                    {
                        httpStatusCode = (HttpStatusCode)parsedHttpStatusCode;
                        collectHeaders = true;
                    }
                    else
                    {
                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 0, PnPCoreResources.Exception_SharePointRest_UnexpectedResult);
                    }
                }
                // First real content returned, lines before are ignored
                else if ((line.StartsWith("{") || httpStatusCode == HttpStatusCode.NoContent) && !responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
#if NET5_0_OR_GREATER
                    responseContent.Append(line).AppendLine();
#else
                    responseContent.AppendLine(line.TrimEnd('\r'));
#endif
                    responseContentOpen = true;
                }
                // More content is being returned, so let's append it
                else if (responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
#if NET5_0_OR_GREATER
                    responseContent.Append(line).AppendLine();
#else
                    responseContent.AppendLine(line.TrimEnd('\r'));
#endif
                }
                // Response headers e.g. CONTENT-TYPE: application/json;odata=verbose;charset=utf-8
                else if (collectHeaders)
                {
#if NET5_0_OR_GREATER
                    HeaderSplit(line, responseHeaders);
#else
                    HeaderSplit(line.AsSpan(), responseHeaders);
#endif
                }
            }
        }

        private static Dictionary<string, string> SpoRestResposeHeadersToPropagate(HttpResponseHeaders headers)
        {
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            if (headers != null && headers.Any())
            {
                if (headers.TryGetValues(PnPConstants.SPRequestGuidHeader, out IEnumerable<string> spRequestGuidHeader))
                {
                    responseHeaders.Add(PnPConstants.SPRequestGuidHeader, string.Join(",", spRequestGuidHeader));
                }

                if (headers.TryGetValues(PnPConstants.SPClientServiceRequestDurationHeader, out IEnumerable<string> spClientServiceRequestDurationHeader))
                {
                    responseHeaders.Add(PnPConstants.SPClientServiceRequestDurationHeader, string.Join(",", spClientServiceRequestDurationHeader));
                }

                if (headers.TryGetValues(PnPConstants.XSharePointHealthScoreHeader, out IEnumerable<string> xSharePointHealthScoreHeader))
                {
                    responseHeaders.Add(PnPConstants.XSharePointHealthScoreHeader, string.Join(",", xSharePointHealthScoreHeader));
                }

                if (headers.TryGetValues(PnPConstants.XSPServerStateHeader, out IEnumerable<string> xSPServerStateHeader))
                {
                    responseHeaders.Add(PnPConstants.XSPServerStateHeader, string.Join(",", xSPServerStateHeader));
                }
            }

            return responseHeaders;
        }

        private static void ExecuteSpoRestRequestModules(BatchRequest request, Dictionary<string, string> headers, ref string requestUrl, ref string requestBody)
        {
            foreach (var module in request.RequestModules.Where(p => p.ExecuteForSpoRest))
            {
                if (module.RequestHeaderHandler != null)
                {
                    module.RequestHeaderHandler.Invoke(headers);
                }

                if (module.RequestUrlHandler != null)
                {
                    requestUrl = module.RequestUrlHandler.Invoke(requestUrl);
                }

                if (module.RequestBodyHandler != null)
                {
                    requestBody = module.RequestBodyHandler.Invoke(requestBody);
                }
            }
        }

        private static string ExecuteSpoRestRequestModulesOnResponse(HttpStatusCode httpStatusCode, Dictionary<string, string> responseHeaders, BatchRequest currentBatchRequest, string responseStringContent)
        {
            foreach (var module in currentBatchRequest.RequestModules.Where(p => p.ExecuteForSpoRest))
            {
                if (module.ResponseHandler != null)
                {
                    responseStringContent = module.ResponseHandler.Invoke(httpStatusCode, responseHeaders, responseStringContent, currentBatchRequest.Id);
                }
            }

            return responseStringContent;
        }

        private static void HeaderSplit(ReadOnlySpan<char> input, Dictionary<string, string> headers)
        {
            if (!input.IsNullOrEmpty())
            {
                var left = input.LeftPart(':');
                var right = input.RightPart(':');

                if (!left.IsNullOrEmpty() && !right.IsNullOrEmpty())
                {
                    if (!headers.ContainsKey(left.Trim().ToString()))
                    {
                        headers.Add(left.Trim().ToString(), right.Trim().ToString());
                    }
                }
            }
        }

#endregion

#region SharePoint REST interactive calls

        private async Task ExecuteSharePointRestInteractiveAsync(Batch batch)
        {
            var restRequest = batch.Requests.First().Value;
            StringContent content = null;
            ByteArrayContent binaryContent = null;

            try
            {
                string requestUrl = restRequest.ApiCall.Request;
                string requestBody = restRequest.ApiCall.JsonBody;
                Dictionary<string, string> headers = new Dictionary<string, string>();

                if (restRequest.ApiCall.Headers != null && restRequest.ApiCall.Headers.Count > 0)
                {
                    foreach (var key in restRequest.ApiCall.Headers.Keys)
                    {
                        string existingKey = headers.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        if (string.IsNullOrWhiteSpace(existingKey))
                        {
                            headers.Add(key, restRequest.ApiCall.Headers[key]);
                        }
                        else
                        {
                            headers[existingKey] = restRequest.ApiCall.Headers[key];
                        }
                    }
                }

                // Run request modules if they're connected
                if (restRequest.RequestModules != null && restRequest.RequestModules.Count > 0)
                {
                    ExecuteSpoRestRequestModules(restRequest, headers, ref requestUrl, ref requestBody);
                }

                using (var request = new HttpRequestMessage(restRequest.Method, requestUrl))
                {
                    PnPContext.Logger.LogDebug($"{restRequest.Method} {restRequest.ApiCall.Request}");

                    // Add custom PnPContext properties to HttpRequest if needed
                    AddHttpRequestMessageProperties(request, batch);

                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                        request.Content = content;

                        // Remove the default Content-Type content header
                        if (content.Headers.Contains("Content-Type"))
                        {
                            content.Headers.Remove("Content-Type");
                        }
                        // Add the batch Content-Type header
                        content.Headers.Add($"Content-Type", $"application/json;odata=verbose");
                        PnPContext.Logger.LogDebug(requestBody);
                    }
                    else if (restRequest.ApiCall.BinaryBody != null)
                    {
                        binaryContent = new ByteArrayContent(restRequest.ApiCall.BinaryBody);
                        request.Content = binaryContent;
                    }

                    if (restRequest.ApiCall.ExpectBinaryResponse)
                    {
                        // Add the batch binarystringresponsebody header
                        request.Headers.Add($"binarystringresponsebody", "true");
                    }

                    if (request.Method == HttpMethod.Delete || request.Method == HttpMethod.Post || request.Method == new HttpMethod("PATCH"))
                    {
                        request.Headers.Add("If-Match", "*");
                    }

                    // Add extra headers
                    foreach (var extraHeader in headers)
                    {
                        if (extraHeader.Key == "Content-Type")
                        {
                            // Remove the default Content-Type content header
                            if (request.Content.Headers.Contains("Content-Type"))
                            {
                                request.Content.Headers.Remove("Content-Type");
                            }
                            request.Content.Headers.Add(extraHeader.Key, extraHeader.Value);
                        }
                        else
                        {
                            if (!request.Headers.Contains(extraHeader.Key))
                            {
                                request.Headers.Add(extraHeader.Key, extraHeader.Value);
                            }
                            else
                            {
                                request.Headers.Remove(extraHeader.Key);
                                request.Headers.Add(extraHeader.Key, extraHeader.Value);
                            }
                        }
                    }

                    telemetryManager?.LogServiceRequest(restRequest, PnPContext);

#if DEBUG
                    string batchKey = null;
                    if (PnPContext.Mode != TestMode.Default)
                    {
                        batchKey = $"{testUseCounter}@@{request.Method}|{restRequest.ApiCall.Request}@@";
                        testUseCounter++;
                    }

                    // Test recording
                    if (PnPContext.Mode == TestMode.Record && PnPContext.GenerateTestMockingDebugFiles)
                    {
                        // Write request
                        TestManager.RecordRequest(PnPContext, batchKey, $"{restRequest.Method}-{restRequest.ApiCall.Request}-{(restRequest.ApiCall.JsonBody ?? "")}");
                    }

                    // If we are not mocking or if there is no mock data
                    if (PnPContext.Mode != TestMode.Mock)
                    {
#endif
                        // Ensure the request contains authentication information
                        Uri site;
                        if (requestUrl.IndexOf("/_api/", 0) > -1)
                        {
                            site = new Uri(requestUrl.Substring(0, restRequest.ApiCall.Request.IndexOf("/_api/", 0)));
                        }
                        else
                        {
                            // We need this as we use _layouts/15/download.aspx to download files 
                            site = new Uri(requestUrl.Substring(0, restRequest.ApiCall.Request.IndexOf("/_layouts/", 0)));
                        }

                        // Do we need a streaming download?
                        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead;
                        if (restRequest.ApiCall.StreamResponse)
                        {
                            httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
                        }

                        // Process the authentication headers using the currently
                        // configured instance of IAuthenticationProvider
                        await ProcessSharePointRestAuthentication(site, request).ConfigureAwait(false);

                        // Send the request
                        HttpResponseMessage response = await PnPContext.RestClient.Client.SendAsync(request, httpCompletionOption, PnPContext.CancellationToken).ConfigureAwait(false);

                        // Process the request response
                        if (response.IsSuccessStatusCode)
                        {
                            // Get the response string
                            Stream requestResponseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

#if DEBUG
                            if (PnPContext.Mode == TestMode.Record && !restRequest.ApiCall.StreamResponse)
                            {
                                // Call out to the rewrite handler if that one is connected
                                if (MockingFileRewriteHandler != null)
                                {
                                    string responseStringContent = null;
                                    if (response.StatusCode == HttpStatusCode.NoContent)
                                    {
                                        responseStringContent = "";
                                    }
                                    else
                                    {
                                        using (var streamReader = new StreamReader(requestResponseStream))
                                        {
                                            string requestResponse = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                                            responseStringContent = requestResponse;
                                        }
                                    }

                                    var mockedRewrittenFileString = MockingFileRewriteHandler(responseStringContent);
                                    requestResponseStream = mockedRewrittenFileString.AsStream();
                                }

                                // Write response
                                TestManager.RecordResponse(PnPContext, batchKey, ref requestResponseStream, response.IsSuccessStatusCode, (int)response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers));
                            }
#endif

                            await ProcessSharePointRestInteractiveResponse(restRequest, response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers), requestResponseStream).ConfigureAwait(false);

                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
                            if (PnPContext.Mode == TestMode.Record)
                            {
                                // Write response
                                TestManager.RecordResponse(PnPContext, batchKey, errorContent, response.IsSuccessStatusCode, (int)response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers));
                            }
#endif
                            // Something went wrong...
                            throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, 
                                                                        (int)response.StatusCode,
                                                                        errorContent,
                                                                        SpoRestResposeHeadersToPropagate(response.Headers));
                        }
#if DEBUG
                    }
                    else
                    {
                        var testResponse = TestManager.MockResponse(PnPContext, batchKey);

                        if (!testResponse.IsSuccessStatusCode)
                        {
                            throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError,
                                   testResponse.StatusCode,
                                   testResponse.Response,
                                   testResponse.Headers);
                        }

                        await ProcessSharePointRestInteractiveResponse(restRequest, (HttpStatusCode)testResponse.StatusCode, testResponse.Headers, testResponse.Response.AsStream()).ConfigureAwait(false);
                    }
#endif

                    // Mark batch as executed
                    batch.Executed = true;
                }
            }
            finally
            {
                if (content != null)
                {
                    content.Dispose();
                }
                if (binaryContent != null)
                {
                    binaryContent.Dispose();
                }
            }
        }

        private static async Task ProcessSharePointRestInteractiveResponse(BatchRequest restRequest, HttpStatusCode statusCode, Dictionary<string, string> responseHeaders, Stream responseContent)
        {
            // If a binary response content is expected
            if (restRequest.ApiCall.ExpectBinaryResponse)
            {
                // Add it to the request and stop processing the response
                restRequest.AddResponse(responseContent, statusCode);
                return;
            }

            string responseStringContent = null;
            if (statusCode == HttpStatusCode.NoContent)
            {
                responseStringContent = "";
            }
            else
            {
                using (var streamReader = new StreamReader(responseContent))
                {
                    responseContent.Seek(0, SeekOrigin.Begin);
                    string requestResponse = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    responseStringContent = requestResponse;
                }
            }

            // Run request modules if they're connected
            if (restRequest.RequestModules != null && restRequest.RequestModules.Count > 0)
            {
                responseStringContent = ExecuteSpoRestRequestModulesOnResponse(statusCode, responseHeaders, restRequest, responseStringContent);
            }

            // Store the response for further processing
            restRequest.AddResponse(responseStringContent, statusCode, responseHeaders);

            // Commit succesful updates in our model
            if (restRequest.Method == new HttpMethod("PATCH") || restRequest.ApiCall.Commit)
            {
                if (restRequest.Model is TransientObject)
                {
                    restRequest.Model.Commit();
                }
            }

            // Update our model by processing the "delete"
            if (restRequest.Method == HttpMethod.Delete)
            {
                if (restRequest.Model is TransientObject)
                {
                    restRequest.Model.RemoveFromParentCollection();
                }
            }

            if (!string.IsNullOrEmpty(restRequest.ResponseJson))
            {
                // A raw request does not require loading of the response into the model
                if (!restRequest.ApiCall.RawRequest)
                {
                    await JsonMappingHelper.MapJsonToModel(restRequest).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessSharePointRestAuthentication(Uri site, HttpRequestMessage request)
        {
            // If the AuthenticationProvider is a legacy one
            var legacyAuthenticationProvider = PnPContext.AuthenticationProvider as ILegacyAuthenticationProvider;

            if (legacyAuthenticationProvider != null && legacyAuthenticationProvider.RequiresCookieAuthentication)
            {
                // Let's set the cookie header for legacy authentication
                request.Headers.Add("Cookie", legacyAuthenticationProvider.GetCookieHeader(site));
                if (request.Method != HttpMethod.Get)
                {
                    request.Headers.Add("X-RequestDigest", legacyAuthenticationProvider.GetRequestDigest());
                }
            }
            else
            {
                // Ensure the request contains authentication information
                await PnPContext.AuthenticationProvider.AuthenticateRequestAsync(site, request).ConfigureAwait(false);
            }
        }

#endregion

#region CSOM batching
        /// <summary>
        /// Execute a batch with CSOM requests.
        /// See https://docs.microsoft.com/en-us/openspecs/sharepoint_protocols/ms-csom/fd645da2-fa28-4daa-b3cd-8f4e506df117 for the CSOM protocol specs
        /// </summary>
        /// <param name="batch">Batch to execute</param>
        /// <returns></returns>
        private async Task ExecuteCsomBatchAsync(Batch batch)
        {
            // Due to previous splitting we can see empty batches...
            if (!batch.Requests.Any(p => p.Value.ExecutionNeeded))
            {
                return;
            }

            // A batch can only combine requests for the same web, if needed we need to split the incoming batch in batches per web
            var csomBatches = CsomBatchSplitting(batch);

            // Execute the batches
            foreach (var csomBatchBeforeSplit in csomBatches)
            {
                // A batch can contain more than the maximum number of items in a Csom batch, so if needed breakup a batch in multiple batches
                var splitCsomBatches = CsomBatchSplittingBySize(csomBatchBeforeSplit);

                foreach (var csomBatch in splitCsomBatches)
                {
                    // Each csom batch only contains one request
                    CSOMApiCallBuilder csomAPICallBuilder = new CSOMApiCallBuilder();
                    string requestBody = "";
                    foreach (var csomAPICall in csomBatch.Batch.Requests.Values)
                    {
                        foreach (IRequest<object> request in csomAPICall.ApiCall.CSOMRequests)
                        {
                            csomAPICallBuilder.AddRequest(request);
                        }

                        telemetryManager?.LogServiceRequest(csomAPICall, PnPContext);
                    }
                    requestBody = csomAPICallBuilder.SerializeCSOMRequests();

#if DEBUG
                    string requestKey = "";
                    if (PnPContext.Mode != TestMode.Default)
                    {
                        requestKey = $"{testUseCounter}@@GET|dummy";
                        testUseCounter++;
                    }
#endif

                    PnPContext.Logger.LogDebug(requestBody);

                    // Make the batch call
                    using (StringContent content = new StringContent(requestBody))
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Post, $"{csomBatch.Site.ToString().TrimEnd(new char[] { '/' })}/_vti_bin/client.svc/ProcessQuery"))
                        {

                            // Add custom PnPContext properties to HttpRequest if needed
                            AddHttpRequestMessageProperties(request, csomBatch.Batch);

                            // Remove the default Content-Type content header
                            if (content.Headers.Contains("Content-Type"))
                            {
                                content.Headers.Remove("Content-Type");
                            }
                            // Add the batch Content-Type header
                            content.Headers.Add($"Content-Type", $"text/xml");

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
                            if (PnPContext.Mode != TestMode.Mock)
                            {
#endif
                                // Process the authentication headers using the currently
                                // configured instance of IAuthenticationProvider
                                await ProcessSharePointRestAuthentication(csomBatch.Site, request).ConfigureAwait(false);

                                // Send the request
                                HttpResponseMessage response = await PnPContext.RestClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, PnPContext.CancellationToken).ConfigureAwait(false);

                                try
                                {
                                    // Process the request response
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Get the response string, using HttpCompletionOption.ResponseHeadersRead and ReadAsStreamAsync to lower the memory
                                        // pressure when processing larger responses + performance is better
                                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                        {
                                            using (StreamReader reader = new StreamReader(streamToReadFrom))
                                            {
                                                string batchResponse = await reader.ReadToEndAsync().ConfigureAwait(false);

#if DEBUG
                                                if (PnPContext.Mode == TestMode.Record)
                                                {
                                                    // Call out to the rewrite handler if that one is connected
                                                    if (MockingFileRewriteHandler != null)
                                                    {
                                                        batchResponse = MockingFileRewriteHandler(batchResponse);
                                                    }

                                                    // Write response
                                                    TestManager.RecordResponse(PnPContext, requestKey, batchResponse, response.IsSuccessStatusCode, (int)response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers));
                                                }
#endif

                                                ProcessCsomBatchResponse(csomBatch, batchResponse, response.StatusCode);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
                                        if (PnPContext.Mode == TestMode.Record)
                                        {
                                            // Write response
                                            TestManager.RecordResponse(PnPContext, requestKey, errorContent, response.IsSuccessStatusCode, (int)response.StatusCode, SpoRestResposeHeadersToPropagate(response?.Headers));
                                        }
#endif
                                        // Something went wrong...
                                        throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError,
                                                                                    (int)response.StatusCode,
                                                                                    errorContent,
                                                                                    SpoRestResposeHeadersToPropagate(response.Headers));
                                    }
                                }
                                finally
                                {
                                    response.Dispose();
                                }
#if DEBUG
                            }
                            else
                            {
                                var testResponse = TestManager.MockResponse(PnPContext, requestKey);

                                if (!testResponse.IsSuccessStatusCode)
                                {
                                    throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError,
                                           testResponse.StatusCode,
                                           testResponse.Response,
                                           testResponse.Headers);
                                }

                                ProcessCsomBatchResponse(csomBatch, testResponse.Response, (HttpStatusCode)testResponse.StatusCode);
                            }
#endif

                            // Mark batch as executed
                            csomBatch.Batch.Executed = true;

                            // Copy the results collection to the upper batch
                            csomBatchBeforeSplit.Batch.Results.AddRange(csomBatch.Batch.Results);
                        }
                    }
                }

                // Copy the results collection to the upper batch
                batch.Results.AddRange(csomBatchBeforeSplit.Batch.Results);
            }

            // Mark the original (non split) batch as complete
            batch.Executed = true;
        }

        /// <summary>
        /// Provides initial processing of a response for a Csom batch request
        /// </summary>
        /// <param name="csomBatch">The batch request to process</param>
        /// <param name="batchResponse">The raw content of the response</param>
        /// <param name="statusCode">The Http status code of the request</param>
        /// <returns></returns>
        private void ProcessCsomBatchResponse(CsomBatch csomBatch, string batchResponse, HttpStatusCode statusCode)
        {
            using (var tracer = Tracer.Track(PnPContext.Logger, "ExecuteCsomBatchAsync-JSONToModel"))
            {
                if (!string.IsNullOrEmpty(batchResponse))
                {
                    var responses = CsomHelper.ParseResponse(batchResponse);

                    // The first response contains possible error status info
                    var firstElement = responses[0];
                    if (!firstElement.Equals(default))
                    {
                        var errorInfo = firstElement.GetProperty("ErrorInfo");
                        if (errorInfo.ValueKind != JsonValueKind.Null)
                        {
                            if (csomBatch.Batch.ThrowOnError)
                            {
                                // Oops, something went wrong
                                throw new CsomServiceException(ErrorType.CsomServiceError, (int)statusCode, firstElement);
                            }
                            else
                            {
                                // Link the returned error to all the requests
                                foreach (var request in csomBatch.Batch.Requests)
                                {
                                    csomBatch.Batch.AddBatchResult(request.Value,
                                                                  statusCode, firstElement.ToString(),
                                                                  new CsomError(ErrorType.CsomServiceError, (int)statusCode, firstElement));
                                }
                            }
                        }
                    }

                    // No error, so let's return the results
                    foreach (var request in csomBatch.Batch.Requests)
                    {
                        // Call the logic that processes the csom response
                        request.Value.ApiCall.CSOMRequests[0].ProcessResponse(batchResponse);

                        // Commit succesful updates in our model
                        if (request.Value.ApiCall.Commit)
                        {
                            if (request.Value.Model is TransientObject)
                            {
                                request.Value.Model.Commit();
                            }
                        }

                        // Execute post mapping handler (even though, there is no actual mapping in this case)
                        request.Value.PostMappingJson?.Invoke(batchResponse);
                    }
                }
            }
        }

        private static List<CsomBatch> CsomBatchSplitting(Batch batch)
        {
            List<CsomBatch> batches = new List<CsomBatch>();

            foreach (var request in batch.Requests.OrderBy(p => p.Value.Order))
            {
                // Each request is a single batch
                Uri site = new Uri(request.Value.ApiCall.Request);

                var csomBatch = batches.FirstOrDefault(b => b.Site == site);
                if (csomBatch == null)
                {
                    // Create a new batch
                    csomBatch = new CsomBatch(site)
                    {
                        Batch = new Batch()
                        {
                            ThrowOnError = batch.ThrowOnError
                        }
                    };

                    batches.Add(csomBatch);
                }

                // Add request to existing batch, we're adding the original request which ensures that once 
                // we update the new batch with results these results are also part of the original batch
                csomBatch.Batch.Requests.Add(request.Value.Order, request.Value);
            }

            return batches;
        }

        private static List<CsomBatch> CsomBatchSplittingBySize(CsomBatch batch)
        {
            List<CsomBatch> batches = new List<CsomBatch>();

            // No need to split
            if (batch.Batch.Requests.Count < MaxRequestsInCsomBatch)
            {
                batches.Add(batch);
                return batches;
            }

            // Split in multiple batches
            int counter = 0;
            int order = 0;
            CsomBatch currentBatch = new CsomBatch(batch.Site)
            {
                Batch = new Batch()
                {
                    ThrowOnError = batch.Batch.ThrowOnError
                }
            };

            foreach (var request in batch.Batch.Requests.OrderBy(p => p.Value.Order))
            {
                currentBatch.Batch.Requests.Add(order, request.Value);
                order++;

                counter++;
                if (counter % MaxRequestsInCsomBatch == 0)
                {
                    order = 0;
                    batches.Add(currentBatch);
                    currentBatch = new CsomBatch(batch.Site)
                    {
                        Batch = new Batch()
                        {
                            ThrowOnError = batch.Batch.ThrowOnError
                        }
                    };
                }
            }

            // Add the last part
            if (currentBatch.Batch.Requests.Count > 0)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }
#endregion

        /// <summary>
        /// Checks if a batch contains an API call that still has unresolved tokens...no point in sending the request at that point
        /// </summary>
        /// <param name="batch"></param>
        private static void CheckForUnresolvedTokens(Batch batch)
        {
            foreach (var request in batch.Requests)
            {
                var unresolvedTokens = TokenHandler.UnresolvedTokens(request.Value.ApiCall.Request);
                if (unresolvedTokens.Count > 0)
                {
                    throw new ClientException(ErrorType.UnresolvedTokens,
                        string.Format(PnPCoreResources.Exception_UnresolvedTokens, request.Value.ApiCall.Request));
                }
            }
        }

        /// <summary>
        /// Splits a batch that contains rest, graph, graph beta or csom calls in four batches, each containing the respective calls
        /// </summary>
        /// <param name="input">Batch to split</param>
        /// <param name="graphAlwaysUsesBeta">Indicates if all Microsoft Graph use the Graph beta endpoint</param>
        /// <returns>A rest batch and graph batch</returns>
        private static Tuple<Batch, Batch, Batch, Batch> SplitIntoBatchesPerApiType(Batch input, bool graphAlwaysUsesBeta)
        {
            Batch restBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };
            Batch graphBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };
            Batch graphBetaBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };
            Batch csomBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };

            foreach (var request in input.Requests.Where(p => p.Value.ExecutionNeeded))
            {
                var br = request.Value;
                if (br.ApiCall.Type == ApiType.SPORest)
                {
                    restBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName, br.RequestModules);
                }
                else if (br.ApiCall.Type == ApiType.Graph)
                {
                    if (graphAlwaysUsesBeta)
                    {
                        graphBetaBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName, br.RequestModules);
                    }
                    else
                    {
                        graphBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName, br.RequestModules);
                    }
                }
                else if (br.ApiCall.Type == ApiType.GraphBeta)
                {
                    graphBetaBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName, br.RequestModules);
                }
                else if (br.ApiCall.Type == ApiType.CSOM)
                {
                    csomBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName, br.RequestModules);
                }
            }

            return new Tuple<Batch, Batch, Batch, Batch>(restBatch, graphBatch, graphBetaBatch, csomBatch);
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
                // ExecuteRequest requests are never impacting the domain model
                if (request.ApiCall.ExecuteRequestApiCall)
                {
                    continue;
                }

                EntityFieldInfo keyField = useGraphBatch ? request.EntityInfo.GraphKeyField : request.EntityInfo.SharePointKeyField;

                // Consolidation can only happen when there's a keyfield set in the model
                if (keyField != null && request.Model.HasValue(keyField.Name))
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
            foreach (var consolidation in getConsolidation.Where(p => p.Requests.Count(p => p.ApiCall.RawRequest == false) > 1))
            {
                var firstRequest = consolidation.Requests.OrderBy(p => p.Order).First();

                bool first = true;
                foreach (var request in consolidation.Requests.OrderBy(p => p.Order))
                {
                    if (!first)
                    {
                        // Merge properties/collections
                        firstRequest.Model.Merge(request.Model);

                        // Mark deleted objects as deleted and remove from their respective parent collection
                        request.Model.RemoveFromParentCollection();
                    }
                    else
                    {
                        first = false;
                    }
                }
            }

            // Mark deleted objects as deleted and remove from their respective parent collection
            foreach (var request in batch.Requests.Values.Where(p => p.Method == HttpMethod.Delete || p.ApiCall.RemoveFromModel))
            {
                // ExecuteRequest requests are never impacting the domain model
                if (request.ApiCall.ExecuteRequestApiCall)
                {
                    continue;
                }

                request.Model.RemoveFromParentCollection();
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
                batches.TryRemove(key, out Batch _);
            }
        }

        private static object GetDynamicProperty(object model, string propertyName)
        {
            var pnpObjectType = model.GetType();
            var propertyToGet = pnpObjectType.GetProperty(propertyName);
            return propertyToGet.GetValue(model);
        }

        private static void AddHttpRequestMessageProperties(HttpRequestMessage request, Batch batch)
        {
            if (request == null || batch == null)
            {
                return;
            }

            var firstRequest = batch.Requests.FirstOrDefault().Value;
            if (firstRequest != null)
            {
                if (firstRequest.Model is IDataModelWithContext dataModelWithContext)
                {
#if NET5_0_OR_GREATER
                    if (dataModelWithContext.PnPContext?.Uri != null)
                    {
                        request.Options.Set(new HttpRequestOptionsKey<string>("webUrl"), dataModelWithContext.PnPContext?.Uri.ToString());
                    }
#else
                    if (dataModelWithContext.PnPContext?.Uri != null)
                    {
                        request.Properties["WebUrl"] = dataModelWithContext.PnPContext?.Uri.ToString();
                    }
#endif
                    if (dataModelWithContext.PnPContext.Properties != null && dataModelWithContext.PnPContext.Properties.Count > 0)
                    {
                        foreach(var property in dataModelWithContext.PnPContext.Properties)
                        {
#if NET5_0_OR_GREATER
                            request.Options.Set(new HttpRequestOptionsKey<object>(property.Key), property.Value);
#else
                            request.Properties[property.Key] = property.Value;
#endif
                        }
                    }
                }
            }
        }
    }
}
