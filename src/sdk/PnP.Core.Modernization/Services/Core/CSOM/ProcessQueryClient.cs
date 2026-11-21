
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Services.Core.CSOM
{
    internal static class ProcessQueryClient
    {
        internal static readonly XNamespace Namespace = "http://schemas.microsoft.com/sharepoint/clientquery/2009";

        internal static async Task<JsonElement> ExecuteAsync(PnPContext context, XElement query, CancellationToken cancellationToken = default)
        {
            SharePointOnlineSupport.Validate(context);
            if (query == null || query.Name != Namespace + "Request")
            {
                throw new ArgumentException("A clientquery Request element is required.", nameof(query));
            }
            var endpoint = new Uri(context.Uri.AbsoluteUri.TrimEnd('/') + "/_vti_bin/client.svc/ProcessQuery");
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Content = new StringContent(query.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml");
                await context.AuthenticationProvider.AuthenticateRequestAsync(context.Uri, request).ConfigureAwait(false);
                using (var response = await context.RestClient.Client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false)))
                    {
                        var result = json.RootElement;
                        if (result.ValueKind != JsonValueKind.Array || result.GetArrayLength() == 0 ||
                            !result[0].TryGetProperty("ErrorInfo", out var error))
                        {
                            throw new InvalidDataException("SharePoint returned an invalid ProcessQuery response.");
                        }
                        if (error.ValueKind != JsonValueKind.Null)
                        {
                            throw new CsomServiceException(ErrorType.CsomServiceError, (int)response.StatusCode, result[0]);
                        }
                        return result.Clone();
                    }
                }
            }
        }

        internal static JsonElement GetResult(JsonElement response, int actionId)
        {
            for (int index = 1; index + 1 < response.GetArrayLength(); index += 2)
            {
                if (response[index].ValueKind == JsonValueKind.Number && response[index].GetInt32() == actionId)
                {
                    return response[index + 1];
                }
            }
            throw new InvalidDataException($"SharePoint did not return ProcessQuery action {actionId}.");
        }
    }
}
