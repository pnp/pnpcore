using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// The two web-level writes PnP Core does not model: supported UI languages
    /// </summary>
    internal static class WebSettingsRestHelper
    {
        #region T1 - regional settings

        /// <summary>
        /// Reads the web's regional settings.
        /// </summary>
        internal static async Task<JsonElement> GetRegionalSettingsAsync(PnPContext context)
        {
            ApiRequestResponse response = await context.Web.ExecuteRequestAsync(
                new ApiRequest(ApiRequestType.SPORest, "_api/web/regionalsettings")).ConfigureAwait(false);

            return JsonDocument.Parse(response.Response).RootElement.Clone();
        }

        /// <summary>
        /// Finds a named property anywhere in the payload.
        /// </summary>
        private static bool TryFind(JsonElement element, string name, out JsonElement found)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.NameEquals(name))
                    {
                        found = property.Value;
                        return true;
                    }

                    if (TryFind(property.Value, name, out found))
                    {
                        return true;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    if (TryFind(item, name, out found))
                    {
                        return true;
                    }
                }
            }

            found = default;
            return false;
        }

        /// <summary>
        /// Reads an integer from a regional settings payload, falling back when absent.
        /// </summary>
        internal static int GetInt(JsonElement element, string name, int fallback = 0)
        {
            if (!TryFind(element, name, out JsonElement value))
            {
                return fallback;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number))
            {
                return number;
            }

            return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out int parsed)
                ? parsed
                : fallback;
        }

        /// <summary>
        /// Reads a boolean from a regional settings payload, falling back when absent.
        /// </summary>
        internal static bool GetBool(JsonElement element, string name, bool fallback = false)
        {
            if (!TryFind(element, name, out JsonElement value))
            {
                return fallback;
            }

            if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            {
                return value.GetBoolean();
            }

            return value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool parsed)
                ? parsed
                : fallback;
        }


        /// <summary>
        /// Sets the web's time zone by its SharePoint id.
        /// </summary>
        internal static async Task SetTimeZoneAsync(PnPContext context, int timeZoneId)
        {
            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                $"_api/web/regionalsettings/timezones/getbyid({timeZoneId.ToString(CultureInfo.InvariantCulture)})", null))
                .ConfigureAwait(false);
        }

        #endregion
    }
}
