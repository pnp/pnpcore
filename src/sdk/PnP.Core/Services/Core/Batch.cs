using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PnP.Core.Model;

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
        /// List with requests 
        /// </summary>
        internal SortedList<int, BatchRequest> Requests { get; private set; } = new SortedList<int, BatchRequest>();

        /// <summary>
        /// Was this <see cref="Batch"/> executed?
        /// </summary>
        internal bool Executed { get; set; } = false;

        /// <summary>
        /// Only use Graph batching when all requests in the batch are targeting Graph
        /// </summary>
        internal bool UseGraphBatch
        {
            get
            {
                return !Requests
                    .Any(r => r.Value.ApiCall.Type == ApiType.SPORest);
            }
        }

        /// <summary>
        /// Returns true if this <see cref="Batch"/> contains both SharePoint REST as Microsoft Graph requests
        /// </summary>
        internal bool HasMixedApiTypes
        {
            get
            {
                bool foundRest = Requests
                    .Any(r => r.Value.ApiCall.Type == ApiType.SPORest);
                bool foundGraph = Requests
                    .Any(r => r.Value.ApiCall.Type == ApiType.Graph);

                return foundRest && foundGraph;
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
                    r.Value.ApiCall.Type == ApiType.Graph &&
                    (r.Value.BackupApiCall.Equals(default(ApiCall)) ||
                    r.Value.BackupApiCall.Type == ApiType.Graph));
            }
        }

        /// <summary>
        /// A raw batch will return the raw json results to the caller, results will not be parsed and loaded into the model
        /// </summary>
        internal bool Raw { get; set; } = false;

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
        /// <returns>The id to created batch request</returns>
        internal Guid Add(TransientObject model, EntityInfo entityInfo, HttpMethod method, ApiCall apiCall, ApiCall backupApiCall, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson)
        {
            var lastAddedRequest = GetLastRequest();
            int order = 0;
            if (lastAddedRequest != null)
            {
                order = lastAddedRequest.Order + 1;
            }

            var batchRequest = new BatchRequest(model, entityInfo, method, apiCall, backupApiCall, fromJsonCasting, postMappingJson, order);

            Requests.Add(order, batchRequest);

            return batchRequest.Id;
        }

        /// <summary>
        /// Get's the request at a specific order
        /// </summary>
        /// <param name="order">Order to get the request for</param>
        /// <returns>The request at the given order</returns>
        internal BatchRequest GetRequest(int order)
        {
            return Requests[order];
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

        /// <summary>
        /// Get's the last request in the list of requests
        /// </summary>
        /// <returns>The last request, null if there were no requests</returns>
        private BatchRequest GetLastRequest()
        {
            var last = Requests.LastOrDefault();
            return last.Value ?? null;
        }

    }
}
