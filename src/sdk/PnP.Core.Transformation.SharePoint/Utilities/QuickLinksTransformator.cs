using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace PnP.Core.Transformation.SharePoint.Utilities
{
    #region QuickLinks property model
    public class QuickLinksWebPartProperties
    {
        //IQuickLinksWebPartProps
        [JsonProperty(PropertyName = "items")] //The Quick Link items.
        public List<QuickLink> Items { get; set; }
        [JsonProperty(PropertyName = "isMigrated")] //The flag indicates whether migration for this web part has been completed. For new web parts this value is true.
        public bool IsMigrated { get; set; }
        [JsonProperty(PropertyName = "shouldShowThumbnail")] //Whether web part should show thumbnail in compact card layout
        public bool ShouldShowThumbnail { get; set; }
        [JsonProperty(PropertyName = "buttonLayoutOptions")] //The layout option of button layout
        public ButtonLayoutOptions ButtonLayoutOptions { get; set; }
        [JsonProperty(PropertyName = "listLayoutOptions")] //The layout option of list layout
        public ListLayoutOptions ListLayoutOptions { get; set; }
        [JsonProperty(PropertyName = "waffleLayoutOptions")] //The layout option of waffle layout
        public WaffleLayoutOptions WaffleLayoutOptions { get; set; }

        // IBaseCollectionWebPartProperties props
        [JsonProperty(PropertyName = "layoutId")] // Set when there are more than one layouts to indicate which layout is in use.
        public string LayoutId { get; set; }
        [JsonProperty(PropertyName = "dataProviderId")] // Set when there are more than one data providers to indicate which data provider is in use.
        public string DataProviderId { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "addToPageScreenReaderLabel")]
        public string AddToPageScreenReaderLabel { get; set; }
        [JsonProperty(PropertyName = "hideWebPartWhenEmpty")]
        public bool? HideWebPartWhenEmpty { get; set; }
        [JsonProperty(PropertyName = "webId")]
        public string WebId { get; set; }
        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }
        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; } // base url to locate web part resources such has file icons

        public QuickLinksWebPartProperties()
        {
            this.Items = new List<QuickLink>();
        }
    }

    public class QuickLink
    {
        /**
         * The info of the source item where does the quick link point to.
         */
        [JsonProperty(PropertyName = "sourceItem")]
        public SourceItemInfo SourceItem { get; set; }

        /**
         * The info of the image selected by user.
         */
        [JsonProperty(PropertyName = "image")]
        public QuickLinkCustomizedImageInfo Image { get; set; }

        /**
         * The info of the icon selected by user.
         */
        [JsonProperty(PropertyName = "fabricReactIcon")]
        public QuickLinkFabricReactIconInfo FabricReactIcon { get; set; }

        /**
         * Indicate the way that thumbnail is chosen and rendered.
         */
        [JsonProperty(PropertyName = "thumbnailType")]
        public QuickLinkThumbnailSourceType ThumbnailType { get; set; }

        /**
         * To store the preview image that doesn't have guid set.
         * If quick link is not targeting a SharePoint item, the image and title will be retrieved from embed service.
         * Also in on-premise environment, the image preview doesn't have guids.
         */
        [JsonProperty(PropertyName = "rawPreviewImageUrl")]
        public string RawPreviewImageUrl { get; set; }

        /**
         * Unique id of the quick link.
         */
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        /**
         * Title of the quick link.
         */
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /**
         * Extra description field of the quick link item.
         */
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /**
         * User customized alt text to place on the image for people with disabilities.
         */
        [JsonProperty(PropertyName = "altText")]
        public string AltText { get; set; }
    }

    public class QuickLinkFabricReactIconInfo
    {
        /**
         * The name of the icon.
         */
        [JsonProperty(PropertyName = "iconName")]
        public string IconName { get; set; }
    }

    public class GuidSet
    {
        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }
        [JsonProperty(PropertyName = "webId")]
        public string WebId { get; set; }
        [JsonProperty(PropertyName = "listId")]
        public string ListId { get; set; }
        [JsonProperty(PropertyName = "uniqueId")]
        public string UniqueId { get; set; }
    }

    public class SourceItemInfo
    {
        /**
         * Original url of the quick link.
         */
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /**
         * Guids that represent the quick link item.
         * Only SharePoint item has guids. It is "undefined" for external items.
         */
        [JsonProperty(PropertyName = "guids")]
        public GuidSet Guids { get; set; }

        /**
         * The type of object represented by the quick link. If the type is unknown, it will be considered
         * as QuickLinkType.Link.
         */
        [JsonProperty(PropertyName = "itemType")]
        public QuickLinkType ItemType { get; set; }

        /**
         * File extension of the file represented by the quick link. Example: ".png". If the object represented
         * by the quick link does not have a file extension or is not a file, this will be null or the empty string.
         * E.g. Some OneNote doesn't have extensions and will need "progId" to identify them.
         */
        [JsonProperty(PropertyName = "fileExtension")]
        public string FileExtension { get; set; }

        /**
         * Identifier of the program used to open this kind of file or folder, if this is a file or folder.
         * E.g. OneNote.
         */
        [JsonProperty(PropertyName = "progId")]
        public string ProgId { get; set; }
    }

    public class QuickLinkCustomizedImageInfo
    {
        /**
         * The source url of the customized image of the quick link.
         */
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /**
         * How image is fit into the container.
         */
        [JsonProperty(PropertyName = "imageFit")]
        public ImageFit ImageFit { get; set; }

        /**
         * Guids that can be used to load the picture from the micro-service, if specified.
         */
        [JsonProperty(PropertyName = "guids")]
        public GuidSet Guids { get; set; }
    }

    public class WaffleLayoutOptions
    {
        /**
         * The size of icon displayed on each card.
         */
        [JsonProperty(PropertyName = "iconSize")]
        public IconSize IconSize { get; set; }

        /**
         * Whether only image or icon is displayed in the waffle layout.
         */
        [JsonProperty(PropertyName = "onlyShowThumbnail")]
        public bool OnlyShowThumbnail { get; set; }
    }

    public class ListLayoutOptions
    {
        /**
         * Whether show the link description text of all link items.
         */
        [JsonProperty(PropertyName = "showDescription")]
        public bool ShowDescription { get; set; }

        /**
         * Whether show the icons of the link items.
         */
        [JsonProperty(PropertyName = "showIcon")]
        public bool ShowIcon { get; set; }
    }

    public class ButtonLayoutOptions
    {
        /**
         * Whether to have icon and the position of the icon if icon is used. (IconPositionType.NoIcon | IconPositionType.IconToLeft | IconPositionType.IconToTop)
         */
        [JsonProperty(PropertyName = "iconPositionType")]
        public IconPositionType IconPositionType { get; set; }

        /**
         * The display style of the button.
         */
        [JsonProperty(PropertyName = "buttonTreatment")]
        public ButtonTreatment ButtonTreatment { get; set; }

        /**
         * The option is only available when IconPositionType.noIcon or IconPositionType.IconToLeft
         * The text can be either top aligned or center aligned. (ContentAlignment.Top | ContentAlignment.Center)
         */
        [JsonProperty(PropertyName = "textAlignmentVertical")]
        public ContentAlignment TextAlignmentVertical { get; set; }

        /**
         * The option is only available when IconPositionType.IconToTop
         * The text can be either left aligned or center aligned. (ContentAlignment.Left | ContentAlignment.Center)
         */
        [JsonProperty(PropertyName = "textAlignmentHorizontal")]
        public ContentAlignment TextAlignmentHorizontal { get; set; }

        /**
         * Number of lines of text that helps user to make the card more compact. Allowed: LinesOfText.SingleLine | LinesOfText.TwoLines;
         */
        [JsonProperty(PropertyName = "linesOfText")]
        public LinesOfText LinesOfText { get; set; }

        /**
         * Whether show the link description text of all link items.
         */
        [JsonProperty(PropertyName = "showDescription")]
        public bool ShowDescription { get; set; }
    }

    public enum ImageFit
    {
        /**
         * The image is not scaled. The image is centered and cropped within the content box.
         */
        center = 0,

        /**
         * The image is scaled to maintain its aspect ratio while being fully contained within the frame. The image will
         * be centered horizontally and vertically within the frame. The space in the top and bottom or in the sides of
         * the frame will be empty depending on the difference in aspect ratio between the image and the frame.
         */
        contain = 1,

        /**
         * The image is scaled to maintain its aspect ratio while filling the frame. Portions of the image will be cropped from
         * the top and bottom, or from the sides, depending on the difference in aspect ratio between the image and the frame.
         */
        cover = 2,

        /**
         * Neither the image nor the frame are scaled. If their sizes do not match, the image will either be cropped or the
         * frame will have empty space.
         */
        none = 3,

        /**
         * The image will be centered horizontally and vertically within the frame and maintains its aspect ratio. It will
         * behave as ImageFit.center if the image's natural height or width is less than the Image frame's height or width,
         * but if both natural height and width are larger than the frame it will behave as ImageFit.cover.
         */
        centerCover = 4
    }

    public enum QuickLinkType
    {
        File = 0,
        Folder = 1,
        Link = 2,
        ListItem = 3,
        List = 4,
        DocumentLibrary = 5
    }

    public enum QuickLinkThumbnailSourceType
    {
        /**
         * A user customized image.
         */
        Image = 1,

        /**
         * A user picked icon from fabric-react icon set.
         */
        FabricReactIcon = 2,

        /**
         * The thumbnail of the item is automatically selected according to the layout type and item data.
         */
        AutoSelect = 3
    }

    public enum ContentAlignment
    {
        Top = 1,
        Center = 2,
        Left = 3
    }

    public enum LinesOfText
    {
        /**
         * The space of rendering the text is fixed to be one line.
         */
        SingleLine = 1,

        /**
         * The space of rendering the text is fixed to be two lines.
         */
        TwoLines = 2,

        /**
         * The maximum space of rendering the text is two lines.
         * But if there is not enough text to fill two lines, the space is dynamic based on the text.
         * I.e. The space could potentially be 0, 1, or 2 lines.
         */
        TwoLinesAutoCollapse = 3
    }

    public enum IconSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge,
        Fill
    }

    public enum IconPositionType
    {
        /**
         * No icon is displayed in button card.
         */
        NoIcon = 1,

        /**
         * Icon is displayed at left side of button card.
         */
        IconToLeft = 2,

        /**
         * Icon is displayed at top side of button card.
         */
        IconToTop = 3,

        /**
         * Only icon is displayed, there is no other content.
         */
        IconOnly = 4
    }

    public enum ButtonTreatment
    {
        /**
         * No button treatment styles.
         */
        None = 1,

        /**
         * There will be an outline border around the button card.
         */
        Outline = 2,

        /**
         * The background of the button card will be filled with single color.
         */
        FillColor = 3
    }

    public enum QuickLinksLayout
    {
        /**
         * The compact card layout renderers CompactCard under GridLayout.
         */
        CompactCard,

        /**
         * The filmstrip layout renderers TileCard under Carousel layout.
         */
        FilmStrip,

        /**
         * The button layout renders ButtonCard under GridLayout.
         */
        Button,

        /**
         * The list layout renders ButtonCard under GridLayout.
         * The ButtonCard rendered in list layout is pre-configured to be single line text without outline.
         */
        List,

        /**
         * The grid layout renderers TileCard under GridLayout.
         */
        Grid,

        /**
         * The waffle layout which renders smaller ButtonCard under GridLayout.
         */
        Waffle
    }
    #endregion

    #region SummaryLinks model
    public class SummaryLink
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAltText { get; set; }
        public string ToolTip { get; set; }
        public string Style { get; set; }
        public bool OpenInNewWindow { get; set; }
    }
    #endregion

    #region Result class
    public class QuickLinksTransformatorResult
    {
        public string Properties { get; set; }
        public string SearchablePlainTexts { get; set; }
        public string ImageSources { get; set; }
        public string Links { get; set; }
    }
    #endregion

    /// <summary>
    /// Class used to generate quicklinks web part properties
    /// </summary>
    public class QuickLinksTransformator
    {
        private QuickLinksWebPartProperties properties;
        private ClientContext clientContext;

        #region Construction
        /// <summary>
        /// Instantiates the class
        /// </summary>
        /// <param name="cc">Client context for the web holding the source page</param>
        public QuickLinksTransformator(ClientContext cc)
        {
            this.clientContext = cc;
            this.properties = new QuickLinksWebPartProperties();

            cc.Web.EnsureProperties(p => p.Id, p => p.Url);
            cc.Site.EnsureProperties(p => p.Id, p => p.RootWeb);
            cc.Site.RootWeb.EnsureProperty(p => p.Url);
        }
        #endregion

        /// <summary>
        /// Generate quick links web part properties coming from a list of links
        /// </summary>
        /// <param name="summaryLinks">Links coming from the summarylink web part</param>
        /// <param name="quickLinksJsonProperties">Json properties blob for QuickLinks web part tailoring</param>
        /// <returns>Properties for highlighted content web part</returns>
        public QuickLinksTransformatorResult Transform(List<SummaryLink> summaryLinks, string quickLinksJsonProperties)
        {
            QuickLinksTransformatorResult result = new QuickLinksTransformatorResult()
            {
                Properties = "",
                ImageSources = "",
                SearchablePlainTexts = "",
                Links = ""
            };

            SetupQuickLinksProperties(quickLinksJsonProperties);

            // Rationalize links
            foreach (var link in summaryLinks)
            {
                if (string.IsNullOrEmpty(link.Title))
                {
                    link.Title = "Link"; // empty title should not be the case
                }
                if (link.Description == null)
                {
                    link.Description = "";
                }
                if (link.ImageAltText == null)
                {
                    link.ImageAltText = "";
                }
                if (link.Url == null)
                {
                    link.Url = "";
                }

                link.Title = JsonEncode(link.Title);
                link.Description = JsonEncode(link.Description);
                link.ImageAltText = JsonEncode(link.ImageAltText);
            }

            int i = 0;
            // SearchablePlainTexts
            string searchablePlainTexts = "";
            string searchablePlainTextsTitle = "";
            string searchablePlainTextsDescription = "";
            string searchablePlainTextsAltText = "";
            foreach (var link in summaryLinks)
            {
                searchablePlainTextsTitle = searchablePlainTextsTitle + $",\"items[{i}].title\": \"{link.Title}\"";
                searchablePlainTextsDescription = searchablePlainTextsDescription + $",\"items[{i}].description\": \"{link.Description}\"";
                searchablePlainTextsAltText = searchablePlainTextsAltText + $",\"items[{i}].altText\": \"{link.ImageAltText}\"";
                i++;
            }
            searchablePlainTexts = $"{searchablePlainTextsTitle}{searchablePlainTextsDescription}{searchablePlainTextsAltText}";

            i = 0;
            // Links
            string links = "";
            foreach (var link in summaryLinks)
            {
                links = links + $",\"items[{i}].sourceItem.url\": \"{link.Url}\"";
                i++;
            }

            i = 0;
            // ImageSources
            string imageSources = "";
            foreach (var link in summaryLinks)
            {
                if (link.ImageUrl != null)
                {
                    imageSources = imageSources + $",\"items[{i}].image.url\": \"{link.ImageUrl}\"";
                }
                i++;
            }

            i = 1;
            // Items
            foreach (var link in summaryLinks)
            {
                QuickLink q = new QuickLink()
                {
                    Id = i,
                    ThumbnailType = QuickLinkThumbnailSourceType.AutoSelect,
                    SourceItem = new SourceItemInfo()
                    {
                        ItemType = QuickLinkType.Link,
                        FileExtension = "",
                        ProgId = "",
                    },
                };

                if (!string.IsNullOrEmpty(link.Url))
                {
                    var linkInfo = FileLookup(link.Url);

                    if (linkInfo != null && !string.IsNullOrEmpty(linkInfo["FileListId"]) && !string.IsNullOrEmpty(linkInfo["FileUniqueId"]))
                    {
                        q.SourceItem.ItemType = QuickLinkType.File;
                        q.SourceItem.FileExtension = System.IO.Path.GetExtension(link.Url);
                    }
                }

                if (!string.IsNullOrEmpty(link.ImageUrl))
                {
                    var previewImageInfo = FileLookup(link.ImageUrl);

                    if (!string.IsNullOrEmpty(previewImageInfo["FileListId"]) && !string.IsNullOrEmpty(previewImageInfo["FileUniqueId"]))
                    {
                        q.ThumbnailType = QuickLinkThumbnailSourceType.Image;
                        q.Image = new QuickLinkCustomizedImageInfo()
                        {
                            Guids = new GuidSet()
                            {
                                ListId = previewImageInfo["FileListId"],
                                UniqueId = previewImageInfo["FileUniqueId"],
                                WebId = previewImageInfo["FileWebId"],
                                SiteId = previewImageInfo["FileSiteId"],
                            },
                            ImageFit = ImageFit.cover,
                        };
                    }
                }

                this.properties.Items.Add(q);
                i++;
            }

            // Prep output
            result.Properties = QuickLinksProperties();
            result.SearchablePlainTexts = searchablePlainTexts;
            result.ImageSources = imageSources.TrimStart(new char[] { ',' });
            result.Links = links;

            // Return the json properties for the converted web part
            return result;
        }

        #region Helper methods
        private void SetupQuickLinksProperties(string quickLinksJsonProperties)
        {
            // Do default setup
            // base properties setup
            this.properties.LayoutId = QuickLinksLayout.List.ToString();
            this.properties.ShouldShowThumbnail = true;
            this.properties.IsMigrated = true;
            this.properties.HideWebPartWhenEmpty = true;
            this.properties.DataProviderId = "QuickLinks";
            this.properties.WebId = this.clientContext.Web.Id.ToString();
            this.properties.SiteId = this.clientContext.Site.Id.ToString();
            this.properties.ButtonLayoutOptions = new ButtonLayoutOptions()
            {
                ShowDescription = false,
                ButtonTreatment = ButtonTreatment.Outline,
                IconPositionType = IconPositionType.IconToLeft,
                TextAlignmentVertical = ContentAlignment.Center,
                TextAlignmentHorizontal = ContentAlignment.Center,
                LinesOfText = LinesOfText.TwoLines,
            };
            this.properties.ListLayoutOptions = new ListLayoutOptions()
            {
                ShowDescription = true,
                ShowIcon = true,
            };
            this.properties.WaffleLayoutOptions = new WaffleLayoutOptions()
            {
                IconSize = IconSize.Medium,
                OnlyShowThumbnail = true,
            };

            if (!string.IsNullOrEmpty(quickLinksJsonProperties))
            {
                // Override defaults with properties obtained from the JSON blob
                try
                {
                    var parsedJson = JObject.Parse(quickLinksJsonProperties);

                    if (parsedJson["isMigrated"] != null)
                    {
                        this.properties.IsMigrated = ((JValue)parsedJson["isMigrated"]).Value<bool>();
                    }
                    if (parsedJson["layoutId"] != null)
                    {
                        this.properties.LayoutId = ((JValue)parsedJson["layoutId"]).Value<string>();
                    }
                    if (parsedJson["shouldShowThumbnail"] != null)
                    {
                        this.properties.ShouldShowThumbnail = ((JValue)parsedJson["shouldShowThumbnail"]).Value<bool>();
                    }
                    if (parsedJson["hideWebPartWhenEmpty"] != null)
                    {
                        this.properties.HideWebPartWhenEmpty = ((JValue)parsedJson["hideWebPartWhenEmpty"]).Value<bool>();
                    }
                    if (parsedJson["buttonLayoutOptions"] != null)
                    {
                        if (parsedJson["buttonLayoutOptions"]["showDescription"] != null)
                        {
                            this.properties.ButtonLayoutOptions.ShowDescription = ((JValue)parsedJson["buttonLayoutOptions"]["showDescription"]).Value<bool>();
                        }
                        if (parsedJson["buttonLayoutOptions"]["buttonTreatment"] != null)
                        {
                            this.properties.ButtonLayoutOptions.ButtonTreatment = (ButtonTreatment)Enum.Parse(typeof(ButtonTreatment), ((JValue)parsedJson["buttonLayoutOptions"]["buttonTreatment"]).Value<string>());
                        }
                        if (parsedJson["buttonLayoutOptions"]["iconPositionType"] != null)
                        {
                            this.properties.ButtonLayoutOptions.IconPositionType = (IconPositionType)Enum.Parse(typeof(IconPositionType), ((JValue)parsedJson["buttonLayoutOptions"]["iconPositionType"]).Value<string>());
                        }
                        if (parsedJson["buttonLayoutOptions"]["textAlignmentVertical"] != null)
                        {
                            this.properties.ButtonLayoutOptions.TextAlignmentVertical = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), ((JValue)parsedJson["buttonLayoutOptions"]["textAlignmentVertical"]).Value<string>());
                        }
                        if (parsedJson["buttonLayoutOptions"]["textAlignmentHorizontal"] != null)
                        {
                            this.properties.ButtonLayoutOptions.TextAlignmentHorizontal = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), ((JValue)parsedJson["buttonLayoutOptions"]["textAlignmentHorizontal"]).Value<string>());
                        }
                        if (parsedJson["buttonLayoutOptions"]["linesOfText"] != null)
                        {
                            this.properties.ButtonLayoutOptions.LinesOfText = (LinesOfText)Enum.Parse(typeof(LinesOfText), ((JValue)parsedJson["buttonLayoutOptions"]["linesOfText"]).Value<string>());
                        }
                    }
                    if (parsedJson["listLayoutOptions"] != null)
                    {
                        if (parsedJson["listLayoutOptions"]["showDescription"] != null)
                        {
                            this.properties.ListLayoutOptions.ShowDescription = ((JValue)parsedJson["listLayoutOptions"]["showDescription"]).Value<bool>();
                        }
                        if (parsedJson["listLayoutOptions"]["showIcon"] != null)
                        {
                            this.properties.ListLayoutOptions.ShowIcon = ((JValue)parsedJson["listLayoutOptions"]["showIcon"]).Value<bool>();
                        }
                    }
                    if (parsedJson["waffleLayoutOptions"] != null)
                    {
                        if (parsedJson["waffleLayoutOptions"]["iconSize"] != null)
                        {
                            this.properties.WaffleLayoutOptions.IconSize = (IconSize)Enum.Parse(typeof(IconSize), ((JValue)parsedJson["waffleLayoutOptions"]["iconSize"]).Value<string>());
                        }
                        if (parsedJson["waffleLayoutOptions"]["onlyShowThumbnail"] != null)
                        {
                            this.properties.WaffleLayoutOptions.OnlyShowThumbnail = ((JValue)parsedJson["waffleLayoutOptions"]["onlyShowThumbnail"]).Value<bool>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Let's not fail transformation in this case but do log this
                    // TODO: Find a replacement
                    // LogWarning(string.Format(LogStrings.Warning_OverridingQuickLinksDefaultsFailed, ex.Message), LogStrings.Heading_BuiltInFunctions);
                }
            }
        }

        private string JsonEncode(string input)
        {
            return HttpUtility.JavaScriptStringEncode(input);
        }

        private Dictionary<string, string> FileLookup(string serverRelativeFilePath)
        {

            bool stop = false;
            if (string.IsNullOrEmpty(serverRelativeFilePath))
            {
                stop = true;
            }

            this.clientContext.Web.EnsureProperty(p => p.ServerRelativeUrl);

            // Check if this url is pointing to content living in this site
            if (!stop && !serverRelativeFilePath.StartsWith(this.clientContext.Web.ServerRelativeUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                stop = true;
            }

            Dictionary<string, string> results = new Dictionary<string, string>();

            if (stop)
            {
                results.Add("FileListId", "");
                results.Add("FileUniqueId", "");
                results.Add("FileWebId", "");
                results.Add("FileSiteId", "");
                return results;
            }

            try
            {
                var file = this.clientContext.Web.GetFileByServerRelativeUrl(serverRelativeFilePath);
                this.clientContext.Load(file, p => p.UniqueId, p => p.ListId);
                this.clientContext.ExecuteQueryRetry();

                results.Add("FileListId", $"{{{file.ListId.ToString()}}}");
                results.Add("FileUniqueId", file.UniqueId.ToString());
                results.Add("FileWebId", this.properties.WebId);
                results.Add("FileSiteId", this.properties.SiteId);
                return results;
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    // Provided image was not found, should not happen
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        internal string QuickLinksProperties()
        {
            // Don't serialize null values
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this.properties, jsonSerializerSettings);

            return json;
        }
        #endregion
    }
}

