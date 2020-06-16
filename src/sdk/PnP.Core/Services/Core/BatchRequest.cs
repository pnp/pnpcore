using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using PnP.Core.Model;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines a request in a <see cref="Batch"/>
    /// </summary>
    internal class BatchRequest
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="modelInstance">Entity object on for which this request was meant</param>
        /// <param name="entityInfo">Info about the entity object</param>
        /// <param name="method">Type of http method (GET/PATCH/POST/...)</param>
        /// <param name="apiCall">Rest call to execute</param>
        /// <param name="backupApiCall">Backup rest api call, will be used in case we encounter a mixed batch</param>
        /// <param name="fromJsonCasting">Delegate for json type parsing</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        /// <param name="order">Order of the request in the list of requests</param>
        internal BatchRequest(TransientObject modelInstance, EntityInfo entityInfo, HttpMethod method, ApiCall apiCall, ApiCall backupApiCall, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson, int order)
        {
            Id = Guid.NewGuid();
            Model = modelInstance;
            EntityInfo = entityInfo;
            Method = method;
            ApiCall = apiCall;
            BackupApiCall = backupApiCall;
            FromJsonCasting = fromJsonCasting;
            PostMappingJson = postMappingJson;
            Order = order;
            ExecutionNeeded = true;
        }

        /// <summary>
        /// Id of the <see cref="BatchRequest"/>
        /// </summary>
        internal Guid Id { get; private set; }

        /// <summary>
        /// Entity object on for which this request was meant
        /// </summary>
        internal TransientObject Model { get; private set; }

        /// <summary>
        /// Info about the entity object
        /// </summary>
        internal EntityInfo EntityInfo { get; private set; }

        /// <summary>
        /// Delegate for json type parsing
        /// </summary>
        internal Func<FromJson, object> FromJsonCasting { get; private set; }

        /// <summary>
        /// Delegate for post mapping 
        /// </summary>
        internal Action<string> PostMappingJson { get; private set; }

        /// <summary>
        /// Type of http method (GET/PATCH/POST/...)
        /// </summary>
        internal HttpMethod Method { get; private set; }

        /// <summary>
        /// The rest call to execute
        /// </summary>
        internal ApiCall ApiCall { get; set; }

        /// <summary>
        /// Backup rest api call, will be used in case we encounter a mixed <see cref="Batch"/>
        /// </summary>
        internal ApiCall BackupApiCall { get; private set; }

        /// <summary>
        /// Order of the request in the list of requests
        /// </summary>
        internal int Order { get; private set; }

        /// <summary>
        /// Json response for this request (only populated when the <see cref="Batch"/> was executed)
        /// </summary>
        internal string ResponseJson { get; private set; }

        /// <summary>
        /// Http response code for this request (only populated when the <see cref="Batch"/> was executed)
        /// </summary>
        internal HttpStatusCode ResponseHttpStatusCode { get; private set; }

        /// <summary>
        /// Headers returned for this request (e.g. Content header to follow-up on async server side operations)
        /// </summary>
        internal Dictionary<string, string> ResponseHeaders { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// This batch request was not executed yet or a retry is needed
        /// </summary>
        internal bool ExecutionNeeded { get; private set; }

        /// <summary>
        /// Records the response of a request (fired as part of the execution of a <see cref="Batch"/>)
        /// </summary>
        /// <param name="json">Json response for this request</param>
        /// <param name="responseHttpStatusCode">Http response status code for this request</param>
        internal void AddResponse(string json, HttpStatusCode responseHttpStatusCode)
        {
            AddResponse(json, responseHttpStatusCode, null);
        }

        /// <summary>
        /// Records the response of a request (fired as part of the execution of a <see cref="Batch"/>)
        /// </summary>
        /// <param name="json">Json response for this request</param>
        /// <param name="responseHttpStatusCode">Http response status code for this request</param>
        /// <param name="responseHeaders">Http response headers</param>
        internal void AddResponse(string json, HttpStatusCode responseHttpStatusCode, Dictionary<string, string> responseHeaders)
        {
            ResponseJson = json;
            ExecutionNeeded = false;
            ResponseHttpStatusCode = responseHttpStatusCode;
            if (responseHeaders != null)
            {
                ResponseHeaders = responseHeaders;
            }
        }

        /// <summary>
        /// This batch request needs to be retried
        /// </summary>
        /// <param name="responseHttpStatusCode">Http response status code for this request</param>
        /// <param name="responseHeaders">Http response headers</param>
        internal void FlagForRetry(HttpStatusCode responseHttpStatusCode, Dictionary<string, string> responseHeaders)
        {
            ResponseHttpStatusCode = responseHttpStatusCode;
            ExecutionNeeded = true;
            if (responseHeaders != null)
            {
                ResponseHeaders = responseHeaders;
            }
        }
    }
}
