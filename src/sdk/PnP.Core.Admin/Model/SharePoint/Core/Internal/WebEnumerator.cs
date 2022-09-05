using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class WebEnumerator
    {
        internal async static Task<List<IWebWithDetails>> GetWithDetailsAsync(PnPContext context, Uri url, bool skipAppWebs)
        {
            if (url == null)
            {
                url = context.Uri;
            }

            if (context.Uri != url)
            {
                using (var newContext = await context.CloneAsync(url).ConfigureAwait(false))
                {
                    return await GetWithDetailsImplementationAsync(newContext, skipAppWebs).ConfigureAwait(false);
                }
            }
            else
            {
                return await GetWithDetailsImplementationAsync(context, skipAppWebs).ConfigureAwait(false);
            }
        }

        private async static Task<List<IWebWithDetails>> GetWithDetailsImplementationAsync(PnPContext context, bool skipAppWebs)
        {
            List<IWebWithDetails> list = new List<IWebWithDetails>();

            var result = await (context.Web as Web).RawRequestAsync(BuildWebEnumerationApiCall(""), HttpMethod.Get).ConfigureAwait(false);

            await ProcessWebsAsync(context, list, result, skipAppWebs).ConfigureAwait(false);

            return list;
        }

        private static async Task ProcessWebsAsync(PnPContext context, List<IWebWithDetails> list, ApiCallResponse result, bool skipAppWebs)
        {
            var webList = JsonDocument.Parse(result.Json).RootElement.GetProperty("value");
            if (webList.ValueKind == JsonValueKind.Array)
            {
                foreach (var web in webList.EnumerateArray())
                {
                    string webTemplateConfiguration = $"{web.GetProperty("WebTemplate").GetString()}#{web.GetProperty("WebTemplateId").GetInt32()}";

                    if (skipAppWebs && webTemplateConfiguration.Equals("APP#0", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var webWithDetails = new WebWithDetails
                    {
                        Id = web.GetProperty("Id").GetGuid(),
                        ServerRelativeUrl = web.GetProperty("ServerRelativeUrl").GetString(),
                        Url = new Uri($"https://{context.Uri.DnsSafeHost}{web.GetProperty("ServerRelativeUrl").GetString()}"),
                        WebTemplate = web.GetProperty("WebTemplate").GetString(),
                        WebTemplateConfiguration = webTemplateConfiguration,
                        Language = (Language)web.GetProperty("Language").GetInt32(),
                        TimeCreated = web.GetProperty("Created").GetDateTime(),
                        LastItemModifiedDate = web.GetProperty("LastItemModifiedDate").GetDateTime(),
                        LastItemUserModifiedDate = web.GetProperty("LastItemUserModifiedDate").GetDateTime(),
                    };

                    if (web.TryGetProperty("Title", out JsonElement webTitle))
                    {
                        webWithDetails.Title = webTitle.GetString();
                    }

                    if (web.TryGetProperty("Description", out JsonElement webDescription))
                    {
                        webWithDetails.Description = webDescription.GetString();
                    }

                    list.Add(webWithDetails);

                    // Get sub sites for the current sub site, fully qualify them as we're making the call with
                    // the initial PnPContext instance. Creating new PnPContext objects for each web will impact 
                    // performance
                    result = await (context.Web as Web).RawRequestAsync(BuildWebEnumerationApiCall(webWithDetails.Url.ToString()), HttpMethod.Get).ConfigureAwait(false);
                    await ProcessWebsAsync(context, list, result, skipAppWebs).ConfigureAwait(false);
                }
            }
        }

        private static ApiCall BuildWebEnumerationApiCall(string urlSuffix)
        {
            if (!string.IsNullOrEmpty(urlSuffix) && !urlSuffix.EndsWith("/"))
            {
                urlSuffix += "/";
            }

            return new ApiCall($"{urlSuffix}_api/web/getsubwebsfilteredforcurrentuser(nWebTemplateFilter=-1,nConfigurationFilter=-1)", ApiType.SPORest);
        }
    }
}
