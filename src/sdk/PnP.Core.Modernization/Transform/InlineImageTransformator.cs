using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;


namespace PnP.Core.Modernization.Transform
{
    internal sealed class InlineImageTransformator : BaseTransform
    {
        private readonly IPage page;
        private readonly Uri sourcePageUrl;
        private readonly Func<Uri, string> transferImage;
        private readonly Func<string, string> rewriteUrl;
        private readonly Func<string, Dictionary<string, string>> imageLookup;
        private readonly Func<Uri, string> importExternalImage;
        private readonly Func<Uri, (int Width, int Height)> systemImageSizeLookup;
        private readonly HashSet<string> internalAuthorities;
        private readonly HtmlParser parser = new HtmlParser(new HtmlParserOptions { IsEmbedded = true },
            BrowsingContext.New(Configuration.Default.WithCss()));

        internal InlineImageTransformator(IPage page, Uri sourcePageUrl,
            Func<Uri, string> transferImage, Func<string, string> rewriteUrl,
            IList<ILogObserver> logObservers = null, Func<string, Dictionary<string, string>> imageLookup = null,
            Func<Uri, string> importExternalImage = null, IEnumerable<Uri> internalOrigins = null,
            Func<Uri, (int Width, int Height)> systemImageSizeLookup = null)
        {
            this.page = page;
            this.sourcePageUrl = sourcePageUrl;
            this.transferImage = transferImage;
            this.rewriteUrl = rewriteUrl;
            this.imageLookup = imageLookup;
            this.importExternalImage = importExternalImage;
            this.systemImageSizeLookup = systemImageSizeLookup ?? SystemImageSize.Get;
            internalAuthorities = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sourcePageUrl.Authority };
            if (internalOrigins != null)
            {
                foreach (var origin in internalOrigins)
                {
                    internalAuthorities.Add(origin.Authority);
                }
            }
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    RegisterObserver(observer);
                }
            }
        }

        // Keep source URLs and image attributes intact until the selected text mapping is rendered.
        // Text markers also survive table-header normalization; no markers are persisted on the page.
        internal static string PreserveImages(string html, Func<string, string> transform)
        {
            using (var document = new HtmlParser().ParseDocument(html))
            {
                var images = document.QuerySelectorAll("img");
                if (images.Length == 0)
                {
                    return transform(html);
                }

                var originals = new Dictionary<string, string>();
                foreach (var image in images)
                {
                    var marker = $"pnpimage{Guid.NewGuid():N}";
                    originals.Add(marker, image.OuterHtml);
                    image.Parent.ReplaceChild(document.CreateTextNode(marker), image);
                }

                var result = transform(document.Body.InnerHtml);
                foreach (var original in originals)
                {
                    result = result.Replace(original.Key, original.Value);
                }
                return result;
            }
        }

        internal string Transform(string html, bool externalImagesOnly = false)
        {
            using (var document = parser.ParseDocument(html))
            {
                var images = document.QuerySelectorAll("img");
                if (images.Length == 0)
                {
                    return html;
                }

                if (page.EditorType != EditorType.CK5)
                {
                    const string message = "Rich-text inline image transformation requires a CK5 target page.";
                    LogError(message, LogStrings.Heading_ContentTransform);
                    throw new NotSupportedException(message);
                }

                bool changed = false;
                foreach (var image in images)
                {
                    var source = image.GetAttribute("src");
                    if (string.IsNullOrWhiteSpace(source))
                    {
                        image.Remove();
                        changed = true;
                        LogWarning("Removed a rich-text image with a missing, empty or whitespace-only src attribute in the source HTML. No image URL was available to migrate.",
                            LogStrings.Heading_ContentTransform);
                        continue;
                    }

                    if (!Uri.TryCreate(sourcePageUrl, source, out var imageUrl) ||
                        (imageUrl.Scheme != Uri.UriSchemeHttp && imageUrl.Scheme != Uri.UriSchemeHttps) ||
                        !string.IsNullOrEmpty(imageUrl.UserInfo))
                    {
                        if (!externalImagesOnly)
                        {
                            Warn(source, "The image source is not a supported HTTP(S) URL.");
                        }
                        continue;
                    }

                    var external = !internalAuthorities.Contains(imageUrl.Authority);
                    if (external != externalImagesOnly)
                    {
                        continue;
                    }
                    if (external && imageUrl.Scheme != Uri.UriSchemeHttps)
                    {
                        Warn(source, "External images require HTTPS for ingestion or native external rendering.");
                        continue;
                    }

                    IElement inlineImage;
                    try
                    {
                        if (!external && Regex.IsMatch(imageUrl.AbsolutePath, @"/_layouts/(?:[0-9]+/)?images/",
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                        {
                            // Native CK5 images are blocks: keep a marked file icon and its label in one inline link.
                            if (image.ClassList.Contains("ms-asset-icon") &&
                                !string.IsNullOrWhiteSpace(image.Closest("a[href]")?.TextContent))
                            {
                                image.SetAttribute("src", imageUrl.AbsoluteUri);
                                changed = true;
                                LogInfo($"Keeping SharePoint system icon '{imageUrl.GetLeftPart(UriPartial.Path)}' inside its labeled link.",
                                    LogStrings.Heading_ContentTransform);
                                continue;
                            }

                            var options = GetOptions(image);
                            if (!options.Width.HasValue && !options.WidthPercentage.HasValue)
                            {
                                var size = systemImageSizeLookup(imageUrl);
                                options.Width = options.Height.HasValue
                                    ? ScaleDimension(options.Height.Value, size.Width, size.Height)
                                    : size.Width;
                                options.Height ??= size.Height;
                                if (!options.Width.HasValue || options.Width <= 0 || options.Height <= 0)
                                {
                                    throw new InvalidDataException("The system image dimensions could not be determined.");
                                }
                            }
                            inlineImage = CreateUrlImage(document, image, imageUrl, options);
                            LogInfo($"Keeping SharePoint system image '{imageUrl.GetLeftPart(UriPartial.Path)}' as a native URL-based CK5 image.",
                                LogStrings.Heading_ContentTransform);
                        }
                        else if (external && ExternalImageImporter.IsStockImage(imageUrl))
                        {
                            inlineImage = CreateUrlImage(document, image, imageUrl);
                            LogInfo("Keeping a stock image as a native external CK5 image.", LogStrings.Heading_ContentTransform);
                        }
                        else
                        {
                            if (external && importExternalImage == null)
                            {
                                throw new InvalidDataException("The external image importer is unavailable.");
                            }
                            var targetUrl = external ? importExternalImage(imageUrl) : transferImage(imageUrl);
                            inlineImage = CreateSharePointImage(image, targetUrl);
                            inlineImage.SetAttribute("data-alttext", image.GetAttribute("alt") ?? "");
                            inlineImage.SetAttribute("data-linkurl", GetImageLink(image, source, imageUrl, targetUrl));
                        }
                    }
                    catch (Exception ex) when (ex is ServiceException ||
                                               ex is ServiceException ||
                                               ex is PnPException ||
                                               ex is HttpRequestException ||
                                               ex is WebException ||
                                               ex is IOException ||
                                               ex is InvalidDataException ||
                                               ex is UnauthorizedAccessException)
                    {
                        if (!external)
                        {
                            Warn(source, ex.Message);
                            continue;
                        }
                        inlineImage = CreateUrlImage(document, image, imageUrl);
                        var reason = ex is ServiceException serviceException && serviceException.Error is ServiceError serviceError
                            ? $"HTTP {serviceError.HttpResponseCode}"
                            : ex.GetType().Name;
                        LogWarning($"Could not import external image '{imageUrl.GetLeftPart(UriPartial.Path)}' ({reason}). Retaining its URL in a native external CK5 image.",
                            LogStrings.Heading_ContentTransform);
                    }

                    image.Parent.ReplaceChild(inlineImage, image);
                    NormalizeParents(inlineImage);
                    changed = true;
                }

                return changed ? document.Body.InnerHtml : html;
            }
        }

        private void Warn(string source, string reason)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var url))
            {
                source = url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps
                    ? url.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.SafeUnescaped)
                    : url.Scheme + ":[omitted]";
            }
            else if (source != null)
            {
                var query = source.IndexOfAny(new[] { '?', '#' });
                if (query >= 0)
                {
                    source = source.Substring(0, query);
                }
            }
            LogWarning($"Could not convert rich-text image '{source}' to a CK5 inline image. Retaining the original image HTML. {reason}",
                LogStrings.Heading_ContentTransform);
        }

        private IElement CreateSharePointImage(IElement image, string targetUrl)
        {
            var options = GetOptions(image);
            CompleteDimensions(options, targetUrl);
            if (options.Height.HasValue && !options.Width.HasValue && !options.WidthPercentage.HasValue)
            {
                throw new InvalidDataException("The image width could not be determined to preserve its specified height.");
            }
            var hasPixelWidth = options.Width.HasValue;
            var hasPercentageWidth = options.WidthPercentage.HasValue;
            var snippet = page.GetInlineImage(null, targetUrl, options);
            using (var fragment = parser.ParseDocument(snippet))
            {
                var plugin = fragment.QuerySelector("div.imagePlugin");
                if (plugin == null)
                {
                    throw new InvalidDataException("The SDK did not return a CK5 inline image.");
                }
                var inlineImage = (IElement)plugin.Clone(true);
                // The SDK's default 100% overrides pixel widths; let CK5 derive their responsive sizing.
                if (hasPixelWidth && !hasPercentageWidth)
                {
                    inlineImage.RemoveAttribute("data-widthpercentage");
                }
                inlineImage.SetAttribute("data-imageurl", targetUrl);
                return inlineImage;
            }
        }

        private IElement CreateUrlImage(IDocument document, IElement image, Uri imageUrl, PageImageOptions options = null)
        {
            options ??= GetOptions(image);
            var plugin = document.CreateElement("div");
            plugin.ClassName = "imagePlugin";
            plugin.SetAttribute("style", "background-color:transparent;position:relative;");
            plugin.SetAttribute("data-imageurl", imageUrl.AbsoluteUri);
            plugin.SetAttribute("data-uploading", "0");
            plugin.SetAttribute("data-alignment", options.Alignment.ToString());
            plugin.SetAttribute("data-width", (options.Width ?? 0).ToString(CultureInfo.InvariantCulture));
            plugin.SetAttribute("data-height", (options.Height ?? 0).ToString(CultureInfo.InvariantCulture));
            plugin.SetAttribute("data-imagenaturalwidth", (options.Width ?? 0).ToString(CultureInfo.InvariantCulture));
            plugin.SetAttribute("data-imagenaturalheight", (options.Height ?? 0).ToString(CultureInfo.InvariantCulture));
            if (options.WidthPercentage.HasValue)
            {
                plugin.SetAttribute("data-widthpercentage", options.WidthPercentage.Value.ToString(CultureInfo.InvariantCulture));
            }
            plugin.SetAttribute("data-alttext", image.GetAttribute("alt") ?? "");
            plugin.SetAttribute("data-captiontext", "");
            plugin.SetAttribute("data-isoverlaytextenabled", "false");
            plugin.SetAttribute("data-linkurl", GetImageLink(image, image.GetAttribute("src"), imageUrl, imageUrl.AbsoluteUri));
            return plugin;
        }

        private string GetImageLink(IElement image, string source, Uri imageUrl, string targetUrl)
        {
            var anchor = image.Closest("a[href]");
            if (anchor == null)
            {
                return "";
            }

            var link = anchor.GetAttribute("href");
            if (string.Equals(link, source, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link, rewriteUrl(source), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link, rewriteUrl(imageUrl.AbsolutePath), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link, rewriteUrl(imageUrl.AbsoluteUri), StringComparison.OrdinalIgnoreCase) ||
                (Uri.TryCreate(sourcePageUrl, link, out var linkUrl) &&
                 string.Equals(linkUrl.AbsoluteUri, imageUrl.AbsoluteUri, StringComparison.OrdinalIgnoreCase)))
            {
                return targetUrl;
            }

            return link;
        }

        private static PageImageOptions GetOptions(IElement image)
        {
            var style = image.GetStyle();
            var width = style.GetPropertyValue("width");
            var height = style.GetPropertyValue("height");
            var alignment = style.GetPropertyValue("float");
            if (string.IsNullOrEmpty(alignment))
            {
                alignment = image.GetAttribute("align");
            }
            for (var parent = image.ParentElement; parent != null && string.IsNullOrEmpty(alignment); parent = parent.ParentElement)
            {
                alignment = parent.GetStyle().GetTextAlign();
            }

            var options = new PageImageOptions
            {
                Width = PixelSize(width) ?? PixelSize(image.GetAttribute("width")),
                Height = PixelSize(height) ?? PixelSize(image.GetAttribute("height")),
                Alignment = PageImageAlignment.Left
            };

            if (string.IsNullOrEmpty(width))
            {
                width = image.GetAttribute("width");
            }
            if (width != null && width.EndsWith("%", StringComparison.Ordinal) &&
                int.TryParse(width.TrimEnd('%'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var percentage) &&
                percentage > 0 && percentage <= 100)
            {
                options.WidthPercentage = percentage;
            }

            if (string.Equals(alignment, "right", StringComparison.OrdinalIgnoreCase))
            {
                options.Alignment = PageImageAlignment.Right;
            }
            else if (string.Equals(alignment, "center", StringComparison.OrdinalIgnoreCase))
            {
                options.Alignment = PageImageAlignment.Center;
            }
            return options;
        }

        private static int? PixelSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            value = value.Trim();
            if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 2);
            }
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size) && size > 0
                ? size : (int?)null;
        }

        private void CompleteDimensions(PageImageOptions options, string imageUrl)
        {
            if (imageLookup == null || (options.Width.HasValue && options.Height.HasValue))
            {
                return;
            }

            var metadata = imageLookup(imageUrl);
            metadata.TryGetValue("ImageWidth", out var width);
            metadata.TryGetValue("ImageHeight", out var height);
            var naturalWidth = PixelSize(width);
            var naturalHeight = PixelSize(height);

            if (!options.Width.HasValue)
            {
                options.Width = options.Height.HasValue
                    ? ScaleDimension(options.Height.Value, naturalWidth, naturalHeight)
                    : naturalWidth;
            }
            if (!options.Height.HasValue)
            {
                options.Height = options.Width.HasValue
                    ? ScaleDimension(options.Width.Value, naturalHeight, naturalWidth)
                    : naturalHeight;
            }
        }

        private static int? ScaleDimension(int size, int? naturalSize, int? otherNaturalSize)
        {
            if (!naturalSize.HasValue || !otherNaturalSize.HasValue)
            {
                return null;
            }
            var scaled = Math.Round((double)size * naturalSize.Value / otherNaturalSize.Value);
            return scaled > 0 && scaled <= int.MaxValue ? (int)scaled : (int?)null;
        }

        private static void NormalizeParents(IElement image)
        {
            // A CK5 image is a block, so split phrasing containers, not canvas text controls.
            var targetParent = image.ParentElement;
            var boundary = image.Closest("td, th, li, body");
            for (var ancestor = image.ParentElement; ancestor != null && ancestor != boundary; ancestor = ancestor.ParentElement)
            {
                if (IsPhrasingContainer(ancestor.TagName))
                {
                    targetParent = ancestor.ParentElement;
                }
            }
            while (image.ParentElement != targetParent)
            {
                var parent = image.ParentElement;
                var before = (IElement)parent.Clone(false);
                var after = (IElement)parent.Clone(false);
                while (parent.FirstChild != image)
                {
                    before.AppendChild(parent.FirstChild);
                }
                while (image.NextSibling != null)
                {
                    after.AppendChild(image.NextSibling);
                }
                if (before.HasChildNodes)
                {
                    parent.Parent.InsertBefore(before, parent);
                    after.RemoveAttribute("id");
                }
                parent.Parent.InsertBefore(image, parent);
                if (after.HasChildNodes)
                {
                    parent.Parent.InsertBefore(after, parent);
                }
                parent.Remove();
            }
        }

        private static bool IsPhrasingContainer(string tagName)
        {
            switch (tagName)
            {
                case "P":
                case "A":
                case "SPAN":
                case "STRONG":
                case "EM":
                case "B":
                case "I":
                case "U":
                case "S":
                case "STRIKE":
                case "FONT":
                case "SUP":
                case "SUB":
                case "CODE":
                case "SMALL":
                case "BIG":
                case "TT":
                case "ABBR":
                case "ACRONYM":
                case "CITE":
                case "DFN":
                case "KBD":
                case "SAMP":
                case "VAR":
                case "BDI":
                case "BDO":
                case "Q":
                case "MARK":
                case "TIME":
                case "PRE":
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                    return true;
                default:
                    return false;
            }
        }
    }
}
