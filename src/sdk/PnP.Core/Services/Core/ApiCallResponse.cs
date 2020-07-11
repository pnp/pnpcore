using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace PnP.Core.Services
{
    internal struct ApiCallResponse
    {
        internal ApiCallResponse(string json, HttpStatusCode statusCode, Dictionary<string, string> headers, Dictionary<int, JsonElement> csomResponseJson)
        {
            Json = json;
            StatusCode = statusCode;
            Headers = headers;
            CsomResponseJson = csomResponseJson;
        }

        /// <summary>
        /// Contains the json response of the request (if any)
        /// </summary>
        internal string Json { get; private set; }

        /// <summary>
        /// Contains the request http status code
        /// </summary>
        internal HttpStatusCode StatusCode { get; private set; } 
        
        /// <summary>
        /// Contains additional response headers (if any)
        /// </summary>
        internal Dictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Contains CSOM response values
        /// </summary>
        internal Dictionary<int, JsonElement> CsomResponseJson { get; private set; }
    }
}
