using System.Collections.Generic;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines an API request that can be executed
    /// </summary>
    public class ApiRequest
    {

        /// <summary>
        /// Creates an API request to execute
        /// </summary>
        /// <param name="httpMethod">Http method to use</param>
        /// <param name="type"><see cref="ApiRequestType"/> of the request</param>
        /// <param name="request">Actual API call to issue</param>
        /// <param name="body">Optional body of the request</param>
        /// <param name="headers">Optional headers for the request</param>
        public ApiRequest(HttpMethod httpMethod, ApiRequestType type, string request, string body, Dictionary<string, string> headers = null)
        {
            HttpMethod = httpMethod;
            Type = type;
            Request = request;
            Body = body;
            Headers = headers;
        }

        /// <summary>
        /// Creates an API request to execute
        /// </summary>
        /// <param name="type"><see cref="ApiRequestType"/> of the request</param>
        /// <param name="request">Actual API call to issue</param>
        public ApiRequest(ApiRequestType type, string request) : this(HttpMethod.Get, type, request, null)
        {
        }

        /// <summary>
        /// The Http method to use
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// The REST/Graph API call to execute
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// The type of API call to execute
        /// </summary>
        public ApiRequestType Type { get; set; }

        /// <summary>
        /// The optional payload/body of the API call to execute
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The optional headers of the API call to execute - for example IF-Match for PATCH Request.
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }
    }
}
