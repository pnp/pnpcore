using System;
using System.Text.Json;

namespace PnP.Core.Model
{
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
