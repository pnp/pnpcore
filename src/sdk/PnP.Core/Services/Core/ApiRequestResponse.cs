using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Services
{
    /// <summary>
    /// The response of an executed <see cref="ApiRequest"/>
    /// </summary>
    public class ApiRequestResponse
    {
        /// <summary>
        /// Gets the <see cref="ApiRequest"/> that resulted in this response
        /// </summary>
        public ApiRequest ApiRequest { get; internal set; }

        /// <summary>
        /// Contains the request http status code
        /// </summary>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// The JSON response
        /// </summary>
        public string Response { get; internal set; }

        /// <summary>
        /// Contains additional response headers (if any)
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }
    }
}
