using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal sealed class CSOMResponseHelper
    {
#pragma warning disable CA1822 // Mark members as static
        internal T ProcessResponse<T>(string response, long propertyIdentifier)
#pragma warning restore CA1822 // Mark members as static
        {
            List<JsonElement> results = JsonSerializer.Deserialize<List<JsonElement>>(response, PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);
            int idIndex = results.FindIndex(r => CompareIdElement(r, propertyIdentifier));
            if (idIndex >= 0)
            {
                JsonElement result = results[idIndex + 1];
                return JsonSerializer.Deserialize<T>(result.GetRawText(), PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);
            }
            return default;
        }

        internal static bool CompareIdElement(JsonElement element, long id)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out long elementId))
            {
                return elementId == id;
            }
            return false;
        }
    }
}
