using System;
using System.Text.Json;

namespace PnP.Core.Services
{
    /// <summary>
    /// Defines the response of an executed API call
    /// </summary>
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct ApiResponse
#pragma warning restore CA1815 // Override equals and operator equals on value types
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
