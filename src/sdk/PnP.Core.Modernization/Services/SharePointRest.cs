
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PnP.Core.Modernization.Services
{
    internal static class SharePointRest
    {
        internal static JsonElement Request(PnPContext context, string url, HttpMethod method = null, object body = null)
        {
            SharePointOnlineSupport.Validate(context);
            var response = context.Web.ExecuteRequest(new ApiRequest(method ?? HttpMethod.Get, ApiRequestType.SPORest,
                url, body == null ? null : JsonSerializer.Serialize(body),
                new Dictionary<string, string> { ["Accept"] = "application/json;odata=nometadata" }));
            using (var json = JsonDocument.Parse(response.Response))
            {
                return json.RootElement.Clone();
            }
        }

        internal static bool IsNotFound(this ServiceException exception) =>
            exception.Error is ServiceError error && (error.HttpResponseCode == 404 ||
                error.Code == "System.IO.FileNotFoundException" ||
                error is SharePointRestError rest && rest.ServerErrorCode == -2147024894);

        internal static string GetLocalizedString(PnPContext context, string resource, string file, int language) =>
            Request(context, "_api/SP.Utilities.Utility.GetLocalizedString", HttpMethod.Post,
                new { source = resource, defaultResourceFile = file, language }).GetProperty("value").GetString();

        internal static string GetWebUrlFromPageUrl(PnPContext context, Uri page) =>
            Request(context, "_api/SP.Web.GetWebUrlFromPageUrl(@v)?@v='" + Escape(page.AbsoluteUri) + "'")
                .GetProperty("value").GetString();

        internal static string Escape(string value) => Uri.EscapeDataString(value.Replace("'", "''"));

        internal static bool HasPermissions(this IList list, params PermissionKind[] permissions) =>
            HasPermissions(list.PnPContext, $"_api/web/lists(guid'{list.Id}')/EffectiveBasePermissions", permissions);

        internal static bool HasPermissions(this IListItem item, params PermissionKind[] permissions) =>
            HasPermissions(item.PnPContext, $"_api/web/lists(guid'{item.ParentList.Id}')/items({item.Id})/EffectiveBasePermissions", permissions);

        private static bool HasPermissions(PnPContext context, string url, PermissionKind[] permissions)
        {
            var result = Request(context, url);
            var high = ulong.Parse(result.GetProperty("High").ToString(), CultureInfo.InvariantCulture);
            var low = ulong.Parse(result.GetProperty("Low").ToString(), CultureInfo.InvariantCulture);
            return permissions.All(permission =>
            {
                var bit = (int)permission - 1;
                return bit >= 0 && bit < 64 && (((bit < 32 ? low : high) >> (bit % 32)) & 1) != 0;
            });
        }

        internal static Guid GetAuthenticationRealm(this IWeb web)
        {
            SharePointOnlineSupport.Validate(web.PnPContext);
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(web.PnPContext.Uri, "/_vti_bin/client.svc")))
            {
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer");
                using (var response = web.PnPContext.RestClient.Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    foreach (var header in response.Headers.WwwAuthenticate)
                    {
                        var match = Regex.Match(header.ToString(), "realm=\"(?<realm>[^\"]+)\"", RegexOptions.IgnoreCase);
                        if (match.Success && Guid.TryParse(match.Groups["realm"].Value, out var realm)) return realm;
                    }
                }
            }
            throw new InvalidOperationException("SharePoint did not provide an authentication realm.");
        }
    }
}
