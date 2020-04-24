using System.Text.Json;

namespace PnP.Core.Model
{
    public struct ApiResponse
    {
        internal ApiResponse(ApiCall apiCall, JsonElement jsonElement)
        {
            ApiCall = apiCall;
            JsonElement = jsonElement;
        }

        internal ApiCall ApiCall { get; private set; }
        internal JsonElement JsonElement { get; private set; }
    }
}
