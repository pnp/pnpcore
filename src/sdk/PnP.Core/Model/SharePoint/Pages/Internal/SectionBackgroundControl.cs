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
    internal sealed class SectionBackgroundControl : CanvasControl, IPageWebPart
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
        /// Instantiates PageBackground Control.
        /// </summary>
        public SectionBackgroundControl()
        {
            var emptyJson = JsonSerializer.Deserialize<JsonElement>("{}");

            controlType = 14;
            WebPartData = "";
            HtmlPropertiesData = "";
            HtmlProperties = "";
            SetPropertiesJson(emptyJson);
            ServerProcessedContent = emptyJson;
            DynamicDataValues = emptyJson;
            DynamicDataPaths = emptyJson;
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

        public JsonElement DynamicDataPaths { get; private set; }
        public JsonElement DynamicDataValues { get; private set; }

        /// <summary>
        /// Return <see cref="Type"/> of the client side web part
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(SectionBackgroundControl);
            }
        }

        public bool IsHeaderControl { get; set; }
        public string RichTextEditorInstanceId { get; set; }

        /// <summary>
        /// Value of the "data-sp-controldata" attribute
        /// </summary>
        public WebPartControlData SpControlData { get; private set; }

        #endregion

        #region public methods
       
        /// <summary>
        /// Returns a HTML representation of the client side web part
        /// </summary>
        /// <param name="controlIndex">The sequence of the control inside the section</param>
        /// <returns>HTML representation of the client side web part</returns>
        public override string ToHtml(float controlIndex)
        {
            WebPartControlData controlData = new() {
                ControlType = controlType, 
                Id = InstanceId.ToString("D"),
                WebPartId = WebPartId,
                Emphasis = new SectionEmphasis
                {
                    ZoneEmphasis = Section.ZoneEmphasis
                },
                Position = new CanvasControlPosition()
                {
                    ZoneIndex = Section.Order,
                    SectionIndex = Column.Order,
                    SectionFactor = Column.ColumnFactor,
                    LayoutIndex = Column.LayoutIndex,
                    ControlIndex = controlIndex,
                    ZoneId = column.ZoneId
                }
            };

            WebPartData webpartData = new()
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
            
            jsonControlData = JsonSerializer.Serialize(controlData);
            JsonWebPartData = JsonSerializer.Serialize(webpartData);
            JsonWebPartData = JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", Properties.ToString());
            JsonWebPartData = JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", ServerProcessedContent.ToString());
            
            StringBuilder html = new StringBuilder();
            
                html.Append($@"<div {CanvasControlAttribute}=""{CanvasControlData}"" {CanvasDataVersionAttribute}=""{CanvasDataVersion}"" {ControlDataAttribute}=""{JsonControlData.Replace("\"", "&quot;")}"">");
                html.Append($@"<div {WebPartAttribute}=""{WebPartData}"" {WebPartDataVersionAttribute}=""{DataVersion}"" {WebPartDataAttribute}=""{JsonWebPartData.Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;")}"">");
                html.Append($@"<div {WebPartComponentIdAttribute}=""""></div>");
                html.Append($@"<div {WebPartHtmlPropertiesAttribute}=""{HtmlProperties}"">");
                RenderHtmlProperties(ref html);
                html.Append("</div>");
                html.Append("</div>");
                html.Append("</div>");
            
            return html.ToString();
        }

        /// <summary>
        /// Overrideable method that allows inheriting webparts to control the HTML rendering
        /// </summary>
        /// <param name="htmlWriter">Reference to the html renderer used</param>
        internal void RenderHtmlProperties(ref StringBuilder htmlWriter)
        {
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

            // Store the server processed content as that's needed for full fidelity
            if (wpJObject.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
            {
                ServerProcessedContent = serverProcessedContent;
            }

            if (wpDiv != null)
            {
                var wpHtmlProperties = wpDiv.GetElementsByTagName("div").FirstOrDefault(a => a.HasAttribute(WebPartHtmlPropertiesAttribute));
                HtmlPropertiesData = wpHtmlProperties.InnerHtml;
                HtmlProperties = wpHtmlProperties.GetAttribute(WebPartHtmlPropertiesAttribute);
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
