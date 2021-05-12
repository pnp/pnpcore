﻿using Microsoft.Extensions.Logging;
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
                    throw new Exception("TODO");
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

                if (request.ApiCall.SkipCollectionClearing)
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
                            (Batch spoRestBatch, Batch graphBatch, Batch csomBatch) = SplitIntoBatchesPerApiType(batch);
                            // execute the 3 batches
                            await ExecuteSharePointRestBatchAsync(spoRestBatch).ConfigureAwait(false);
                            await ExecuteMicrosoftGraphBatchAsync(graphBatch).ConfigureAwait(false);
                            await ExecuteCsomBatchAsync(csomBatch).ConfigureAwait(false);

                            // Aggregate batch results from the executed batches
                            batch.Results.AddRange(spoRestBatch.Results);
                            batch.Results.AddRange(graphBatch.Results);
                            batch.Results.AddRange(csomBatch.Results);
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

            return batch.Results;
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
            if (!batch.Requests.Any())
            {
                return;
            }

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
                        throw new ServiceException(ErrorType.TooManyBatchRetries, 0,
                            string.Format(PnPCoreResources.Exception_ServiceException_BatchMaxRetries, retryCount));
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
                                // All was good, connect response to the original request
                                batchRequest.AddResponse(bodyContent.ToString(), graphBatchResponse.Status, graphBatchResponse.Headers);

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
                    else
                    {
                        // Invoke a delegate (if defined) to trigger processing of raw batch requests
                        batchRequest.ApiCall.RawResultsHandler?.Invoke(batchRequest.ResponseJson, batchRequest.ApiCall);
                    }
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

                telemetryManager?.LogServiceRequest(request, PnPContext);
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

            // No need to split
            if (batch.Batch.Requests.Count < MaxRequestsInSharePointRestBatch)
            {
                batches.Add(batch);
                return batches;
            }

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
            if (!batch.Requests.Any())
            {
                return;
            }

            // A batch can only combine requests for the same web, if needed we need to split the incoming batch in batches per web
            var restBatches = SharePointRestBatchSplitting(batch);

            // Execute the batches
            foreach (var restBatch in restBatches)
            {
                // A batch can contain more than the maximum number of items in a SharePoint batch, so if needed breakup a batch in multiple batches
                var splitRestBatches = SharePointRestBatchSplittingBySize(restBatch);

                foreach (var splitRestBatch in splitRestBatches)
                {

                    (string requestBody, string requestKey) = BuildSharePointRestBatchRequestContent(splitRestBatch.Batch);

                    PnPContext.Logger.LogDebug(requestBody);

                    // Make the batch call
                    using StringContent content = new StringContent(requestBody);

#pragma warning disable CA2000 // Dispose objects before losing scope
                    using (var request = new HttpRequestMessage(HttpMethod.Post, $"{splitRestBatch.Site.ToString().TrimEnd(new char[] { '/' })}/_api/$batch"))
#pragma warning restore CA2000 // Dispose objects before losing scope
                    {
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
                        if (PnPContext.Mode != TestMode.Mock) // || !TestManager.IsMockAvailable(PnPContext, requestKey))
                        {
#endif
                            // Process the authentication headers using the currently
                            // configured instance of IAuthenticationProvider
                            await ProcessSharePointRestAuthentication(splitRestBatch.Site, request).ConfigureAwait(false);

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

                                await ProcessSharePointRestBatchResponse(splitRestBatch, batchResponse).ConfigureAwait(false);
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

                            await ProcessSharePointRestBatchResponse(splitRestBatch, batchResponse).ConfigureAwait(false);
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
        /// <returns></returns>
        private async Task ProcessSharePointRestBatchResponse(SPORestBatch restBatch, string batchResponse)
        {
            using (var tracer = Tracer.Track(PnPContext.Logger, "ExecuteSharePointRestBatchAsync-JSONToModel"))
            {
                // Process the batch response, assign each response to it's request 
                ProcessSharePointRestBatchResponseContent(restBatch.Batch, batchResponse);

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
                        else
                        {
                            // Invoke a delegate (if defined) to trigger processing of raw batch requests
                            batchRequest.ApiCall.RawResultsHandler?.Invoke(batchRequest.ResponseJson, batchRequest.ApiCall);
                        }
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
            int counter = -1;
            var httpStatusCode = HttpStatusCode.Continue;

            bool responseContentOpen = false;
            StringBuilder responseContent = new StringBuilder();
            foreach (var line in responseLines)
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
                                // Todo: parsing json in line to provide a more structured error message + add to logging
                                throw new SharePointRestServiceException(ErrorType.SharePointRestServiceError, (int)httpStatusCode, responseContent.ToString());
                            }
                            else
                            {
                                // Store the batch error
                                batch.AddBatchResult(currentBatchRequest,
                                                     httpStatusCode,
                                                     responseContent.ToString(),
                                                     new SharePointRestError(ErrorType.SharePointRestServiceError, (int)httpStatusCode, responseContent.ToString()));
                            }
                        }
                        else
                        {
                            if (httpStatusCode == HttpStatusCode.NoContent)
                            {
                                currentBatchRequest.AddResponse("", httpStatusCode);
                            }
                            else
                            {
                                currentBatchRequest.AddResponse(responseContent.ToString(), httpStatusCode);
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

                    counter++;
                }
                // Response status code
                else if (line.StartsWith("HTTP/1.1 "))
                {
                    // HTTP/1.1 200 OK
                    if (int.TryParse(line.Substring(9, 3), out int parsedHttpStatusCode))
                    {
                        httpStatusCode = (HttpStatusCode)parsedHttpStatusCode;
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
                    responseContent.AppendLine(line.TrimEnd('\r'));
                    responseContentOpen = true;
                }
                // More content is being returned, so let's append it
                else if (responseContentOpen)
                {
                    // content can be seperated via \r\n and we split on \n. Since we're using AppendLine remove the carriage return to avoid duplication
                    responseContent.AppendLine(line.TrimEnd('\r'));
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
                using (var request = new HttpRequestMessage(restRequest.Method, restRequest.ApiCall.Request))
                {
                    PnPContext.Logger.LogDebug($"{restRequest.Method} {restRequest.ApiCall.Request}");

                    if (!string.IsNullOrEmpty(restRequest.ApiCall.JsonBody))
                    {
                        content = new StringContent(restRequest.ApiCall.JsonBody, Encoding.UTF8, "application/json");
                        request.Content = content;

                        // Remove the default Content-Type content header
                        if (content.Headers.Contains("Content-Type"))
                        {
                            content.Headers.Remove("Content-Type");
                        }
                        // Add the batch Content-Type header
                        content.Headers.Add($"Content-Type", $"application/json;odata=verbose");
                        PnPContext.Logger.LogDebug(restRequest.ApiCall.JsonBody);
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
                        if (restRequest.ApiCall.Request.IndexOf("/_api/", 0) > -1)
                        {
                            site = new Uri(restRequest.ApiCall.Request.Substring(0, restRequest.ApiCall.Request.IndexOf("/_api/", 0)));
                        }
                        else
                        {
                            // We need this as we use _layouts/15/download.aspx to download files 
                            site = new Uri(restRequest.ApiCall.Request.Substring(0, restRequest.ApiCall.Request.IndexOf("/_layouts/", 0)));
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
                        HttpResponseMessage response = await PnPContext.RestClient.Client.SendAsync(request, httpCompletionOption).ConfigureAwait(false);

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
                                    var mockedRewrittenFileString = MockingFileRewriteHandler(requestResponseStream.CopyAsString());
#pragma warning disable CA2000 // Dispose objects before losing scope
                                    requestResponseStream = mockedRewrittenFileString.AsStream();
#pragma warning restore CA2000 // Dispose objects before losing scope
                                }

                                // Write response
                                TestManager.RecordResponse(PnPContext, batchKey, requestResponseStream);
                            }
#endif

                            await ProcessSharePointRestInteractiveResponse(restRequest, response.StatusCode, requestResponseStream).ConfigureAwait(false);

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
#pragma warning disable CA2000 // Dispose objects before losing scope
                        var requestResponseStream = TestManager.MockResponseAsStream(PnPContext, batchKey);
#pragma warning restore CA2000 // Dispose objects before losing scope

                        // TODO: get status code from recorded response file
                        await ProcessSharePointRestInteractiveResponse(restRequest, HttpStatusCode.OK, requestResponseStream).ConfigureAwait(false);
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

        private static async Task ProcessSharePointRestInteractiveResponse(BatchRequest restRequest, HttpStatusCode responseCode, Stream responseContent)
        {
            // If a binary response content is expected
            if (restRequest.ApiCall.ExpectBinaryResponse)
            {
                // Add it to the request and stop processing the response
                restRequest.AddResponse(responseContent, responseCode);
                return;
            }

            if (responseCode == HttpStatusCode.NoContent)
            {
                restRequest.AddResponse("", responseCode);
            }
            else
            {
                using (var streamReader = new StreamReader(responseContent))
                {
                    responseContent.Seek(0, SeekOrigin.Begin);
                    string requestResponse = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    restRequest.AddResponse(requestResponse, responseCode);
                }
            }

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
            if (!batch.Requests.Any())
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

                                    ProcessCsomBatchResponse(csomBatch, batchResponse, response.StatusCode);
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

                                ProcessCsomBatchResponse(csomBatch, batchResponse, HttpStatusCode.OK);
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
        /// Splits a batch that contains rest and graph calls in two batches, one containing the rest calls, one containing the graph calls
        /// </summary>
        /// <param name="input">Batch to split</param>
        /// <returns>A rest batch and graph batch</returns>
        private static Tuple<Batch, Batch, Batch> SplitIntoBatchesPerApiType(Batch input)
        {
            Batch restBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };
            Batch graphBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };
            Batch csomBatch = new Batch()
            {
                ThrowOnError = input.ThrowOnError
            };

            foreach (var request in input.Requests)
            {
                var br = request.Value;
                if (br.ApiCall.Type == ApiType.SPORest)
                {
                    restBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName);
                }
                else if (br.ApiCall.Type == ApiType.Graph || br.ApiCall.Type == ApiType.GraphBeta)
                {
                    graphBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName);
                }
                else if (br.ApiCall.Type == ApiType.CSOM)
                {
                    csomBatch.Add(br.Model, br.EntityInfo, br.Method, br.ApiCall, br.BackupApiCall, br.FromJsonCasting, br.PostMappingJson, br.OperationName);
                }
            }

            return new Tuple<Batch, Batch, Batch>(restBatch, graphBatch, csomBatch);
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
            foreach (var consolidation in getConsolidation.Where(p => p.Requests.Count > 1))
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
    }
}
