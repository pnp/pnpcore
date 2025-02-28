using AngleSharp.Dom;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// This class is used to instantiate controls of type 3 (= client side web parts). Using this class you can instantiate a control and 
    /// add it on a <see cref="IPage"/>.
    /// </summary>
    internal sealed class PageWebPart : CanvasControl, IPageWebPart
    {
        #region variables
        // Constants
        internal const string WebPartAttribute = "data-sp-webpart";
        internal const string WebPartDataVersionAttribute = "data-sp-webpartdataversion";
        internal const string WebPartDataAttribute = "data-sp-webpartdata";
        internal const string WebPartComponentIdAttribute = "data-sp-componentid";
        internal const string WebPartHtmlPropertiesAttribute = "data-sp-htmlproperties";

        private string propertiesJson;
        #endregion

        #region construction
        /// <summary>
        /// Instantiates client side web part from scratch.
        /// </summary>
        public PageWebPart() : base()
        {
            var emptyJson = JsonSerializer.Deserialize<JsonElement>("{}");

            controlType = 3;
            WebPartData = "";
            HtmlPropertiesData = "";
            HtmlProperties = "";
            Title = "";
            Description = "";
            SupportsFullBleed = false;
            SetPropertiesJson(emptyJson);
            WebPartPreviewImage = "";
            UsingSpControlDataOnly = false;
            DynamicDataPaths = emptyJson;
            DynamicDataValues = emptyJson;
            ServerProcessedContent = emptyJson;
        }

        /// <summary>
        /// Instantiates a client side web part based on the information that was obtain from calling the AvailableClientSideComponents methods on the <see cref="IPage"/> object.
        /// </summary>
        /// <param name="component">Component to create a ClientSideWebPart instance for</param>
        public PageWebPart(IPageComponent component) : this()
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            Import(component);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Value of the "data-sp-webpartdata" attribute
        /// </summary>
        public string JsonWebPartData { get; private set; }

        /// <summary>
        /// Value of the "data-sp-htmlproperties" element
        /// </summary>
        public string HtmlPropertiesData { get; private set; }

        /// <summary>
        /// Value of the "data-sp-htmlproperties" attribute
        /// </summary>
        public string HtmlProperties { get; private set; }

        /// <summary>
        /// ID of the client side web part
        /// </summary>
        public string WebPartId { get; internal set; }

        /// <summary>
        /// Supports full bleed display experience
        /// </summary>
        public bool SupportsFullBleed { get; private set; }

        /// <summary>
        /// Value of the "data-sp-webpart" attribute
        /// </summary>
        public string WebPartData { get; private set; }

        /// <summary>
        /// Title of the web part
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the web part
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Preview image that can serve as page preview image when the page holding this web part is promoted to a news page
        /// </summary>
        public string WebPartPreviewImage { get; private set; }

        /// <summary>
        /// Json serialized web part information. For 1st party web parts this ideally is the *full* JSON string 
        /// fetch via workbench or via copying it from an existing page. It's important that the serverProcessedContent
        /// element is included here!
        /// </summary>
        public string PropertiesJson
        {
            get
            {
                return Properties.ToString();
            }
            set
            {
                SetPropertiesJson(JsonSerializer.Deserialize<JsonElement>(value));
            }
        }

        /// <summary>
        /// Web properties as configurable <see cref="JsonElement"/>
        /// </summary>
        public JsonElement Properties { get; private set; }

        /// <summary>
        /// ServerProcessedContent json node
        /// </summary>
        public JsonElement ServerProcessedContent { get; private set; }

        //public JObject DynamicDataPaths
        public JsonElement DynamicDataPaths { get; private set; }

        //public JObject DynamicDataValues
        public JsonElement DynamicDataValues { get; private set; }

        /// <summary>
        /// Return <see cref="Type"/> of the client side web part
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(PageWebPart);
            }
        }

        /// <summary>
        /// Value of the "data-sp-controldata" attribute
        /// </summary>
        public WebPartControlData SpControlData { get; private set; }

        /// <summary>
        /// Indicates that this control is persisted/read using the data-sp-controldata attribute only
        /// </summary>
        internal bool UsingSpControlDataOnly { get; set; }

        /// <summary>
        /// This control lives in the page header (not removable control)
        /// </summary>
        public bool IsHeaderControl { get; set; }

        /// <summary>
        /// If this webpart is used inline in a text editor then this property points to the editor using it
        /// </summary>
        public string RichTextEditorInstanceId { get; set; }

        internal string ACEIconProperty { get; set; }

        internal string ACECardSize { get; set; }
        #endregion

        #region public methods
        /// <summary>
        /// Imports a <see cref="PageComponent"/> to use it as base for configuring the client side web part instance
        /// </summary>
        /// <param name="component"><see cref="PageComponent"/> to import</param>
        /// <param name="clientSideWebPartPropertiesUpdater">Function callback that allows you to manipulate the client side web part properties after import</param>
        public void Import(IPageComponent component, Func<string, string> clientSideWebPartPropertiesUpdater = null)
        {
            // Sometimes the id guid is encoded with curly brackets, so let's drop those
            WebPartId = new Guid(component.Id).ToString("D");

            // Parse the manifest json blob as we need some data from it
            var wpJObject = JsonSerializer.Deserialize<JsonElement>(component.Manifest);

            Title = wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("title").GetProperty("default").GetString();

            Description = wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("title").GetProperty("default").GetString();

            if (wpJObject.TryGetProperty("supportsFullBleed", out JsonElement supportsFullBleed))
            {
                SupportsFullBleed = supportsFullBleed.GetBoolean();
            }
            else if (Page.IdToDefaultWebPart(WebPartId) == DefaultWebPart.PageTitle)
            {
                SupportsFullBleed = true; //Message ID: MC791596 / Roadmap ID: 386904
            }
            else
            {
                SupportsFullBleed = false;
            }

            SetPropertiesJson(wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("properties"));

            if (clientSideWebPartPropertiesUpdater != null)
            {
                propertiesJson = clientSideWebPartPropertiesUpdater(propertiesJson);
            }
        }

        /// <summary>
        /// Returns a HTML representation of the client side web part
        /// </summary>
        /// <param name="controlIndex">The sequence of the control inside the section</param>
        /// <returns>HTML representation of the client side web part</returns>
        public override string ToHtml(float controlIndex)
        {
            if (!IsHeaderControl)
            {
                // Can this control be hosted in this section type?
                if (Section.Type == CanvasSectionTemplate.OneColumnFullWidth)
                {
                    if (!SupportsFullBleed)
                    {
                        throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Page_ControlNotAllowedInFullWidthSection);
                    }
                }

                WebPartControlData controlData;
                if (UsingSpControlDataOnly)
                {
                    controlData = new WebPartControlDataOnly();
                }
                else
                {
                    controlData = new WebPartControlData();
                }

                // Obtain the json data
                controlData.ControlType = ControlType;
                controlData.Id = InstanceId.ToString("D");
                controlData.WebPartId = WebPartId;
                controlData.Position = new CanvasControlPosition()
                {
                    ZoneIndex = Section.Order,
                    SectionIndex = Column.Order,
                    SectionFactor = Column.ColumnFactor,
                    LayoutIndex = Column.LayoutIndex,
                    ControlIndex = controlIndex,
                    ZoneId = column.ZoneId
                };

                if (SpControlData != null)
                {
                    controlData.RteInstanceId = SpControlData.RteInstanceId;
                    controlData.AddedFromPersistedData = SpControlData.AddedFromPersistedData;
                    controlData.ReservedHeight = SpControlData.ReservedHeight;
                    controlData.ReservedWidth = SpControlData.ReservedWidth;
                }
                else
                {
                    if (!string.IsNullOrEmpty(RichTextEditorInstanceId))
                    {
                        controlData.RteInstanceId = RichTextEditorInstanceId;
                        controlData.AddedFromPersistedData = true;
                    }
                }

                if (section.Type == CanvasSectionTemplate.OneColumnVerticalSection)
                {
                    if (section.Columns.First().Equals(Column))
                    {
                        controlData.Position.SectionFactor = 12;
                    }
                }

                controlData.Emphasis = new SectionEmphasis()
                {
                    ZoneEmphasis = Column.VerticalSectionEmphasis ?? Section.ZoneEmphasis,
                };

                // Persist the collapsible section settings
                if (Section.Collapsible && !Column.IsVerticalSectionColumn)
                {
                    controlData.ZoneGroupMetadata = new SectionZoneGroupMetadata()
                    {
                        // Set section type to 1 if it was not set (when new sections are added via code)
                        Type = (Section as CanvasSection).SectionType == 0 ? 1 : (Section as CanvasSection).SectionType,
                        DisplayName = Section.DisplayName,
                        IsExpanded = Section.IsExpanded,
                        ShowDividerLine = Section.ShowDividerLine,
                    };

                    if (Section.IconAlignment.HasValue)
                    {
                        controlData.ZoneGroupMetadata.IconAlignment = Section.IconAlignment.Value.ToString().ToLower();
                    }
                    else
                    {
                        controlData.ZoneGroupMetadata.IconAlignment = "true";
                    }
                }

                // Set the control's data version to the latest version...default was 1.0, but some controls use a higher version
                var webPartType = Page.IdToDefaultWebPart(controlData.WebPartId);

                // if we read the control from the page then the value might already be set to something different than 1.0...if so, leave as is
                if (DataVersion == "1.0")
                {
                    if (webPartType == DefaultWebPart.Image)
                    {
                        dataVersion = "1.9";
                    }
                    else if (webPartType == DefaultWebPart.ImageGallery)
                    {
                        dataVersion = "1.8";
                    }
                    else if (webPartType == DefaultWebPart.People)
                    {
                        dataVersion = "1.3";
                    }
                    else if (webPartType == DefaultWebPart.DocumentEmbed)
                    {
                        dataVersion = "1.2";
                    }
                    else if (webPartType == DefaultWebPart.ContentRollup)
                    {
                        dataVersion = "2.5";
                    }
                    else if (webPartType == DefaultWebPart.QuickLinks)
                    {
                        dataVersion = "2.2";
                    }
                }

                // Set the web part preview image url
                if (ServerProcessedContent.TryGetProperty("imageSources", out JsonElement imageSources))
                {
                    foreach (var property in imageSources.EnumerateObject())
                    {
                        if (!string.IsNullOrEmpty(property.Value.ToString()))
                        {
                            WebPartPreviewImage = property.Value.ToString().ToLower();
                            break;
                        }
                    }
                }

                WebPartData webpartData;
                if (string.IsNullOrEmpty(ACECardSize))
                {
                    webpartData = new WebPartData
                    { 
                        Id = controlData.WebPartId, 
                        InstanceId = controlData.Id, 
                        Title = Title, 
                        Description = Description, 
                        DataVersion = DataVersion, 
                        Properties = "jsonPropsToReplacePnPRules", 
                        DynamicDataPaths = "jsonDynamicDataPathsToReplacePnPRules", 
                        DynamicDataValues = "jsonDynamicDataValuesToReplacePnPRules", 
                        ServerProcessedContent = "jsonServerProcessedContentToReplacePnPRules" 
                    };
                }
                else
                {
                    // The web part represents an ACE

                    if (!string.IsNullOrEmpty(ACECardSize) && (controlData.ReservedWidth == 0 || controlData.ReservedHeight == 0))
                    {
                        // ACEs are configured with a reserved with and height, so set these depending on the card size
                        if (ACECardSize.Equals(CardSize.Medium.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            controlData.ReservedHeight = 180;
                            controlData.ReservedWidth = 164;
                        }
                        else if (ACECardSize.Equals(CardSize.Large.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            controlData.ReservedHeight = 180;
                            controlData.ReservedWidth = 344;
                        }
                    }

                    webpartData = new ACEWebPartData
                    {
                        Id = controlData.WebPartId,
                        InstanceId = controlData.Id,
                        Title = Title,
                        Description = Description,
                        DataVersion = DataVersion,
                        Properties = "jsonPropsToReplacePnPRules",
                        DynamicDataPaths = "jsonDynamicDataPathsToReplacePnPRules",
                        DynamicDataValues = "jsonDynamicDataValuesToReplacePnPRules",
                        ServerProcessedContent = "jsonServerProcessedContentToReplacePnPRules",
                        CardSize = ACECardSize,
                        IconProperty = ACEIconProperty
                    };
                }

                if (UsingSpControlDataOnly)
                {
                    (controlData as WebPartControlDataOnly).WebPartData = "jsonWebPartDataToReplacePnPRules";
                    jsonControlData = JsonSerializer.Serialize(controlData);
                    JsonWebPartData = JsonSerializer.Serialize(webpartData);
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", Properties.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", ServerProcessedContent.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonDynamicDataPathsToReplacePnPRules\"", DynamicDataPaths.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonDynamicDataValuesToReplacePnPRules\"", DynamicDataValues.ToString());
                    jsonControlData = jsonControlData.Replace("\"jsonWebPartDataToReplacePnPRules\"", JsonWebPartData);
                }
                else
                {
                    jsonControlData = JsonSerializer.Serialize(controlData);
                    if (webpartData is ACEWebPartData)
                    {
                        JsonWebPartData = JsonSerializer.Serialize(webpartData as ACEWebPartData);
                    }
                    else
                    {
                        JsonWebPartData = JsonSerializer.Serialize(webpartData);
                    }
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", Properties.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", ServerProcessedContent.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonDynamicDataPathsToReplacePnPRules\"", DynamicDataPaths.ToString());
                    JsonWebPartData = JsonWebPartData.Replace("\"jsonDynamicDataValuesToReplacePnPRules\"", DynamicDataValues.ToString());
                }
            }
            else
            {
                HeaderControlData webpartData = new HeaderControlData() { Id = WebPartId, InstanceId = InstanceId.ToString("D"), Title = Title, Description = Description, DataVersion = DataVersion, Properties = "jsonPropsToReplacePnPRules", ServerProcessedContent = "jsonServerProcessedContentToReplacePnPRules" };
                canvasDataVersion = DataVersion;
                JsonWebPartData = JsonSerializer.Serialize(webpartData);
                JsonWebPartData = JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", Properties.ToString());
                JsonWebPartData = JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", ServerProcessedContent.ToString());
                jsonControlData = JsonWebPartData;
            }

            StringBuilder html = new StringBuilder();
            if (UsingSpControlDataOnly || IsHeaderControl)
            {
                html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{CanvasDataVersion}"" {ControlDataAttribute}=""{JsonControlData.Replace("\"", "&quot;")}""></div>");
            }
            else
            {
                html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{CanvasDataVersion}"" {ControlDataAttribute}=""{JsonControlData.Replace("\"", "&quot;")}"">");
                html.Append($@"<div {WebPartAttribute}=""{WebPartData}"" {WebPartDataVersionAttribute}=""{DataVersion}"" {WebPartDataAttribute}=""{JsonWebPartData.Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;")}"">");
                html.Append($@"<div {WebPartComponentIdAttribute}=""{WebPartId}""></div>");
                html.Append($@"<div {WebPartHtmlPropertiesAttribute}=""{HtmlProperties}"">");
                RenderHtmlProperties(ref html);
                html.Append("</div>");
                html.Append("</div>");
                html.Append("</div>");
            }
            return html.ToString();
        }

        /// <summary>
        /// Overrideable method that allows inheriting webparts to control the HTML rendering
        /// </summary>
        /// <param name="htmlWriter">Reference to the html renderer used</param>
        internal void RenderHtmlProperties(ref StringBuilder htmlWriter)
        {
            if (!ServerProcessedContent.Equals(default))
            {
                if (ServerProcessedContent.TryGetProperty("searchablePlainTexts", out JsonElement searchablePlainTexts))
                {
                    foreach (var property in searchablePlainTexts.EnumerateObject())
                    {
                        htmlWriter.Append($@"<div data-sp-prop-name=""{property.Name}"" data-sp-searchableplaintext=""true"">");
                        htmlWriter.Append(property.Value.GetString());
                        htmlWriter.Append("</div>");
                    }
                }

                if (ServerProcessedContent.TryGetProperty("imageSources", out JsonElement imageSources))
                {
                    foreach (var property in imageSources.EnumerateObject())
                    {
                        htmlWriter.Append($@"<img data-sp-prop-name=""{property.Name}""");

                        if (!string.IsNullOrEmpty(property.Value.ToString()))
                        {
                            htmlWriter.Append($@" src=""{property.Value.GetString()}""");
                        }
                        htmlWriter.Append("></img>");
                    }
                }

                if (ServerProcessedContent.TryGetProperty("links", out JsonElement links))
                {
                    if (links.ValueKind != JsonValueKind.Null)
                    {
                        foreach (var property in links.EnumerateObject())
                        {
                            htmlWriter.Append($@"<a data-sp-prop-name=""{property.Name}"" href=""{property.Value.GetString()}""></a>");
                        }
                    }
                }

                if (ServerProcessedContent.TryGetProperty("htmlStrings", out JsonElement htmlStrings))
                {
                    foreach (var property in htmlStrings.EnumerateObject())
                    {
                        htmlWriter.Append($@"<div data-sp-prop-name=""{property.Name}"">{property.Value.GetString()}</div>");
                    }
                }
            }
            else
            {
                htmlWriter.Append(HtmlPropertiesData);
            }
        }
        #endregion

        #region Internal and private methods
        internal override void FromHtml(IElement element, bool isHeader)
        {
            base.FromHtml(element, isHeader);
                
            // Set/update dataVersion if it was provided as html attribute
            var webPartDataVersion = element.GetAttribute(WebPartDataVersionAttribute);
            if (!string.IsNullOrEmpty(webPartDataVersion))
            {
                dataVersion = element.GetAttribute(WebPartDataVersionAttribute);
            }

            SpControlData = JsonSerializer.Deserialize<WebPartControlData>(element.GetAttribute(ControlDataAttribute), PnPConstants.JsonSerializer_IgnoreNullValues);
            controlType = SpControlData.ControlType;
            RichTextEditorInstanceId = SpControlData.RteInstanceId;

            IElement wpDiv = null;
            string decodedWebPart = null;

            // Some components are in the page header and need to be handled as a control instead of a webpart
            if (isHeader)
            {
                // Decode the html encoded string
                decodedWebPart = WebUtility.HtmlDecode(element.GetAttribute(ControlDataAttribute));
                IsHeaderControl = true;
            }
            else
            {
                wpDiv = element.GetElementsByTagName("div").FirstOrDefault(a => a.HasAttribute(WebPartDataAttribute));
                if (wpDiv != null)
                {
                    // This is a valid web part
                    WebPartData = wpDiv.GetAttribute(WebPartAttribute);

                    // Decode the html encoded string
                    decodedWebPart = WebUtility.HtmlDecode(wpDiv.GetAttribute(WebPartDataAttribute));
                }
                else
                {
                    // The web part is not presented by a data-sp-webpartdata attribute on the DIV, typically
                    // this means the web part is broken. Check the page and see if it renders properly

                    // Let's try to get the web part data from the webPartData element in the control data attribute json content
                    var controlDataAttributeJsonContent = WebUtility.HtmlDecode(element.GetAttribute(ControlDataAttribute));
                    if (!string.IsNullOrEmpty(controlDataAttributeJsonContent))
                    {
                        var controlDataAttributeJsonObject = JsonSerializer.Deserialize<JsonElement>(controlDataAttributeJsonContent);
                        if (controlDataAttributeJsonObject.TryGetProperty("webPartData", out JsonElement webPartDataProperty) && webPartDataProperty.ValueKind != JsonValueKind.Null)
                        {
                            decodedWebPart = webPartDataProperty.ToString();
                        }
                    }
                }
            }

            // If above fallback code did not result in web part data then return with just the basic info we have
            if (string.IsNullOrEmpty(decodedWebPart))
            {
                return;
            }

            var wpJObject = JsonSerializer.Deserialize<JsonElement>(decodedWebPart);

            if (wpJObject.TryGetProperty("title", out JsonElement titleProperty))
            {
                Title = titleProperty.GetString();
            }
            else
            {
                Title = "";
            }

            if (wpJObject.TryGetProperty("description", out JsonElement descriptionProperty))
            {
                Description = descriptionProperty.GetString();
            }
            else
            {
                Description = "";
            }

            // Set property to trigger correct loading of properties 
            PropertiesJson = wpJObject.GetProperty("properties").ToString();

            if (wpJObject.TryGetProperty("id", out JsonElement webPartId))
            {
                WebPartId = webPartId.GetString();
            }

            // Set/update dataVersion if it was set in the json data
            if (wpJObject.TryGetProperty("dataVersion", out JsonElement dataVersionValue))
            {
                dataVersion = dataVersionValue.GetString();
            }

            if (controlType == 14) // Special control type for background image
            {
                SupportsFullBleed = true;
            }
            else
            {
                // Check for fullbleed supporting web parts
                if (wpJObject.TryGetProperty("properties", out JsonElement properties))
                {
                    if (properties.TryGetProperty("isFullWidth", out JsonElement isFullWidth))
                    {
                        SupportsFullBleed = isFullWidth.GetBoolean();
                    }
                    // Ensure that for first party web parts that support full bleed we set the SupportsFullBleed flag
                    else if (Page.IdToDefaultWebPart(WebPartId) == DefaultWebPart.PageTitle || //Message ID: MC791596 / Roadmap ID: 386904
                             Page.IdToDefaultWebPart(WebPartId) == DefaultWebPart.Image ||
                             Page.IdToDefaultWebPart(WebPartId) == DefaultWebPart.Hero ||
                             Page.IdToDefaultWebPart(WebPartId) == DefaultWebPart.CountDown)
                    {
                        SupportsFullBleed = true;
                    }
                }
            }

            // Store the server processed content as that's needed for full fidelity
            if (wpJObject.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
            {
                ServerProcessedContent = serverProcessedContent;
            }

            if (wpJObject.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
            {
                DynamicDataPaths = dynamicDataPaths;
            }

            if (wpJObject.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
            {
                DynamicDataValues = dynamicDataValues;
            }

            if (wpDiv != null)
            {
                var wpHtmlProperties = wpDiv.GetElementsByTagName("div").FirstOrDefault(a => a.HasAttribute(WebPartHtmlPropertiesAttribute));
                HtmlPropertiesData = wpHtmlProperties.InnerHtml;
                HtmlProperties = wpHtmlProperties.GetAttribute(WebPartHtmlPropertiesAttribute);
            }

            if (wpJObject.TryGetProperty("iconProperty", out JsonElement ACEIconPropertyElement))
            {
                ACEIconProperty = ACEIconPropertyElement.GetString();
            }

            if (wpJObject.TryGetProperty("cardSize", out JsonElement ACECardSizeElement))
            {
                ACECardSize = ACECardSizeElement.GetString();
            }
        }

        private void SetPropertiesJson(JsonElement parsedJson)
        {
            if (parsedJson.ValueKind == JsonValueKind.Null)
            {
                return;
            }

            propertiesJson = parsedJson.ToString();

            if (parsedJson.TryGetProperty("webPartData", out JsonElement webPartData))
            {
                if (webPartData.TryGetProperty("properties", out JsonElement properties))
                {
                    Properties = properties;
                }

                if (webPartData.TryGetProperty("dataVersion", out JsonElement dataVersion))
                {
                    this.dataVersion = dataVersion.GetString().Trim('"');
                }

                if (webPartData.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                {
                    ServerProcessedContent = serverProcessedContent;
                }

                if (webPartData.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
                {
                    DynamicDataPaths = dynamicDataPaths;
                }

                if (webPartData.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
                {
                    DynamicDataValues = dynamicDataValues;
                }
            }
            else
            {
                if (parsedJson.TryGetProperty("properties", out JsonElement properties))
                {
                    Properties = properties;
                }
                else
                {
                    Properties = parsedJson;
                }

                if (parsedJson.TryGetProperty("dataVersion", out JsonElement dataVersion))
                {
                    this.dataVersion = dataVersion.GetString().Trim('"');
                }

                if (parsedJson.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                {
                    ServerProcessedContent = serverProcessedContent;
                }

                if (parsedJson.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
                {
                    DynamicDataPaths = dynamicDataPaths;
                }

                if (parsedJson.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
                {
                    DynamicDataValues = dynamicDataValues;
                }
            }
        }
        #endregion
    }
}
