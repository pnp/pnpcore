using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Modernization.Transform
{
    internal sealed class ExternalImageImporter : BaseTransform
    {
        private readonly Uri webUrl;
        private readonly int pageId;
        private readonly string pageName;
        private readonly Func<HttpMethod, string, string> executeRequest;
        private readonly Dictionary<string, string> importedAssets = new Dictionary<string, string>(StringComparer.Ordinal);
        private bool folderLoaded;
        private string assetFolder;

        internal ExternalImageImporter(Uri webUrl, int pageId, string pageName,
            Func<HttpMethod, string, string> executeRequest, IList<ILogObserver> observers = null)
        {
            this.webUrl = webUrl;
            this.pageId = pageId;
            this.pageName = pageName;
            this.executeRequest = executeRequest;
            if (observers != null)
            {
                foreach (var observer in observers)
                {
                    RegisterObserver(observer);
                }
            }
        }

        internal string Import(Uri externalUrl)
        {
            if (pageId <= 0)
            {
                throw new InvalidDataException("External image ingestion requires a saved target page.");
            }
            if (externalUrl.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(externalUrl.UserInfo))
            {
                throw new InvalidDataException("External image ingestion requires an HTTPS URL without embedded credentials.");
            }

            var key = GetImageKey(externalUrl);
            if (importedAssets.TryGetValue(key, out var cachedPath))
            {
                return cachedPath;
            }
            LoadFolder();
            var existing = FindAsset(key);
            if (existing != null)
            {
                importedAssets[key] = existing;
                LogInfo($"Reusing external image asset '{existing}'.", LogStrings.Heading_ContentTransform);
                return existing;
            }

            // SharePoint owns the page's asset folder and infers the extension from the downloaded image.
            var request = Endpoint("AddImageFromExternalUrl") + PageParameters() +
                "&imageFileName=" + Literal(key) + "&externalUrl=" + Literal(externalUrl.AbsoluteUri) +
                "&$select=ServerRelativeUrl";
            using (var document = ParseResponse(executeRequest(HttpMethod.Post, request)))
            {
                var path = ReadString(document.RootElement, "ServerRelativeUrl");
                ValidatePath(path);
                var name = path.Substring(path.LastIndexOf('/') + 1);
                if (!name.Equals(key, StringComparison.OrdinalIgnoreCase) &&
                    !name.StartsWith(key + ".", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("The image service did not preserve the requested asset name.");
                }
                assetFolder = path.Substring(0, path.LastIndexOf('/'));
                importedAssets[key] = path;
                LogInfo($"Imported external image asset '{path}'.", LogStrings.Heading_ContentTransform);
                return path;
            }
        }

        internal static string GetImageKey(Uri externalUrl)
        {
            // The filename is a durable URL-to-file mapping scoped to the server-owned page folder.
            // Include the query string: signed URLs and image rendition parameters are significant.
            using (var sha = SHA256.Create())
            {
                return "pnp-external-" + BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(externalUrl.AbsoluteUri)))
                    .Replace("-", "").ToLowerInvariant();
            }
        }

        internal static bool IsStockImage(Uri imageUrl)
        {
            return imageUrl.Host.Equals("cdn.hubblecontent.osi.office.net", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadFolder()
        {
            if (folderLoaded)
            {
                return;
            }

            using (var document = ParseResponse(executeRequest(HttpMethod.Post,
                Endpoint("GetOrCreateAssetFolder") + PageParameters() + "&createFolderIfNeeded=false")))
            {
                assetFolder = ReadString(document.RootElement, "value");
            }
            if (!string.IsNullOrEmpty(assetFolder))
            {
                ValidatePath(assetFolder);
            }
            folderLoaded = true;
        }

        private string FindAsset(string key)
        {
            if (string.IsNullOrEmpty(assetFolder))
            {
                return null;
            }
            var filter = $"Name eq '{key}' or startswith(Name,'{key}.')";
            var request = webUrl.AbsoluteUri.TrimEnd('/') + "/_api/web/GetFolderByServerRelativePath(decodedurl=" +
                Literal(assetFolder) + ")/Files?$select=Name,ServerRelativeUrl&$top=2&$filter=" + Uri.EscapeDataString(filter);
            using (var document = ParseResponse(executeRequest(HttpMethod.Get, request)))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Object ||
                    !document.RootElement.TryGetProperty("value", out var files) || files.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidDataException("The image asset listing is not an array.");
                }
                if (files.GetArrayLength() > 1 || document.RootElement.TryGetProperty("odata.nextLink", out _) ||
                    document.RootElement.TryGetProperty("@odata.nextLink", out _))
                {
                    throw new InvalidDataException("More than one imported asset matches the external image URL.");
                }
                foreach (var file in files.EnumerateArray())
                {
                    var name = ReadString(file, "Name");
                    var path = ReadString(file, "ServerRelativeUrl");
                    ValidatePath(path);
                    if ((!name.Equals(key, StringComparison.OrdinalIgnoreCase) &&
                         !name.StartsWith(key + ".", StringComparison.OrdinalIgnoreCase)) ||
                        !path.Equals(assetFolder.TrimEnd('/') + "/" + name, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException("An image asset does not match the page folder and URL key.");
                    }
                    return path;
                }
                return null;
            }
        }

        private string Endpoint(string operation)
        {
            return webUrl.AbsoluteUri.TrimEnd('/') + "/_api/sitepages/" + operation;
        }

        private string PageParameters()
        {
            return "?pageName=" + Literal(pageName) + "&pageId=" + pageId.ToString(CultureInfo.InvariantCulture);
        }

        private static string Literal(string value)
        {
            return "'" + Uri.EscapeDataString(value.Replace("'", "''")) + "'";
        }

        private static void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/", StringComparison.Ordinal) ||
                path.StartsWith("//", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The image service did not return a server-relative path.");
            }
        }

        private static JsonDocument ParseResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new InvalidDataException("The image service returned an empty response.");
            }
            try
            {
                return JsonDocument.Parse(response);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("The image service returned invalid JSON.", ex);
            }
        }

        private static string ReadString(JsonElement element, string property)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value) ||
                value.ValueKind != JsonValueKind.String)
            {
                throw new InvalidDataException($"The image service response is missing '{property}'.");
            }
            return value.GetString();
        }
    }
}
