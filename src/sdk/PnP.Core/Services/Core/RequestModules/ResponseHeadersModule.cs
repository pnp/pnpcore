using System;
using System.Collections.Generic;
using System.Net;

namespace PnP.Core.Services
{
    /// <summary>
    /// This module is configured to return the response headers for SharePoint API requests (REST + CSOM)
    /// </summary>
    internal sealed class ResponseHeadersModule : RequestModuleBase
    {
        override public Guid Id { get => PnPConstants.SPResponseHeadersModuleId; }

        public override bool ExecuteForMicrosoftGraph => true;

        public override bool ExecuteForSpoRest => true;
         
        public override bool ExecuteForSpoCsom => true;

        internal ResponseHeadersModule(Action<Dictionary<string, string>> responseHeaders)
        {
            if (responseHeaders != null)
            {
                // Configure to return the response headers for SharePoint API requests (REST + CSOM) and Microsoft Graph requests
                ResponseHandler = (HttpStatusCode statusCode, Dictionary<string, string> headers, string responseContent, Guid batchRequestId) =>
                {
                    responseHeaders.Invoke(new Dictionary<string, string>(headers));
                    return responseContent;
                };
            }
        }
    }
}
