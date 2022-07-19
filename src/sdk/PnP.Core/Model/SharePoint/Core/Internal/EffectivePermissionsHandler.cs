using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class EffectivePermissionsHandler
    {
        internal static IBasePermissions ParseGetUserEffectivePermissionsResponse(string json)
        {
            var parsedJson = JsonSerializer.Deserialize<JsonElement>(json);
            var basePermissions = new BasePermissions();

            if (parsedJson.TryGetProperty("High", out JsonElement high))
            {
                basePermissions.High = Convert.ToInt64(high.GetString());
            }

            if (parsedJson.TryGetProperty("Low", out JsonElement low))
            {
                basePermissions.Low = Convert.ToInt64(low.GetString());
            }

            return basePermissions;
        }
    }
}
