using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Test.Live.Diagnostics
{
    /// <summary>
    /// Prints what the <c>SiteScriptUtility</c> REST endpoints actually answer with.
    /// </summary>
    [TestClass]
    public class SiteScriptEndpointProbeTests : LiveTestBase
    {
        private const string Prefix = "_api/Microsoft.SharePoint.Utilities.WebTemplateExtensions.SiteScriptUtility.";

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task WhatDoesGetSiteScriptsAnswerWith()
        {
            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    foreach (string method in new[] { "GetSiteScripts", "GetSiteDesigns" })
                    {
                        try
                        {
                            ApiRequestResponse response = await admin.Web.ExecuteRequestAsync(new ApiRequest(
                                HttpMethod.Post, ApiRequestType.SPORest, $"{Prefix}{method}", "{}"))
                                .ConfigureAwait(false);

                            Console.WriteLine($"===== {method} =====");
                            Console.WriteLine(Truncate(response.Response));
                            Console.WriteLine($"Root kind: {RootKind(response.Response)}");
                            Console.WriteLine();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"===== {method} FAILED =====");
                            Console.WriteLine(Describe(ex));
                        }
                    }
                }
            }
        }

        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task WhatDoesCreateSiteScriptAnswerWith()
        {
            string title = $"{TestPrefix}Probe_{Guid.NewGuid():N}";

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                PnPContext admin;

                try
                {
                    admin = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SkipIfUnavailable("The tenant admin site", ex);
                    return;
                }

                using (admin)
                {
                    string createUrl = $"{Prefix}CreateSiteScript(Title=@title,Description=@desc)" +
                        $"?@title='{title}'&@desc='probe'";

                    string content = "{\"$schema\":\"schema.json\",\"actions\":[" +
                        "{\"verb\":\"applyTheme\",\"themeName\":\"Blue\"}],\"bindata\":{},\"version\":1}";

                    string createdId = null;

                    try
                    {
                        ApiRequestResponse response = await admin.Web.ExecuteRequestAsync(new ApiRequest(
                            HttpMethod.Post, ApiRequestType.SPORest, createUrl, content)).ConfigureAwait(false);

                        Console.WriteLine("===== CreateSiteScript =====");
                        Console.WriteLine(Truncate(response.Response));
                        Console.WriteLine($"Root kind: {RootKind(response.Response)}");

                        createdId = ReadId(response.Response);
                        Console.WriteLine($"Parsed id: {createdId ?? "<none>"}");

                        Assert.IsFalse(string.IsNullOrEmpty(createdId),
                            "CreateSiteScript answered without an id anywhere in its response.");
                    }
                    catch (AssertInconclusiveException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"===== CreateSiteScript FAILED ====={Environment.NewLine}{Describe(ex)}");
                        throw;
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(createdId) && Guid.TryParse(createdId, out Guid id))
                        {
                            try
                            {
                                await admin.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post,
                                    ApiRequestType.SPORest, $"{Prefix}DeleteSiteScript",
                                    $"{{\"id\":\"{id}\"}}")).ConfigureAwait(false);

                                Console.WriteLine($"Deleted probe script {id}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"COULD NOT DELETE PROBE SCRIPT {id}: {Describe(ex)}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prints what the storage entity endpoints on the app catalog answer with.
        /// </summary>
        // Online test - requires a tenant
        [Ignore]
        [TestMethod]
        [TestCategory("Live")]
        [TestCategory("Diagnostics")]
        public async Task WhatDoTheStorageEntityEndpointsAnswerWith()
        {
            string key = $"{TestPrefix}Probe_{Guid.NewGuid():N}";

            using (PnPContext context = await GetContextAsync().ConfigureAwait(false))
            {
                Uri catalogUri = await context.GetTenantAppManager().GetTenantAppCatalogUriAsync().ConfigureAwait(false);

                if (catalogUri == null)
                {
                    Assert.Inconclusive("This tenant has no app catalog.");
                }

                Console.WriteLine($"App catalog: {catalogUri}");

                using (PnPContext catalog = await context.CloneAsync(catalogUri).ConfigureAwait(false))
                {
                    Console.WriteLine($"NoScript: {await catalog.Web.IsNoScriptSiteAsync().ConfigureAwait(false)}");

                    try
                    {
                        await ProbeAsync(catalog, "SET", HttpMethod.Post,
                            $"_api/web/SetStorageEntity(key='{key}',value='provisioned'," +
                            $"description='probe',comments='probe')", "{}").ConfigureAwait(false);

                        await ProbeAsync(catalog, "GET (same context)", null,
                            $"_api/web/GetStorageEntity('{key}')", null).ConfigureAwait(false);

                        using (PnPContext fresh = await GetContextAsync(1).ConfigureAwait(false))
                        using (PnPContext freshCatalog = await fresh.CloneAsync(catalogUri).ConfigureAwait(false))
                        {
                            await ProbeAsync(freshCatalog, "GET (fresh context)", null,
                                $"_api/web/GetStorageEntity('{key}')", null).ConfigureAwait(false);

                            await ProbeAsync(freshCatalog, "GET via TenantProperties", null,
                                "_api/web/AllProperties?$select=" + Uri.EscapeDataString($"PnPCoreProbe"),
                                null).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        try
                        {
                            await catalog.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post,
                                ApiRequestType.SPORest, $"_api/web/RemoveStorageEntity(key='{key}')", "{}"))
                                .ConfigureAwait(false);

                            Console.WriteLine($"Removed probe entity {key}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"COULD NOT REMOVE PROBE ENTITY: {Describe(ex)}");
                        }
                    }
                }
            }
        }

        private static async Task ProbeAsync(PnPContext context, string label, HttpMethod method,
            string url, string body)
        {
            Console.WriteLine($"===== {label} =====");

            try
            {
                ApiRequest request = method == null
                    ? new ApiRequest(ApiRequestType.SPORest, url)
                    : new ApiRequest(method, ApiRequestType.SPORest, url, body ?? "{}");

                ApiRequestResponse response = await context.Web.ExecuteRequestAsync(request).ConfigureAwait(false);

                Console.WriteLine(Truncate(response.Response));
                Console.WriteLine($"Root kind: {RootKind(response.Response)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {Describe(ex)}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Finds an <c>Id</c> anywhere in the response, whatever wraps it.
        /// </summary>
        private static string ReadId(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                return FindId(document.RootElement);
            }
        }

        private static string FindId(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase)
                        && property.Value.ValueKind == JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }

                    string nested = FindId(property.Value);

                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string nested = FindId(item);

                    if (nested != null)
                    {
                        return nested;
                    }
                }
            }

            return null;
        }

        private static string RootKind(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "<empty>";
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return document.RootElement.ValueKind.ToString();
                }

                var names = new System.Collections.Generic.List<string>();

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    names.Add($"{property.Name} ({property.Value.ValueKind})");
                }

                return string.Join(", ", names);
            }
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            return value.Length <= 2000 ? value : value.Substring(0, 2000) + "… [truncated]";
        }
    }
}
