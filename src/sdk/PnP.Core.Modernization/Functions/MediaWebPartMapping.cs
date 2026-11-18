using AngleSharp.Html.Parser;
using Newtonsoft.Json.Linq;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PnP.Core.Modernization.Functions
{
    internal sealed class MediaWebPartMapping : FunctionsBase
    {
        private readonly PnPContext sourceClientContext;
        private readonly IListItem sourcePage;

        internal MediaWebPartMapping(PnPContext targetContext, PnPContext sourceContext,
            IListItem sourcePage, IList<ILogObserver> logObservers) : base(targetContext)
        {
            this.sourceClientContext = sourceContext;
            this.sourcePage = sourcePage;
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    RegisterObserver(observer);
                }
            }
        }

        internal Dictionary<string, string> ResolveConfiguration(string mediaSource, string embedCode,
            string videoSetSource, bool configureFromContext)
        {
            PnPContext source = this.sourceClientContext ?? this.clientContext;
            IListItem item = null;
            PnPContext itemContext = source;
            try
            {
                if (configureFromContext)
                {
                    item = this.sourcePage;
                    if (item == null)
                    {
                        throw MediaUnavailable("Media web part requires a source page item for ConfigureFromContext.");
                    }
                }
                else if (string.IsNullOrWhiteSpace(mediaSource) && string.IsNullOrWhiteSpace(embedCode) &&
                    !string.IsNullOrWhiteSpace(videoSetSource))
                {
                    var address = ResolveMediaUrl(videoSetSource, source);
                    RequireSharePointMediaOrigin(address, source);
                    itemContext = GetMediaWebContext(source, address.AbsoluteUri);
                    item = itemContext.Web.GetFolderByServerRelativeUrl(Uri.UnescapeDataString(address.AbsolutePath), f => f.ListItemAllFields).ListItemAllFields;
                    item.LoadBatch();
                    itemContext.Execute();
                }

                if (item != null)
                {
                    if (GetMediaField(item, "ContentTypeId").StartsWith("0x0120D520A808", StringComparison.OrdinalIgnoreCase))
                    {
                        // Classic VideoSet order: user override, default encoding, external link, then embed.
                        mediaSource = new[] { "VideoSetUserOverrideEncoding", "VideoSetDefaultEncoding", "VideoSetExternalLink" }
                            .Select(field => GetMediaField(item, field)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
                        if (string.IsNullOrWhiteSpace(mediaSource))
                        {
                            embedCode = GetMediaField(item, "VideoSetEmbedCode");
                        }
                    }
                    else if (configureFromContext)
                    {
                        mediaSource = GetMediaField(item, Constants.FileRefField);
                    }
                    else
                    {
                        throw MediaUnavailable("Media web part VideoSetSource is not a video document set.");
                    }
                }

                // As in classic rendering, configured embed code wins over a media file.
                if (!string.IsNullOrWhiteSpace(embedCode))
                {
                    return new Dictionary<string, string>
                    {
                        ["MediaMapping"] = "EMBED",
                        ["MediaEmbedJson"] = MediaToEmbedProperties(embedCode, source)
                    };
                }

                var media = ResolveMediaUrl(mediaSource, source);
                RequireSharePointMediaOrigin(media, source);
                var extension = Path.GetExtension(media.AbsolutePath);
                var supportedExtensions = new[] { ".3g2", ".3gp", ".3gpp", ".asf", ".avi", ".m4v", ".mkv", ".mov",
                    ".mp4", ".mp4v", ".mts", ".ts", ".webm", ".wmv", ".m4a", ".mp3", ".wav", ".wma", ".m2ts" };
                if (!supportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
                    media.AbsolutePath.IndexOf("/_layouts/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw MediaUnavailable($"Media web part source '{mediaSource}' is not a supported media file (placeholder media is not migrated).");
                }
                return new Dictionary<string, string>
                {
                    ["MediaMapping"] = "FILE",
                    ["ResolvedMediaSource"] = media.AbsolutePath
                };
            }
            catch (ServiceException ex) when (ex.IsNotFound())
            {
                throw MediaUnavailable($"Media web part VideoSetSource '{videoSetSource}' was not found or is inaccessible.");
            }
            finally
            {
                if (itemContext != source)
                {
                    itemContext.Dispose();
                }
            }
        }

        internal string ToFileProperties(string resolvedMediaSource, string transferredMediaSource)
        {
            PnPContext source = this.sourceClientContext ?? this.clientContext;
            var retainedSource = string.Equals(resolvedMediaSource, transferredMediaSource, StringComparison.Ordinal);
            PnPContext context = retainedSource ? source : this.clientContext;
            var path = retainedSource ? Uri.UnescapeDataString(resolvedMediaSource) : transferredMediaSource;
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/", StringComparison.Ordinal) ||
                path.StartsWith("//", StringComparison.Ordinal))
            {
                throw MediaUnavailable("Media web part asset handling did not return a file path.");
            }
            if (retainedSource && !source.Uri.AbsoluteUri.Equals(this.clientContext.Uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                LogWarning($"Media file '{path}' was not transferred; retaining its source location and identities. Verify destination users have access.", LogStrings.Heading_BuiltInFunctions);
            }
            var fileUrl = new Uri(context.Uri.AbsoluteUri).GetLeftPart(UriPartial.Authority) +
                string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
            PnPContext fileContext = context;
            try
            {
                fileContext = GetMediaWebContext(context, fileUrl);
                IFile file = fileContext.Web.GetFileByServerRelativeUrl(path);
                file.LoadBatch(value => value.Name,value => value.ServerRelativeUrl, value => value.UniqueId,value => value.ListId,value => value.Author);
                fileContext.Web.LoadBatch(value => value.Id,value => value.Url);
                fileContext.Site.LoadBatch(value => value.Id);
                fileContext.Execute();
                if (file.UniqueId == Guid.Empty || file.ListId == Guid.Empty)
                {
                    throw MediaUnavailable($"Media file '{fileUrl}' could not be resolved to a SharePoint file.");
                }
                var data = new JObject
                {
                    ["serverProcessedContent"] = new JObject
                    {
                        ["htmlStrings"] = new JObject(),
                        ["searchablePlainTexts"] = new JObject { ["annotation"] = "", ["title"] = file.Name },
                        ["imageSources"] = new JObject(),
                        ["links"] = new JObject { ["serverRelativeUrl"] = file.ServerRelativeUrl, ["wopiurl"] = "" }
                    },
                    ["dataVersion"] = "1.4",
                    ["properties"] = new JObject
                    {
                        ["file"] = fileContext.Web.Url.GetLeftPart(UriPartial.Authority) +
                            string.Join("/", file.ServerRelativeUrl.Split('/').Select(Uri.EscapeDataString)),
                        ["serverRelativeUrl"] = file.ServerRelativeUrl,
                        ["title"] = file.Name,
                        ["siteId"] = fileContext.Site.Id.ToString(),
                        ["webId"] = fileContext.Web.Id.ToString(),
                        ["webAbsoluteUrl"] = fileContext.Web.Url,
                        ["listId"] = file.ListId.ToString(),
                        ["uniqueId"] = file.UniqueId.ToString(),
                        ["authorName"] = file.Author?.Title ?? "",
                        ["photoUrl"] = "",
                        ["startPage"] = 1,
                        ["wdallowinteractivity"] = true,
                        ["wdhidegridlines"] = true,
                        ["wdhideheaders"] = true
                    }
                };
                return EncodeWebPartMappingJson(data);
            }
            catch (ServiceException ex) when (ex.IsNotFound())
            {
                throw MediaUnavailable($"Media file '{fileUrl}' was not found or is inaccessible.");
            }
            finally
            {
                if (fileContext != context)
                {
                    fileContext.Dispose();
                }
            }
        }

        private static string GetMediaField(IListItem item, string name)
        {
            if (!item.Values.TryGetValue(name, out var value) || value == null)
            {
                return "";
            }
            return value is IFieldUrlValue url ? url.Url : value.ToString();
        }

        private Uri ResolveMediaUrl(string value, PnPContext source)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl) || value.Contains("\\") ||
                !Uri.TryCreate(new Uri(source.Web.GetUrl().TrimEnd('/') + "/"), value.Trim(), out var address) ||
                !IsHttpUrl(address))
            {
                throw MediaUnavailable("Media web part requires a valid HTTP(S) media URL or iframe embed code. No source was inferred.");
            }
            return address;
        }

        private void RequireSharePointMediaOrigin(Uri address, PnPContext source)
        {
            if (!address.GetLeftPart(UriPartial.Authority).Equals(new Uri(source.Web.GetUrl()).GetLeftPart(UriPartial.Authority),
                StringComparison.OrdinalIgnoreCase))
            {
                throw MediaUnavailable($"Media source '{address.AbsoluteUri}' is not on the source SharePoint host. External media requires supported iframe embed code.");
            }
        }

        private PnPContext GetMediaWebContext(PnPContext context, string fileUrl)
        {
            var webUrl = SharePointRest.GetWebUrlFromPageUrl(context, new Uri(fileUrl));
            if (string.IsNullOrWhiteSpace(webUrl))
            {
                throw MediaUnavailable($"The SharePoint web for media '{fileUrl}' could not be resolved.");
            }
            return webUrl.Equals(context.Uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase) ? context : context.Clone(new Uri(webUrl));
        }

        private string MediaToEmbedProperties(string embedCode, PnPContext source)
        {
            using (var document = new HtmlParser().ParseDocument(embedCode))
            {
                var iframe = document.Body.FirstElementChild;
                if (document.Head.Children.Length != 0 || document.Body.Children.Length != 1 ||
                    iframe?.LocalName != "iframe" || !string.IsNullOrWhiteSpace(document.Body.TextContent) ||
                    iframe.Attributes.Any(attribute => attribute.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) ||
                        attribute.Name.Equals("srcdoc", StringComparison.OrdinalIgnoreCase)))
                {
                    throw MediaUnavailable("Media embed code must contain only an iframe, without script, event handlers or srcdoc.");
                }
                var address = ResolveMediaUrl(iframe.GetAttribute("src"), source);
                if (address.Scheme != Uri.UriSchemeHttps)
                {
                    throw MediaUnavailable("The modern Embed web part requires an HTTPS iframe source.");
                }
                // Rebuild a minimal iframe; never serialize arbitrary input attributes or trusted embed caches.
                var normalized = document.CreateElement("iframe");
                normalized.SetAttribute("src", address.AbsoluteUri);
                foreach (var dimension in new[] { "width", "height" })
                {
                    var value = iframe.GetAttribute(dimension);
                    if (int.TryParse(value, out var pixels) && pixels > 0)
                    {
                        normalized.SetAttribute(dimension, pixels.ToString(CultureInfo.InvariantCulture));
                    }
                }
                if (iframe.HasAttribute("allowfullscreen"))
                {
                    normalized.SetAttribute("allowfullscreen", "");
                }
                LogWarning($"Media Embed '{address.AbsoluteUri}' remains subject to the destination site's HTML Field Security and the embedded site's browser framing restrictions. Verify playback at the destination.", LogStrings.Heading_BuiltInFunctions);
                return EncodeWebPartMappingJson(new JObject
                {
                    ["serverProcessedContent"] = new JObject
                    {
                        ["htmlStrings"] = new JObject(),
                        ["searchablePlainTexts"] = new JObject(),
                        ["imageSources"] = new JObject(),
                        ["links"] = new JObject()
                    },
                    ["dataVersion"] = "1.2",
                    ["properties"] = new JObject
                    {
                        ["embedCode"] = normalized.OuterHtml,
                        ["cachedEmbedCode"] = "",
                        ["cachedEmbedCodeHtml"] = "",
                        ["shouldScaleWidth"] = true
                    }
                });
            }
        }

        private NotAvailableAtTargetException MediaUnavailable(string reason)
        {
            var message = reason + " This web part will be skipped.";
            LogWarning(message, LogStrings.Heading_BuiltInFunctions);
            return new NotAvailableAtTargetException(message);
        }
    }
}
