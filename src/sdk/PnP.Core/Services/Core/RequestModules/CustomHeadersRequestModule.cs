using System;
using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Services
{
    /// <summary>
    /// Request module that inserts custom headers into the HTTP requests being made
    /// </summary>
    internal sealed class CustomHeadersRequestModule : RequestModuleBase
    {
        /// <summary>
        /// Default constructor, configures the custom header module. If provided header was already defined then it will be overwritten
        /// </summary>
        /// <param name="headers">Custom request headers to be added.</param>
        /// <param name="responseHeaders">Delegate that can be invoked to pass along the response headers</param>
        internal CustomHeadersRequestModule(Dictionary<string, string> headers, Action<Dictionary<string, string>> responseHeaders)
        {
            Headers = headers;

            RequestHeaderHandler = (Dictionary<string, string> currentHeaders) =>
            {
                if (Headers != null && Headers.Count > 0)
                {
                    foreach (var header in Headers)
                    {
                        if (currentHeaders == null)
                        {
                            currentHeaders = new Dictionary<string, string>();
                        }

                        if (currentHeaders.ContainsKey(header.Key))
                        {
                            currentHeaders[header.Key] = header.Value;
                        }
                        else
                        {
                            currentHeaders.Add(header.Key, header.Value);
                        }
                    }
                }
            };

            if (responseHeaders != null)
            {
                ResponseHandler = (HttpStatusCode statusCode, Dictionary<string, string> headers, string responseContent, Guid batchRequestId) =>
                {
                    responseHeaders.Invoke(new Dictionary<string, string>(headers));
                    return responseContent;
                };
            }
        }

        /// <summary>
        /// Unique ID of this request module
        /// </summary>
        public override Guid Id { get => PnPConstants.CustomHeadersModuleId; }

        /// <summary>
        /// The headers that will be added to the requests via this module
        /// </summary>
        internal Dictionary<string, string> Headers { get; private set; }

    }
}
