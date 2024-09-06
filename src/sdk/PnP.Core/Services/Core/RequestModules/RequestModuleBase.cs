using System;
using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Services
{
    /// <summary>
    /// Base class for request modules
    /// </summary>
    internal abstract class RequestModuleBase : IRequestModule
    {
        /// <summary>
        /// Each request module must have a unique id, override this property in your request module
        /// </summary>
        public virtual Guid Id => throw new NotImplementedException();

        /// <summary>
        /// Defines if the request module is applied when invoking SPO REST requests. Defaults to true
        /// </summary>
        public virtual bool ExecuteForSpoRest => true;

        /// <summary>
        /// Defines if the request module is applied when invoking Microsoft Graph requests. Defaults to true
        /// </summary>
        public virtual bool ExecuteForMicrosoftGraph => true;

        /// <summary>
        /// Defines if the request module is applied when invoking SPO CSOM requests. Defaults to false
        /// </summary>
        public virtual bool ExecuteForSpoCsom => false;

        /// <summary>
        /// Delegate that can be implemented to manipulate the request headers before the request is send to the server
        /// </summary>
        public Action<Dictionary<string, string>> RequestHeaderHandler { get; set; } = null;

        /// <summary>
        /// Delegate that can be implemented to manipulate the request url before the request is send to the server
        /// </summary>
        public Func<string, string> RequestUrlHandler { get; set; } = null;

        /// <summary>
        /// Delegate that can be implemented to manipulate the request body before the request is send to the server
        /// </summary>
        public Func<string, string> RequestBodyHandler { get; set; } = null;

        /// <summary>
        /// Delegate that can be implemented to manipulate the request response before it gets processed
        /// </summary>
        public Func<HttpStatusCode, Dictionary<string, string>, string, Guid, string> ResponseHandler { get; set; } = null;
    }
}
