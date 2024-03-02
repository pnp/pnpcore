using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines a <see cref="Batch"/> of requests to execute
    /// </summary>
    public class Batch
    {
        /// <summary>
        /// Instantiates a <see cref="Batch"/> for a given id
        /// </summary>
        /// <param name="id">Id of the batch</param>
        internal Batch(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Default public constructor, instantiates a <see cref="Batch"/>
        /// </summary>
        public Batch() : this(Guid.NewGuid()) { }

        /// <summary>
        /// Id of the <see cref="Batch"/>
        /// </summary>
        internal Guid Id { get; private set; }

        /// <summary>
        /// List with requests, this list will be cleared after the requests have been executed 
        /// </summary>
        public SortedList<int, BatchRequest> Requests { get; internal set; } = new SortedList<int, BatchRequest>();

        /// <summary>
        /// List with batch results, will be populated when <see cref="ThrowOnError"/> is set to false
        /// </summary>
        internal List<BatchResult> Results { get; private set; } = new List<BatchResult>();

        /// <summary>
        /// Throw an exception when a batch request had an error. Defaults to true, if set to 
        /// false then a collection of batch errors will be available as output of the Execute methods
        /// </summary>
        internal bool ThrowOnError { get; set; } = true;

        /// <summary>
        /// Was this <see cref="Batch"/> executed?
        /// </summary>
        public bool Executed { get; internal set; }

        /// <summary>
        /// Event handler triggered when batch execution is done
        /// </summary>
        internal Action BatchExecuted { get; set; }

        /// <summary>
        /// Returns true if this batch had errors and throw on error was turned off
        /// </summary>
        internal bool HasErrors
        {
            get
            {
                return !ThrowOnError && Results.Count > 0;
            }
        }

        /// <summary>
        /// Only use Graph batching when all requests in the batch are targeting Graph
        /// </summary>
        internal bool UseGraphBatch
        {
            get
            {
                return Requests.Any(r => r.Value.ApiCall.Type == ApiType.Graph || r.Value.ApiCall.Type == ApiType.GraphBeta);
            }
        }

        /// <summary>
        /// Only use Csom batching when all requests in the batch are targeting csom
        /// </summary>
        internal bool UseCsomBatch
        {
            get
            {
                return Requests.Any(r => r.Value.ApiCall.Type == ApiType.CSOM);
            }
        }

        /// <summary>
        /// Returns true if this <see cref="Batch"/> contains both SharePoint REST as Microsoft Graph requests
        /// </summary>
        internal bool HasMixedApiTypes
        {
            get
            {
                int apiTypeCount = 0;

                if (Requests.Any(r => r.Value.ExecutionNeeded && r.Value.ApiCall.Type == ApiType.SPORest)) { apiTypeCount++; }
                if (Requests.Any(r => r.Value.ExecutionNeeded && r.Value.ApiCall.Type == ApiType.Graph)) { apiTypeCount++; }
                if (Requests.Any(r => r.Value.ExecutionNeeded && r.Value.ApiCall.Type == ApiType.GraphBeta)) { apiTypeCount++; }
                if (Requests.Any(r => r.Value.ExecutionNeeded && r.Value.ApiCall.Type == ApiType.CSOM)) { apiTypeCount++; }

                return apiTypeCount > 1;
            }
        }

        /// <summary>
        /// Returns true if this batch contains an interactive request
        /// </summary>
        internal bool HasInteractiveRequest
        {
            get
            {
                return (Requests.Any(r => r.Value.ApiCall.Interactive));
            }
        }

        /// <summary>
        /// Returns true if this <see cref="Batch"/> can be completely executed via SPO REST
        /// </summary>
        internal bool CanFallBackToSPORest
        {
            get
            {
                return !Requests.Any(r =>
                    (r.Value.ApiCall.Type != ApiType.SPORest) &&
                    (r.Value.BackupApiCall.Equals(default(ApiCall)) ||
                    r.Value.BackupApiCall.Type != ApiType.SPORest));
            }
        }

        /// <summary>
        /// Add a new request to this <see cref="Batch"/>
        /// </summary>
        /// <param name="model">Entity object on for which this request was meant</param>
        /// <param name="entityInfo">Info about the entity object</param>
        /// <param name="method">Type of http method (GET/PATCH/POST/...)</param>
        /// <param name="apiCall">Rest/Graph call</param>
        /// <param name="backupApiCall">Backup rest api call, will be used in case we encounter a mixed batch</param>
        /// <param name="fromJsonCasting">Delegate for json type parsing</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        /// <param name="requestModules">List with request modules to execute</param>
        /// <returns>The id to created batch request</returns>
        internal Guid Add(TransientObject model, EntityInfo entityInfo, HttpMethod method, ApiCall apiCall,
            ApiCall backupApiCall, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson, string operationName, List<IRequestModule> requestModules)
        {
            var lastAddedRequest = GetLastRequest();
            int order = 0;
            if (lastAddedRequest != null)
            {
                order = lastAddedRequest.Order + 1;
            }

            var batchRequest = new BatchRequest(model, entityInfo, method, apiCall, backupApiCall, fromJsonCasting, postMappingJson, operationName, order);
            batchRequest.RequestModules = requestModules;

            Requests.Add(order, batchRequest);

            return batchRequest.Id;
        }

        /// <summary>
        /// Add a new request to this <see cref="Batch"/>
        /// </summary>
        /// <param name="model">Entity object on for which this request was meant</param>
        /// <param name="entityInfo">Info about the entity object</param>
        /// <param name="method">Type of http method (GET/PATCH/POST/...)</param>
        /// <param name="apiCall">Rest/Graph call</param>
        /// <param name="backupApiCall">Backup rest api call, will be used in case we encounter a mixed batch</param>
        /// <param name="fromJsonCasting">Delegate for json type parsing</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        /// <returns>The id to created batch request</returns>
        internal Guid Add(TransientObject model, EntityInfo entityInfo, HttpMethod method, ApiCall apiCall, ApiCall backupApiCall, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson, string operationName)
        {
            // Copy the request modules list as it will get cleared at context level
            List<IRequestModule> requestModulesToUse = null;
            var requestModules = (model as IDataModelWithContext).PnPContext.RequestModules;
            if (requestModules != null)
            {
                requestModulesToUse = new List<IRequestModule>(requestModules);
            }

            return Add(model, entityInfo, method, apiCall, backupApiCall, fromJsonCasting, postMappingJson, operationName, requestModulesToUse);
        }

        /// <summary>
        /// Gets the request at a specific order
        /// </summary>
        /// <param name="order">Order to get the request for</param>
        /// <returns>The request at the given order</returns>
        internal BatchRequest GetRequest(int order)
        {
            return Requests[order];
        }

        /// <summary>
        /// Gets the request by id
        /// </summary>
        /// <returns>The request at the given order</returns>
        internal BatchRequest GetRequest(Guid id)
        {
            return Requests.First(r => r.Value.Id == id).Value;
        }

        /// <summary>
        /// Verifies if this batch contains the given batch request
        /// </summary>
        /// <param name="id">If of the batch request to check for</param>
        /// <returns>True if the batch contains the request, false otherwise</returns>
        internal bool ContainsRequest(Guid id)
        {
            var request = Requests.FirstOrDefault(r => r.Value.Id == id);
            return request.Value != null;
        }

        /// <summary>
        /// Promotes a backup rest call to be the actual api call
        /// </summary>
        internal void MakeSPORestOnlyBatch()
        {
            foreach (var request in Requests.Where(p => p.Value.ApiCall.Type == ApiType.Graph).ToList())
            {
                request.Value.ApiCall = request.Value.BackupApiCall;
            }
        }

        internal void AddBatchResult(BatchRequest request, HttpStatusCode httpStatusCode, string apiResponse, ServiceError error)
        {
            Results.Add(new BatchResult(
                httpStatusCode,
                error,
                apiResponse,
                request.ApiCall.Type.ToString(),
                request.ApiCall.Request,
                request.Method,
                !string.IsNullOrEmpty(request.ApiCall.JsonBody) ? request.ApiCall.JsonBody : "",
                request.Id));
        }

        internal Guid PrepareLastAddedRequestForBatchProcessing<T>(Action<string, ApiCall> rawResultsHandler, T resultObject)
        {
            var lastBatchRequest = Requests.Last().Value;
            var key = Requests.Last().Key;
            var sourceApiCall = lastBatchRequest.ApiCall;

            lastBatchRequest.ApiCall = new ApiCall()
            {
                BinaryBody = sourceApiCall.BinaryBody,
                Commit = sourceApiCall.Commit,
                CSOMRequests = sourceApiCall.CSOMRequests,
                ExecuteRequestApiCall = sourceApiCall.ExecuteRequestApiCall,
                ExpectBinaryResponse = sourceApiCall.ExpectBinaryResponse,
                Headers = sourceApiCall.Headers,
                Interactive = sourceApiCall.Interactive,
                JsonBody = sourceApiCall.JsonBody,
                LoadPages = sourceApiCall.LoadPages,
                RawResultsHandler = rawResultsHandler,
                RawSingleResult = resultObject,
                ReceivingProperty = sourceApiCall.ReceivingProperty,
                RemoveFromModel = sourceApiCall.RemoveFromModel,
                Request = sourceApiCall.Request,
                SkipCollectionClearing = sourceApiCall.SkipCollectionClearing,
                StreamResponse = sourceApiCall.StreamResponse,
                Type = sourceApiCall.Type
            };

            Requests[key] = lastBatchRequest;

            return lastBatchRequest.Id;
        }

        /// <summary>
        /// Gets the last request in the list of requests
        /// </summary>
        /// <returns>The last request, null if there were no requests</returns>
        private BatchRequest GetLastRequest()
        {
            var last = Requests.LastOrDefault();
            return last.Value ?? null;
        }

    }
}
