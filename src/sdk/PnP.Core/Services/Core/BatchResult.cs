using System;
using System.Net;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Holds information about the outcome of each batch request
    /// </summary>
    public class BatchResult
    {

        internal BatchResult(HttpStatusCode statusCode, ServiceError error, string apiResponse, string apiType, string apiRequest, HttpMethod apiMethod, string apiBody, Guid batchRequestId)
        {
            StatusCode = statusCode;
            Error = error;
            ApiResponse = apiResponse;
            ApiType = apiType;
            ApiRequest = apiRequest;
            ApiMethod = apiMethod;
            ApiBody = apiBody;
            BatchRequestId = batchRequestId;
        }

        /// <summary>
        /// Status code of the request
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// If an error happened then the error information is stored here
        /// </summary>
        public ServiceError Error { get; private set; }

        /// <summary>
        /// The response content from the batch request
        /// </summary>
        public string ApiResponse { get; private set; }

        /// <summary>
        /// API call Type
        /// </summary>
        public string ApiType { get; private set; }

        /// <summary>
        /// API call request
        /// </summary>
        public string ApiRequest { get; private set; }

        /// <summary>
        /// API call method
        /// </summary>
        public HttpMethod ApiMethod { get; private set; }

        /// <summary>
        /// API call body
        /// </summary>
        public string ApiBody { get; private set; }

        /// <summary>
        /// Id of the <see cref="BatchRequest"/> this result is for
        /// </summary>
        public Guid BatchRequestId { get; private set; }
    }
}
