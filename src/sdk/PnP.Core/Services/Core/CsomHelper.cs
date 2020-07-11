using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services
{
    internal static class CsomHelper
    {
        internal static Dictionary<int, JsonElement> ParseResponse(string jsonResponse)
        {
            Dictionary<int, JsonElement> responses = new Dictionary<int, JsonElement>();

            bool first = true;
            int nextActionId = 1;
            var responseJson = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            foreach(var response in responseJson.EnumerateArray())
            {
                if (first)
                {
                    first = false;
                    responses.Add(0, response);
                }
                else
                {
                    if (response.ValueKind == JsonValueKind.Number)
                    {
                        nextActionId = response.GetInt32();
                    }
                    else if (response.ValueKind == JsonValueKind.Object)
                    {
                        responses.Add(nextActionId, response);
                    }
                }
            }

            return responses;
        }
    }
}
