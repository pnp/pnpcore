using System;
using System.Collections.Generic;

namespace PnP.Core.Services
{
    /// <summary>
    /// Request module that inserts custom headers into the HTTP requests being made
    /// </summary>
    internal class CustomHeadersRequestModule : RequestModuleBase
    {
        /// <summary>
        /// Default constructor, configures the custom header module. If provided header was already defined then it will be overwritten
        /// </summary>
        /// <param name="headers">Custom request headers to be added.</param>
        public CustomHeadersRequestModule(Dictionary<string, string> headers)
        {
            Headers = headers;

            RequestHeaderHandler = (Dictionary<string, string> currentHeaders) =>
            {
                if (Headers != null && Headers.Count > 0)
                {
                    foreach (var header in Headers)
                    {
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
        }

        /// <summary>
        /// Unique ID of this request module
        /// </summary>
        public override Guid Id { get => PnPConstants.CustomHeadersModuleId; }

        /// <summary>
        /// The headers that will be added to the requests via this module
        /// </summary>
        public Dictionary<string, string> Headers { get; private set; }

    }
}
