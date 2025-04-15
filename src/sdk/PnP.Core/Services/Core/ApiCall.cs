using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Struct that defines the API call to make
    /// </summary>
    internal struct ApiCall
    {
        internal ApiCall(string request, ApiType apiType, string jsonBody = null, string receivingProperty = null,
            bool loadPages = false)
        {
            Type = apiType;
            Request = request;
            JsonBody = jsonBody;
            CSOMRequests = new List<Core.CSOM.Requests.IRequest<object>>();
            ReceivingProperty = receivingProperty;
            RawRequest = false;
            RawSingleResult = null;
            RawEnumerableResult = null;
            RawResultsHandler = null;
            Commit = false;
            Interactive = false;
            Content = null;
            ExpectBinaryResponse = false;
            StreamResponse = false;
            RemoveFromModel = false;
            LoadPages = loadPages;
            SkipCollectionClearing = false;
            ExecuteRequestApiCall = false;
            Headers = null;
            AddedViaBatchMethod = false;
        }

        internal ApiCall(List<Core.CSOM.Requests.IRequest<object>> csomRequests, string receivingProperty = null)
        {
            Request = null;
            Type = ApiType.CSOM;
            JsonBody = null;
            CSOMRequests = csomRequests;
            ReceivingProperty = receivingProperty;
            RawRequest = false;
            RawSingleResult = null;
            RawEnumerableResult = null;
            RawResultsHandler = null;
            Commit = false;
            Interactive = false;
            Content = null;
            ExpectBinaryResponse = false;
            StreamResponse = false;
            RemoveFromModel = false;
            LoadPages = false;
            SkipCollectionClearing = false;
            ExecuteRequestApiCall = false;
            Headers = null;
            AddedViaBatchMethod = false;
        }

        /// <summary>
        /// Defines the type of API to call: SPO Rest or Microsoft Graph
        /// </summary>
        internal ApiType Type { get; set; }

        /// <summary>
        /// Defines the URL of the request
        /// </summary>
        internal string Request { get; set; }

        /// <summary>
        /// Defines the JSON body of the request, if any
        /// </summary>
        internal string JsonBody { get; set; }

        /// <summary>
        /// List of CSOM requests for this API call
        /// </summary>
        internal List<Core.CSOM.Requests.IRequest<object>> CSOMRequests { get; set; }

        /// <summary>
        /// Typically the JSON response will be mapped to the current model object, but sometimes a call 
        /// will need to be mapped to a specific property in the current model. This for example is 
        /// done when a property requires it's own Graph Get request.
        /// 
        /// Current this property is set automatically, no need to manually fiddle with it in API call overrides!
        /// </summary>
        internal string ReceivingProperty { get; set; }

        /// <summary>
        /// Is this a raw request that does not require automatic parsing of the returned json?
        /// </summary>
        internal bool RawRequest { get; set; }

        /// <summary>
        /// List containing the parsed Raw results (if a collection is returned)
        /// </summary>
        internal IEnumerable RawEnumerableResult { get; set; }

        /// <summary>
        /// Result object from this API call (if a single result is returned)
        /// </summary>
        internal object RawSingleResult { get; set; }

        /// <summary>
        /// Event handler triggered to parse raw results
        /// </summary>
        internal Action<string, ApiCall> RawResultsHandler { get; set; }

        /// <summary>
        /// When set to true the current model item will be committed as changes are synced with the server
        /// </summary>
        internal bool Commit { get; set; }

        /// <summary>
        /// When set to true this request will be executed interactively, so without being wrapped in a batch request
        /// </summary>
        internal bool Interactive { get; set; }

        /// <summary>
        /// Http Content to add Binary content or other content to this API call
        /// </summary>
        internal HttpContent Content { get; set; }

        /// <summary>
        /// Indicates whether the call expects a binary response
        /// </summary>
        internal bool ExpectBinaryResponse { get; set; }

        /// <summary>
        /// Indicates whether the response will be streamed, meaning we'll return the first bytes before all the content was downloaded
        /// </summary>
        internal bool StreamResponse { get; set; }

        /// <summary>
        /// Indicates if the model instance linked to this request needs to be removed from it's model collection after successful execution of this API call
        /// </summary>
        internal bool RemoveFromModel { get; set; }

        /// <summary>
        /// Indicates if batch should support pagination and make multiple calls
        /// </summary>
        internal bool LoadPages { get; set; }

        /// <summary>
        /// Don't clear the current collection when data is loaded
        /// </summary>
        internal bool SkipCollectionClearing { get; set; }

        /// <summary>
        /// Flag that indicates this ApiCall was issued from an ExecuteRequest method
        /// </summary>
        internal bool ExecuteRequestApiCall { get; set; }

        /// <summary>
        /// Optional Http-Headers for Request to make
        /// </summary>
        internal Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Was this API call requested via a batch method?
        /// </summary>
        internal bool AddedViaBatchMethod { get; set; }
    }
}