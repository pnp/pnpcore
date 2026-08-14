using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Tenant theme CRUD, over the <c>thememanager</c> REST endpoints.
    /// </summary>
    internal static class TenantThemes
    {
        /// <summary>
        /// The theme names the tenant already has.
        /// </summary>
        internal static async Task<HashSet<string>> GetNamesAsync(PnPContext context)
        {
            List<ITheme> themes = await context.Web.GetBrandingManager()
                .GetAvailableThemesAsync().ConfigureAwait(false);

            return new HashSet<string>(
                themes.Where(t => t.Name != null).Select(t => t.Name),
                StringComparer.OrdinalIgnoreCase);
        }

        internal static async Task AddAsync(PnPContext context, string name, string palette, bool isInverted)
        {
            await PostAsync(context, "AddTenantTheme", name, palette, isInverted).ConfigureAwait(false);
        }

        internal static async Task UpdateAsync(PnPContext context, string name, string palette, bool isInverted)
        {
            await PostAsync(context, "UpdateTenantTheme", name, palette, isInverted).ConfigureAwait(false);
        }

        internal static async Task DeleteAsync(PnPContext context, string name)
        {
            string body = JsonSerializer.Serialize(new Dictionary<string, object> { ["name"] = name });

            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                "_api/thememanager/DeleteTenantTheme", body)).ConfigureAwait(false);
        }

        private static async Task PostAsync(PnPContext context, string method, string name,
            string palette, bool isInverted)
        {
            string body = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["name"] = name,
                ["themeJson"] = BuildThemeJson(name, palette, isInverted),
            });

            await context.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                $"_api/thememanager/{method}", body)).ConfigureAwait(false);
        }

        /// <summary>
        /// Wraps a palette into the shape <c>thememanager</c> expects.
        /// </summary>
        internal static string BuildThemeJson(string name, string palette, bool isInverted)
        {
            using (JsonDocument parsed = JsonDocument.Parse(string.IsNullOrWhiteSpace(palette) ? "{}" : palette))
            {
                if (parsed.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException(
                        $"The palette for theme '{name}' is not a JSON object.", nameof(palette));
                }

                return "{\"name\":" + JsonSerializer.Serialize(name)
                    + ",\"palette\":" + parsed.RootElement.GetRawText()
                    + ",\"isInverted\":" + (isInverted ? "true" : "false") + "}";
            }
        }
    }
}
