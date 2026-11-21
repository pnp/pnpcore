

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services
{
    internal enum ClassicWebPartExportMode
    {
        None = 0,
        All = 1,
        NonSensitiveData = 2
    }

    internal sealed class ClassicWebPartDefinition
    {
        internal Guid Id { get; set; }
        internal string ZoneId { get; set; }
        internal ClassicWebPart WebPart { get; set; }
    }

    internal sealed class ClassicWebPart
    {
        internal string Title { get; set; }
        internal int ZoneIndex { get; set; }
        internal bool Hidden { get; set; }
        internal bool IsClosed { get; set; }
        internal ClassicWebPartExportMode ExportMode { get; set; }
        internal Dictionary<string, object> Properties { get; set; }
    }

    internal sealed class ClassicWebPartReader
    {
        private readonly IFile file;
        private List<ClassicWebPartDefinition> definitions;
        private readonly Dictionary<Guid, string> exports = new Dictionary<Guid, string>();

        internal ClassicWebPartReader(IFile file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file));
            SharePointOnlineSupport.Validate(file.PnPContext);
        }

        internal IReadOnlyList<ClassicWebPartDefinition> Read() => ReadAsync().GetAwaiter().GetResult();

        internal async Task<IReadOnlyList<ClassicWebPartDefinition>> ReadAsync()
        {
            if (definitions != null)
            {
                return definitions;
            }
            await file.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);
            var request = ManagerUrl() + "/WebParts?$expand=WebPart,WebPart/Properties" +
                "&$select=Id,ZoneId,WebPart/Title,WebPart/ZoneIndex,WebPart/Hidden,WebPart/IsClosed,WebPart/ExportMode,WebPart/Properties";
            var result = new List<ClassicWebPartDefinition>();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (request != null)
            {
                if (!visited.Add(request))
                {
                    throw new InvalidOperationException("SharePoint returned a repeated web-part continuation URL.");
                }
                var response = await file.PnPContext.Web.ExecuteRequestAsync(new ApiRequest(
                    HttpMethod.Get, ApiRequestType.SPORest, request, null, JsonHeaders())).ConfigureAwait(false);
                using (var json = JsonDocument.Parse(response.Response))
                {
                    foreach (var item in json.RootElement.GetProperty("value").EnumerateArray())
                    {
                        var part = item.GetProperty("WebPart");
                        result.Add(new ClassicWebPartDefinition
                        {
                            Id = item.GetProperty("Id").GetGuid(),
                            ZoneId = item.GetProperty("ZoneId").GetString(),
                            WebPart = new ClassicWebPart
                            {
                                Title = part.GetProperty("Title").GetString(),
                                ZoneIndex = part.GetProperty("ZoneIndex").GetInt32(),
                                Hidden = part.GetProperty("Hidden").GetBoolean(),
                                IsClosed = part.GetProperty("IsClosed").GetBoolean(),
                                ExportMode = (ClassicWebPartExportMode)part.GetProperty("ExportMode").GetInt32(),
                                Properties = part.GetProperty("Properties").EnumerateObject()
                                    .ToDictionary(property => property.Name, property => ReadValue(property.Value))
                            }
                        });
                    }
                    request = json.RootElement.TryGetProperty("odata.nextLink", out var next) ? next.GetString() : null;
                    if (request != null)
                    {
                        var nextUri = new Uri(request, UriKind.Absolute);
                        var site = file.PnPContext.Uri;
                        if (nextUri.Scheme != site.Scheme || nextUri.Host != site.Host || nextUri.Port != site.Port ||
                            !nextUri.AbsolutePath.StartsWith(site.AbsolutePath.TrimEnd('/') + "/_api/", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("SharePoint returned an out-of-scope web-part continuation URL.");
                        }
                    }
                }
            }
            definitions = result;
            return definitions;
        }

        internal string Export(Guid id) => ExportAsync(id).GetAwaiter().GetResult();

        internal async Task<string> ExportAsync(Guid id)
        {
            if (exports.TryGetValue(id, out var cached))
            {
                return cached;
            }
            var part = (await ReadAsync().ConfigureAwait(false)).Single(p => p.Id == id);
            if (part.WebPart.ExportMode != ClassicWebPartExportMode.All)
            {
                throw new InvalidOperationException("The source web part does not permit a full XML export.");
            }
            var body = JsonSerializer.Serialize(new { webPartId = id.ToString() });
            var response = await file.PnPContext.Web.ExecuteRequestAsync(new ApiRequest(
                HttpMethod.Post, ApiRequestType.SPORest, ManagerUrl() + "/ExportWebPart", body, JsonHeaders())).ConfigureAwait(false);
            using (var json = JsonDocument.Parse(response.Response))
            {
                var xml = json.RootElement.GetProperty("value").GetString();
                if (string.IsNullOrWhiteSpace(xml))
                {
                    throw new InvalidOperationException("SharePoint returned an empty web-part XML export.");
                }
                exports.Add(id, xml);
                return xml;
            }
        }

        private string ManagerUrl() =>
            $"_api/web/GetFileByServerRelativePath(decodedUrl='{Uri.EscapeDataString(file.ServerRelativeUrl.Replace("'", "''"))}')/GetLimitedWebPartManager(scope=1)";

        private static Dictionary<string, string> JsonHeaders() => new Dictionary<string, string>
        {
            ["Accept"] = "application/json;odata=nometadata"
        };

        private static object ReadValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Null: return null;
                case JsonValueKind.String: return value.GetString();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out int integer)) return integer;
                    if (value.TryGetInt64(out long largeInteger)) return largeInteger;
                    return value.GetDouble();
                case JsonValueKind.Array: return value.EnumerateArray().Select(ReadValue).ToArray();
                case JsonValueKind.Object:
                    return value.EnumerateObject().ToDictionary(property => property.Name, property => ReadValue(property.Value));
                default: throw new JsonException("Unsupported web-part property value.");
            }
        }
    }
}
