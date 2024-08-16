using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Class that implements the page header
    /// </summary>
    internal sealed class PageHeader : IPageHeader
    {
        private const string NoPageHeader = "<div><div data-sp-canvascontrol=\"\" data-sp-canvasdataversion=\"1.4\" data-sp-controldata=\"&#123;&quot;id&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;instanceId&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;title&quot;&#58;&quot;Title Region&quot;,&quot;description&quot;&#58;&quot;Title Region Description&quot;,&quot;serverProcessedContent&quot;&#58;&#123;&quot;htmlStrings&quot;&#58;&#123;&#125;,&quot;searchablePlainTexts&quot;&#58;&#123;&#125;,&quot;imageSources&quot;&#58;&#123;&#125;,&quot;links&quot;&#58;&#123;&#125;&#125;,&quot;dataVersion&quot;&#58;&quot;1.4&quot;,&quot;properties&quot;&#58;&#123;&quot;title&quot;&#58;&quot;@@title@@&quot;,&quot;imageSourceType&quot;&#58;4,&quot;layoutType&quot;&#58;&quot;NoImage&quot;,&quot;textAlignment&quot;&#58;&quot;@@textalignment@@&quot;,&quot;showTopicHeader&quot;&#58;@@showtopicheader@@,&quot;showPublishDate&quot;&#58;@@showpublishdate@@,&quot;topicHeader&quot;&#58;&quot;@@topicheader@@&quot;&#125;&#125;\"></div></div>";
        private const string DefaultPageHeader = "<div><div data-sp-canvascontrol=\"\" data-sp-canvasdataversion=\"1.4\" data-sp-controldata=\"&#123;&quot;id&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;instanceId&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;title&quot;&#58;&quot;Title Region&quot;,&quot;description&quot;&#58;&quot;Title Region Description&quot;,&quot;serverProcessedContent&quot;&#58;&#123;&quot;htmlStrings&quot;&#58;&#123;&#125;,&quot;searchablePlainTexts&quot;&#58;&#123;&#125;,&quot;imageSources&quot;&#58;&#123;&#125;,&quot;links&quot;&#58;&#123;&#125;&#125;,&quot;dataVersion&quot;&#58;&quot;1.4&quot;,&quot;properties&quot;&#58;&#123;&quot;title&quot;&#58;&quot;@@title@@&quot;,&quot;imageSourceType&quot;&#58;4,&quot;layoutType&quot;&#58;&quot;@@layouttype@@&quot;,&quot;textAlignment&quot;&#58;&quot;@@textalignment@@&quot;,&quot;showTopicHeader&quot;&#58;@@showtopicheader@@,&quot;showPublishDate&quot;&#58;@@showpublishdate@@,&quot;topicHeader&quot;&#58;&quot;@@topicheader@@&quot;,&quot;authorByline&quot;&#58;[@@authorbyline@@],&quot;authors&quot;&#58;[@@authors@@]&#125;&#125;\"></div></div>";
        private const string CustomPageHeader = "<div><div data-sp-canvascontrol=\"\" data-sp-canvasdataversion=\"1.4\" data-sp-controldata=\"&#123;&quot;id&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;instanceId&quot;&#58;&quot;cbe7b0a9-3504-44dd-a3a3-0e5cacd07788&quot;,&quot;title&quot;&#58;&quot;Title Region&quot;,&quot;description&quot;&#58;&quot;Title Region Description&quot;,&quot;serverProcessedContent&quot;&#58;&#123;&quot;htmlStrings&quot;&#58;&#123;&#125;,&quot;searchablePlainTexts&quot;&#58;&#123;&#125;,&quot;imageSources&quot;&#58;&#123;&quot;imageSource&quot;&#58;&quot;@@imageSource@@&quot;&#125;,&quot;links&quot;&#58;&#123;&#125;,&quot;customMetadata&quot;&#58;&#123;&quot;imageSource&quot;&#58;&#123;&quot;siteId&quot;&#58;&quot;@@siteId@@&quot;,&quot;webId&quot;&#58;&quot;@@webId@@&quot;,&quot;listId&quot;&#58;&quot;@@listId@@&quot;,&quot;uniqueId&quot;&#58;&quot;@@uniqueId@@&quot;&#125;&#125;&#125;,&quot;dataVersion&quot;&#58;&quot;1.4&quot;,&quot;properties&quot;&#58;&#123;&quot;title&quot;&#58;&quot;@@title@@&quot;,&quot;imageSourceType&quot;&#58;2,&quot;layoutType&quot;&#58;&quot;@@layouttype@@&quot;,&quot;textAlignment&quot;&#58;&quot;@@textalignment@@&quot;,&quot;showTopicHeader&quot;&#58;@@showtopicheader@@,&quot;showPublishDate&quot;&#58;@@showpublishdate@@,&quot;topicHeader&quot;&#58;&quot;@@topicheader@@&quot;,&quot;authorByline&quot;&#58;[@@authorbyline@@],&quot;authors&quot;&#58;[@@authors@@],&quot;altText&quot;&#58;&quot;@@alternativetext@@&quot;,&quot;webId&quot;&#58;&quot;@@webId@@&quot;,&quot;siteId&quot;&#58;&quot;@@siteId@@&quot;,&quot;listId&quot;&#58;&quot;@@listId@@&quot;,&quot;uniqueId&quot;&#58;&quot;@@uniqueId@@&quot;@@focalPoints@@&#125;&#125;\"></div></div>";
        private const string PageTitleWPHeader = "";
        private string imageServerRelativeUrl;
        private readonly PnPContext clientContext;
        private bool headerImageResolved;
        private Guid siteId = Guid.Empty;
        private Guid webId = Guid.Empty;
        private Guid listId = Guid.Empty;
        private Guid uniqueId = Guid.Empty;

        /// <summary>
        /// Returns the type of header
        /// </summary>
        public PageHeaderType Type { get; private set; }

        /// <summary>
        /// Server relative link to page header image, set to null for default header image.
        /// Note: image needs to reside in the current site
        /// </summary>
        public string ImageServerRelativeUrl
        {
            get
            {
                return imageServerRelativeUrl;
            }
            set
            {
                imageServerRelativeUrl = value;
                headerImageResolved = false;
            }
        }

        /// <summary>
        /// Image focal point X coordinate
        /// </summary>
        public double? TranslateX { get; set; }

        /// <summary>
        /// Image focal point Y coordinate
        /// </summary>
        public double? TranslateY { get; set; }

        /// <summary>
        /// Type of layout used inside the header
        /// </summary>
        public PageHeaderLayoutType LayoutType { get; set; }

        /// <summary>
        /// Alignment of the title in the header
        /// </summary>
        public PageHeaderTitleAlignment TextAlignment { get; set; }

        /// <summary>
        /// Show the topic header in the title region
        /// </summary>
        public bool ShowTopicHeader { get; set; }

        /// <summary>
        /// Show the page publication date in the title region
        /// </summary>
        public bool ShowPublishDate { get; set; }

        /// <summary>
        /// The topic header text to show if ShowTopicHeader is set to true
        /// </summary>
        public string TopicHeader { get; set; }

        /// <summary>
        /// Alternative text for the header image
        /// </summary>
        public string AlternativeText { get; set; }

        /// <summary>
        /// Page author(s) to be displayed
        /// </summary>
        public string Authors { get; set; }

        /// <summary>
        /// Page author byline
        /// </summary>
        public string AuthorByLine { get; set; }

        /// <summary>
        /// Id of the page author
        /// </summary>
        public int AuthorByLineId { get; set; }

        internal Guid HeaderImageId
        {
            get
            {
                return uniqueId;
            }
        }

        #region construction
        /// <summary>
        /// Creates a custom header with a custom image
        /// </summary>
        /// <param name="cc">ClientContext of the site hosting the image</param>
        /// <param name="pageHeaderType">Type of page header</param>
        /// <param name="imageServerRelativeUrl">Server relative image url</param>
        internal PageHeader(PnPContext cc, PageHeaderType pageHeaderType, string imageServerRelativeUrl)
        {
            this.imageServerRelativeUrl = imageServerRelativeUrl;
            clientContext = cc;
            Type = pageHeaderType;
            LayoutType = PageHeaderLayoutType.FullWidthImage;
            TextAlignment = PageHeaderTitleAlignment.Left;
            ShowTopicHeader = false;
            TopicHeader = "";
            Authors = "";
            AlternativeText = "";
            ShowPublishDate = false;
            AuthorByLineId = -1;
        }
        #endregion

        /// <summary>
        /// Returns the header value to set a "no header"
        /// </summary>
        /// <param name="pageTitle">Title of the page</param>
        /// <param name="titleAlignment">Left align or center the title</param>
        /// <returns>Header html value that indicates "no header"</returns>
        private static string NoHeader(string pageTitle, PageHeaderTitleAlignment titleAlignment)
        {
            if (pageTitle == null)
            {
                pageTitle = "";
            }
            else
            {
                pageTitle = EncodePageTitle(pageTitle);
            }

            string header = Replace1point4Defaults(NoPageHeader);

            return header.Replace("@@title@@", pageTitle).Replace("@@textalignment@@", titleAlignment.ToString());
        }

        /// <summary>
        /// Returns the header value to set a "no header"
        /// </summary>
        /// <param name="pageTitle">Title of the page</param>
        /// <returns>Header html value that indicates "no header"</returns>
        public static string NoHeader(string pageTitle)
        {
            return NoHeader(pageTitle, PageHeaderTitleAlignment.Left);
        }

        /// <summary>
        ///  Returns the header value to set a "PageTitleWebPart"
        /// </summary>
        /// <returns></returns>
        public static string PageTitleWebPartHeader()
        {
            return PageTitleWPHeader;
        }


        /// <summary>
        /// Load the PageHeader object from the given html
        /// </summary>
        /// <param name="pageHeaderHtml">Page header html</param>
        public void FromHtml(string pageHeaderHtml)
        {
            // select all control div's
            if (string.IsNullOrEmpty(pageHeaderHtml))
            {
                Type = PageHeaderType.Default;
                return;
            }

            HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });
            using (var document = parser.ParseDocument(pageHeaderHtml))
            {
                var pageHeaderControl = document.All.FirstOrDefault(m => m.HasAttribute(CanvasControl.ControlDataAttribute));
                if (pageHeaderControl != null)
                {
                    string pageHeaderData = pageHeaderControl.GetAttribute(CanvasControl.ControlDataAttribute);
                    string decoded = "";

                    if (pageHeaderData.Contains("%7B") && pageHeaderData.Contains("%22") && pageHeaderData.Contains("%7D"))
                    {
                        decoded = WebUtility.UrlDecode(pageHeaderData);
                    }
                    else
                    {
                        decoded = WebUtility.HtmlDecode(pageHeaderData);
                    }

                    var wpJObject = JsonSerializer.Deserialize<JsonElement>(decoded);

                    // Store the server processed content as that's needed for full fidelity
                    if (wpJObject.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                    {
                        if (serverProcessedContent.TryGetProperty("imageSources", out JsonElement imageSources) && imageSources.TryGetProperty("imageSource", out JsonElement imageSource))
                        {
                            imageServerRelativeUrl = imageSource.GetString();
                        }

                        // Properties that apply to all header configurations
                        if (wpJObject.TryGetProperty("properties", out JsonElement properties))
                        {
                            if (properties.TryGetProperty("layoutType", out JsonElement layoutType))
                            {
                                LayoutType = (PageHeaderLayoutType)Enum.Parse(typeof(PageHeaderLayoutType), layoutType.GetString());
                            }

                            if (properties.TryGetProperty("textAlignment", out JsonElement textAlignment))
                            {
                                TextAlignment = (PageHeaderTitleAlignment)Enum.Parse(typeof(PageHeaderTitleAlignment), textAlignment.GetString());
                            }

                            if (properties.TryGetProperty("showTopicHeader", out JsonElement showTopicHeader))
                            {
                                ShowTopicHeader = showTopicHeader.GetBoolean();
                            }

                            if (properties.TryGetProperty("showPublishDate", out JsonElement showPublishDate))
                            {
                                ShowPublishDate = showPublishDate.GetBoolean();
                            }

                            if (properties.TryGetProperty("topicHeader", out JsonElement topicHeader))
                            {
                                TopicHeader = topicHeader.GetString();
                            }

                            if (properties.TryGetProperty("authors", out JsonElement authors))
                            {
                                Authors = authors.ToString();
                            }

                            if (properties.TryGetProperty("authorByline", out JsonElement authorByline))
                            {
                                AuthorByLine = authorByline.ToString();
                            }

                            // Specific properties that only apply when the header has a custom image
                            if (!string.IsNullOrEmpty(imageServerRelativeUrl))
                            {
                                Type = PageHeaderType.Custom;

                                if (properties.TryGetProperty("siteId", out JsonElement siteId) && siteId.TryGetGuid(out Guid siteIdGuid))
                                {
                                    this.siteId = siteIdGuid;
                                }

                                if (properties.TryGetProperty("webId", out JsonElement webId) && webId.TryGetGuid(out Guid webIdGuid))
                                {
                                    this.webId = webIdGuid;
                                }

                                if (properties.TryGetProperty("listId", out JsonElement listId) && listId.TryGetGuid(out Guid listIdGuid))
                                {
                                    this.listId = listIdGuid;
                                }

                                if (properties.TryGetProperty("uniqueId", out JsonElement uniqueId) && uniqueId.TryGetGuid(out Guid uniqueIdGuid))
                                {
                                    this.uniqueId = uniqueIdGuid;
                                }
                                if (this.siteId != Guid.Empty && this.webId != Guid.Empty && this.listId != Guid.Empty && this.uniqueId != Guid.Empty)
                                {
                                    headerImageResolved = true;
                                }

                                System.Globalization.CultureInfo usCulture = new System.Globalization.CultureInfo("en-US");
                                System.Globalization.CultureInfo europeanCulture = new System.Globalization.CultureInfo("nl-BE");

                                if (properties.TryGetProperty("translateX", out JsonElement translateXElement))
                                {
                                    if (translateXElement.ValueKind == JsonValueKind.Number)
                                    {
                                        var translateXEN = translateXElement.GetDecimal().ToString();

                                        System.Globalization.CultureInfo cultureToUse;
                                        if (translateXEN.Contains("."))
                                        {
                                            cultureToUse = usCulture;
                                        }
                                        else if (translateXEN.Contains(","))
                                        {
                                            cultureToUse = europeanCulture;
                                        }
                                        else
                                        {
                                            cultureToUse = usCulture;
                                        }

                                        double.TryParse(translateXEN, System.Globalization.NumberStyles.Float, cultureToUse, out double translateX);
                                        TranslateX = translateX;
                                    }
                                }

                                if (properties.TryGetProperty("translateY", out JsonElement translateYElement))
                                {
                                    if (translateYElement.ValueKind == JsonValueKind.Number)
                                    {
                                        var translateYEN = translateYElement.GetDecimal().ToString();

                                        System.Globalization.CultureInfo cultureToUse;
                                        if (translateYEN.Contains("."))
                                        {
                                            cultureToUse = usCulture;
                                        }
                                        else if (translateYEN.Contains(","))
                                        {
                                            cultureToUse = europeanCulture;
                                        }
                                        else
                                        {
                                            cultureToUse = usCulture;
                                        }

                                        double.TryParse(translateYEN, System.Globalization.NumberStyles.Float, cultureToUse, out double translateY);
                                        TranslateY = translateY;
                                    }
                                }

                                if (properties.TryGetProperty("altText", out JsonElement altText))
                                {
                                    AlternativeText = altText.GetString();
                                }
                            }
                            else
                            {
                                Type = PageHeaderType.Default;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the header html representation
        /// </summary>
        /// <param name="pageTitle">Title of the page</param>
        /// <returns>Header html value</returns>
        public async Task<string> ToHtmlAsync(string pageTitle)
        {
            if (Type == PageHeaderType.PageTitleWebPart)
                return PageTitleWPHeader;

            if (pageTitle == null)
            {
                pageTitle = "";
            }
            else
            {
                pageTitle = EncodePageTitle(pageTitle);
            }

            // Get the web part properties
            if (!string.IsNullOrEmpty(ImageServerRelativeUrl) && clientContext != null)
            {
                if (!headerImageResolved)
                {
                    await ResolvePageHeaderImageAsync().ConfigureAwait(false);
                }

                if (headerImageResolved)
                {
                    string focalPoints = "";
                    if (TranslateX.HasValue || TranslateY.HasValue)
                    {
                        System.Globalization.CultureInfo usCulture = new System.Globalization.CultureInfo("en-US");
                        var translateX = TranslateX.Value.ToString(usCulture);
                        var translateY = TranslateY.Value.ToString(usCulture);
                        focalPoints = $",&quot;translateX&quot;&#58;{translateX},&quot;translateY&quot;&#58;{translateY}";
                    }

                    // Populate default properties
                    var header = await FillDefaultPropertiesAsync(CustomPageHeader).ConfigureAwait(false);
                    // Populate custom header specific properties
                    return header.Replace("@@siteId@@", siteId.ToString()).Replace("@@webId@@", webId.ToString()).Replace("@@listId@@", listId.ToString()).Replace("@@uniqueId@@", uniqueId.ToString()).Replace("@@focalPoints@@", focalPoints).Replace("@@title@@", pageTitle).Replace("@@imageSource@@", ImageServerRelativeUrl).Replace("@@alternativetext@@", AlternativeText == null ? "" : AlternativeText);
                }
            }

            // in case nothing worked out...
            // Populate default properties
            var defaultHeader = await FillDefaultPropertiesAsync(DefaultPageHeader).ConfigureAwait(false);
            // Populate title
            return defaultHeader.Replace("@@title@@", pageTitle);
        }

        private async Task<string> FillDefaultPropertiesAsync(string header)
        {
            if (!string.IsNullOrEmpty(Authors))
            {
                string data = Authors.Replace("\r", "").Replace("\n", "").TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                var jsonencoded = WebUtility.HtmlEncode(data).Replace(":", "&#58;"); //.Replace("@", "%40");
                header = header.Replace("@@authors@@", jsonencoded);
            }
            else
            {
                header = header.Replace("@@authors@@", "");
            }

            if (!string.IsNullOrEmpty(AuthorByLine))
            {
                string data = AuthorByLine.Replace("\r", "").Replace("\n", "").Replace(" ", "").TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                var jsonencoded = WebUtility.HtmlEncode(data).Replace(":", "&#58;");
                header = header.Replace("@@authorbyline@@", jsonencoded);

                int userId = -1;
                try
                {
                    var user = await clientContext.Web.EnsureUserAsync(data.Replace("\"", "").Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[0]).ConfigureAwait(false);
                    userId = user.Id;
                }
                catch (Exception)
                {

                }

                AuthorByLineId = userId;
            }
            else
            {
                header = header.Replace("@@authorbyline@@", "");
            }

            return header.Replace("@@showtopicheader@@", ShowTopicHeader.ToString().ToLower()).Replace("@@showpublishdate@@", ShowPublishDate.ToString().ToLower()).Replace("@@topicheader@@", TopicHeader == null ? "" : TopicHeader).Replace("@@textalignment@@", TextAlignment.ToString()).Replace("@@layouttype@@", LayoutType.ToString());
        }

        private static string Replace1point4Defaults(string header)
        {
            return header.Replace("@@showtopicheader@@", "false").Replace("@@showpublishdate@@", "false").Replace("@@topicheader@@", "");
        }

        private static string EncodePageTitle(string pageTitle)
        {
            string result = pageTitle;

            if (result.Contains("\""))
            {
                result = result.Replace("\"", "\\&quot;");
            }

            return result;
        }

        private async Task ResolvePageHeaderImageAsync()
        {
            try
            {
                await clientContext.Site.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);
                await clientContext.Web.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);
                siteId = clientContext.Site.Id;
                webId = clientContext.Web.Id;

                if (!ImageServerRelativeUrl.StartsWith("/_LAYOUTS", StringComparison.OrdinalIgnoreCase))
                {
                    var pageHeaderImage = await clientContext.Web.GetFileByServerRelativeUrlAsync(ImageServerRelativeUrl, p => p.UniqueId, p => p.ListId).ConfigureAwait(false);

                    listId = pageHeaderImage.ListId;
                    uniqueId = pageHeaderImage.UniqueId;
                }

                headerImageResolved = true;
            }
            catch (SharePointRestServiceException ex)
            {
                var error = ex.Error as SharePointRestError;

                if (File.ErrorIndicatesFileDoesNotExists(error))
                {
                    clientContext.Logger.LogInformation("Provided file link does not exist...we're eating the exception and the page will end up with a default page header");
                }
                else if (error.Message.Contains("SPWeb.ServerRelativeUrl"))
                {
                    headerImageResolved = true;
                    clientContext.Logger.LogInformation("Image resides in a different site collection context, we will simply allow it to be referred in the page header section");
                }
                else
                {
                    headerImageResolved = true;
                    clientContext.Logger.LogInformation("The image can also refer to a path outside SharePoint, that is also allowed, so we will mark it as resolved and move ahead");
                }
            }
        }
    }
}
