using PnP.Core.Services.Core.CSOM.Utils.CustomConverters;
using PnP.Core.Test.Services.Core.CSOM.Utils.CustomConverters;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal class CSOMResponseHelper
    {
        internal JsonSerializerOptions Options { get; set; }

        internal CSOMResponseHelper()
        {
            Options = new JsonSerializerOptions();
            Options.Converters.Add(new SPGuidConverter());
            Options.Converters.Add(new DateTimeConverter());
        }

        internal T ProcessResponse<T>(string response, long propertyIdentifier)
        {
            List<JsonElement> results = JsonSerializer.Deserialize<List<JsonElement>>(response, Options);
            int idIndex = results.FindIndex(r => CompareIdElement(r, propertyIdentifier));
            if (idIndex >= 0)
            {
                JsonElement result = results[idIndex + 1];
                return JsonSerializer.Deserialize<T>(result.GetRawText(), Options);
            }
            return default;
        }

        private static bool CompareIdElement(JsonElement element, long id)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out long elementId))
            {
                return elementId == id;
            }
            return false;
        }
    }
}
