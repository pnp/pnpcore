using System;
using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Services
{
    /// <summary>
    /// Interface defining a request module
    /// </summary>
    internal interface IRequestModule
    {
        /// <summary>
        /// Id of the request module
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Defines if the request module is applied when invoking SPO REST requests. Defaults to true
        /// </summary>
        bool ExecuteForSpoRest { get; }

        /// <summary>
        /// Defines if the request module is applied when invoking Microsoft Graph requests. Defaults to true
        /// </summary>
        bool ExecuteForMicrosoftGraph { get; }

        /// <summary>
        /// Defines if the request module is applied when invoking SPO CSOM requests. Defaults to false
        /// </summary>
        bool ExecuteForSpoCsom { get; }

        /// <summary>
        /// Delegate that can be implemented to manipulate the request headers
        /// </summary>
        Action<Dictionary<string, string>> RequestHeaderHandler { get; }

        /// <summary>
        /// Delegate that can be implemented to manipulate the request url before it gets send to the server
        /// </summary>
        Func<string, string> RequestUrlHandler { get; }

        /// <summary>
        /// Delegate that can be implemented to manipulate the request body before it gets send to the server
        /// </summary>
        Func<string, string> RequestBodyHandler { get; }

        /// <summary>
        /// Delegate that can be implemented to manipulate the request response before it gets processed
        /// </summary>
        Func<HttpStatusCode, Dictionary<string, string>, string, Guid, string> ResponseHandler { get; }
    }
}
