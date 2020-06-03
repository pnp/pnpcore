using System;
using System.Text.Json;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the response of an executed API call
    /// </summary>
    public struct ApiResponse
    {
        internal ApiResponse(ApiCall apiCall, JsonElement jsonElement, Guid batchRequestId)
        {
            ApiCall = apiCall;
            JsonElement = jsonElement;
            BatchRequestId = batchRequestId;
        }

        internal ApiCall ApiCall { get; private set; }
        internal JsonElement JsonElement { get; private set; }
        internal Guid BatchRequestId { get; private set; }
    }
}
