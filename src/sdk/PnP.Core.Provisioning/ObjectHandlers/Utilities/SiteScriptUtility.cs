using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Site script and site design CRUD, over the <c>SiteScriptUtility</c> REST endpoints.
    /// </summary>
    internal static class SiteScriptUtility
    {
        private const string Prefix = "_api/Microsoft.SharePoint.Utilities.WebTemplateExtensions.SiteScriptUtility.";

        #region Site scripts

        internal static async Task<List<SiteScriptMetadata>> GetSiteScriptsAsync(PnPContext admin)
        {
            return await PostListAsync(admin, "GetSiteScripts", null, SiteScriptMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task<SiteScriptMetadata> CreateSiteScriptAsync(PnPContext admin,
            string title, string description, string content)
        {
            // Title travels in the query string, not the body - that is how the endpoint is defined,
            // and putting it in the body is accepted and then ignored.
            string url = $"{Prefix}CreateSiteScript(Title=@title,Description=@desc)" +
                $"?@title='{Escape(title)}'&@desc='{Escape(description)}'";

            return await PostAsync(admin, url, content, SiteScriptMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task<SiteScriptMetadata> UpdateSiteScriptAsync(PnPContext admin,
            Guid id, string title, string description, string content)
        {
            var body = new Dictionary<string, object>
            {
                ["updateInfo"] = new Dictionary<string, object>
                {
                    ["Id"] = id.ToString(),
                    ["Title"] = title,
                    ["Description"] = description,
                    ["Content"] = content,
                },
            };

            return await PostAsync(admin, $"{Prefix}UpdateSiteScript",
                JsonSerializer.Serialize(body), SiteScriptMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task DeleteSiteScriptAsync(PnPContext admin, Guid id)
        {
            await PostRawAsync(admin, $"{Prefix}DeleteSiteScript",
                JsonSerializer.Serialize(new Dictionary<string, object> { ["id"] = id.ToString() }))
                .ConfigureAwait(false);
        }

        #endregion

        #region Site designs

        internal static async Task<List<SiteDesignMetadata>> GetSiteDesignsAsync(PnPContext admin)
        {
            return await PostListAsync(admin, "GetSiteDesigns", null, SiteDesignMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task<SiteDesignMetadata> CreateSiteDesignAsync(PnPContext admin, SiteDesignInfo info)
        {
            var body = new Dictionary<string, object> { ["info"] = info.ToPayload() };

            return await PostAsync(admin, $"{Prefix}CreateSiteDesign",
                JsonSerializer.Serialize(body), SiteDesignMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task<SiteDesignMetadata> UpdateSiteDesignAsync(PnPContext admin, Guid id, SiteDesignInfo info)
        {
            Dictionary<string, object> payload = info.ToPayload();
            payload["Id"] = id.ToString();

            var body = new Dictionary<string, object> { ["updateInfo"] = payload };

            return await PostAsync(admin, $"{Prefix}UpdateSiteDesign",
                JsonSerializer.Serialize(body), SiteDesignMetadata.Read).ConfigureAwait(false);
        }

        internal static async Task DeleteSiteDesignAsync(PnPContext admin, Guid id)
        {
            await PostRawAsync(admin, $"{Prefix}DeleteSiteDesign",
                JsonSerializer.Serialize(new Dictionary<string, object> { ["id"] = id.ToString() }))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Grants principals a right on a site design.
        /// </summary>
        internal static async Task GrantSiteDesignRightsAsync(PnPContext admin, Guid id,
            IEnumerable<string> principalNames, int right)
        {
            var body = new Dictionary<string, object>
            {
                ["id"] = id.ToString(),
                ["principalNames"] = VerboseOData.Collection(principalNames),
                ["grantedRights"] = right.ToString(CultureInfo.InvariantCulture),
            };

            await PostRawAsync(admin, $"{Prefix}GrantSiteDesignRights",
                JsonSerializer.Serialize(body)).ConfigureAwait(false);
        }

        #endregion

        #region Plumbing

        private static async Task<List<T>> PostListAsync<T>(PnPContext admin, string method, string body,
            Func<JsonElement, T> read)
        {
            string json = await PostRawAsync(admin, $"{Prefix}{method}", body).ConfigureAwait(false);

            var items = new List<T>();

            if (string.IsNullOrEmpty(json))
            {
                return items;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement payload = VerboseOData.Unwrap(document.RootElement);

                if (payload.ValueKind != JsonValueKind.Array)
                {
                    return items;
                }

                foreach (JsonElement item in payload.EnumerateArray())
                {
                    items.Add(read(item));
                }
            }

            return items;
        }

        private static async Task<T> PostAsync<T>(PnPContext admin, string url, string body,
            Func<JsonElement, T> read)
        {
            string json = await PostRawAsync(admin, url, body).ConfigureAwait(false);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                return read(VerboseOData.Unwrap(document.RootElement));
            }
        }

        private static async Task<string> PostRawAsync(PnPContext admin, string url, string body)
        {
            ApiRequestResponse response = await admin.Web.ExecuteRequestAsync(
                new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, url, body ?? "{}")).ConfigureAwait(false);

            return response.Response;
        }

        /// <summary>
        /// Escapes a value going into an OData string literal.
        /// </summary>
        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        #endregion

        #region Payloads

        /// <summary>
        /// What a site design create or update needs, before it becomes JSON.
        /// </summary>
        internal sealed class SiteDesignInfo
        {
            internal string Title { get; set; }

            internal string Description { get; set; }

            internal string PreviewImageUrl { get; set; }

            internal string PreviewImageAltText { get; set; }

            internal bool IsDefault { get; set; }

            /// <summary>64 for a team site, 68 for a communication site, 1 for a group-less team site.</summary>
            internal string WebTemplate { get; set; }

            internal List<string> SiteScriptIds { get; } = new List<string>();

            internal Dictionary<string, object> ToPayload()
            {
                return new Dictionary<string, object>
                {
                    ["Title"] = Title,
                    ["Description"] = Description,
                    ["PreviewImageUrl"] = PreviewImageUrl,
                    ["PreviewImageAltText"] = PreviewImageAltText,
                    ["IsDefault"] = IsDefault,
                    ["WebTemplate"] = WebTemplate,
                    ["SiteScriptIds"] = VerboseOData.Collection(SiteScriptIds),
                };
            }
        }

        internal sealed class SiteScriptMetadata
        {
            public Guid Id { get; set; }

            public string Title { get; set; }

            public string Description { get; set; }

            public string Content { get; set; }

            internal static SiteScriptMetadata Read(JsonElement element)
            {
                return new SiteScriptMetadata
                {
                    Id = VerboseOData.GuidOf(element, "Id"),
                    Title = VerboseOData.StringOf(element, "Title"),
                    Description = VerboseOData.StringOf(element, "Description"),
                    Content = VerboseOData.StringOf(element, "Content"),
                };
            }
        }

        internal sealed class SiteDesignMetadata
        {
            public Guid Id { get; set; }

            public string Title { get; set; }

            public string Description { get; set; }

            public bool IsDefault { get; set; }

            public string PreviewImageUrl { get; set; }

            public string PreviewImageAltText { get; set; }

            public string WebTemplate { get; set; }

            public List<Guid> SiteScriptIds { get; } = new List<Guid>();

            /// <summary>
            /// Reads a design, whose <c>SiteScriptIds</c> is a verbose OData collection.
            /// </summary>
            internal static SiteDesignMetadata Read(JsonElement element)
            {
                var design = new SiteDesignMetadata
                {
                    Id = VerboseOData.GuidOf(element, "Id"),
                    Title = VerboseOData.StringOf(element, "Title"),
                    Description = VerboseOData.StringOf(element, "Description"),
                    IsDefault = VerboseOData.BoolOf(element, "IsDefault"),
                    PreviewImageUrl = VerboseOData.StringOf(element, "PreviewImageUrl"),
                    PreviewImageAltText = VerboseOData.StringOf(element, "PreviewImageAltText"),
                    WebTemplate = VerboseOData.StringOf(element, "WebTemplate"),
                };

                foreach (JsonElement scriptId in VerboseOData.CollectionOf(element, "SiteScriptIds"))
                {
                    if (scriptId.ValueKind == JsonValueKind.String
                        && Guid.TryParse(scriptId.GetString(), out Guid id))
                    {
                        design.SiteScriptIds.Add(id);
                    }
                }

                return design;
            }
        }

        #endregion
    }
}
