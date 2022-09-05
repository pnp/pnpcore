using System;
using System.Text.Json;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the response of an executed API call
    /// </summary>
    public readonly struct ApiResponse
    {
        internal ApiResponse(ApiCall apiCall, JsonElement jsonElement, Guid batchRequestId)
        {
            ApiCall = apiCall;
            JsonElement = jsonElement;
            BatchRequestId = batchRequestId;
        }

        internal ApiCall ApiCall { get; }
        internal JsonElement JsonElement { get; }
        internal Guid BatchRequestId { get; }
    }
}
