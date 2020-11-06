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
    internal class ClientSideWebPart : CanvasControl, IClientSideWebPart
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
        public ClientSideWebPart() : base()
        {
            this.controlType = 3;
            this.WebPartData = "";
            this.HtmlPropertiesData = "";
            this.HtmlProperties = "";
            this.Title = "";
            this.Description = "";
            this.SupportsFullBleed = false;
            //this.SetPropertiesJson("{}");
            this.SetPropertiesJson(JsonDocument.Parse("{}").RootElement);
            this.WebPartPreviewImage = "";
            this.UsingSpControlDataOnly = false;
            //this.dynamicDataPaths = JObject.Parse("{}");
            //this.dynamicDataValues = JObject.Parse("{}");
            //this.serverProcessedContent = JObject.Parse("{}");
            this.DynamicDataPaths = JsonDocument.Parse("{}").RootElement;
            this.DynamicDataValues = JsonDocument.Parse("{}").RootElement;
            this.ServerProcessedContent = JsonDocument.Parse("{}").RootElement;
        }

        /// <summary>
        /// Instantiates a client side web part based on the information that was obtain from calling the AvailableClientSideComponents methods on the <see cref="ClientSidePage"/> object.
        /// </summary>
        /// <param name="component">Component to create a ClientSideWebPart instance for</param>
        public ClientSideWebPart(IClientSideComponent component) : this()
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            this.Import(component as ClientSideComponent);
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
        public string WebPartId { get; private set; }

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
                return this.Properties.ToString(/*Formatting.None*/);
            }
            set
            {
                //this.SetPropertiesJson(value);
                this.SetPropertiesJson(JsonDocument.Parse(value).RootElement);
            }
        }

        /// <summary>
        /// Web properties as configurable <see cref="JsonElement"/>
        /// </summary>
        //public JObject Properties
        public JsonElement Properties { get; private set; }

        /// <summary>
        /// ServerProcessedContent json node
        /// </summary>
        //public JObject ServerProcessedContent
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
                return typeof(ClientSideWebPart);
            }
        }

        /// <summary>
        /// Value of the "data-sp-controldata" attribute
        /// </summary>
        public ClientSideWebPartControlData SpControlData { get; private set; }

        /// <summary>
        /// Indicates that this control is persisted/read using the data-sp-controldata attribute only
        /// </summary>
        public bool UsingSpControlDataOnly { get; set; }

        /// <summary>
        /// This control lives in the page header (not removable control)
        /// </summary>
        public bool IsHeaderControl { get; internal set; }

        #endregion

        #region public methods
        /// <summary>
        /// Imports a <see cref="ClientSideComponent"/> to use it as base for configuring the client side web part instance
        /// </summary>
        /// <param name="component"><see cref="ClientSideComponent"/> to import</param>
        /// <param name="clientSideWebPartPropertiesUpdater">Function callback that allows you to manipulate the client side web part properties after import</param>
        public void Import(ClientSideComponent component, Func<String, String> clientSideWebPartPropertiesUpdater = null)
        {
            // Sometimes the id guid is encoded with curly brackets, so let's drop those
            this.WebPartId = new Guid(component.Id).ToString("D");

            // Parse the manifest json blob as we need some data from it
            //JObject wpJObject = JObject.Parse(component.Manifest);
            var wpJObject = JsonDocument.Parse(component.Manifest).RootElement;

            //this.title = wpJObject["preconfiguredEntries"][0]["title"]["default"].Value<string>();
            this.Title = wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("title").GetProperty("default").GetString();

            //this.description = wpJObject["preconfiguredEntries"][0]["title"]["default"].Value<string>();
            this.Description = wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("title").GetProperty("default").GetString();

            //if (wpJObject["supportsFullBleed"] != null)
            //{
            //    this.supportsFullBleed = wpJObject["supportsFullBleed"].Value<bool>();
            //}
            //else
            //{
            //    this.supportsFullBleed = false;
            //}
            if (wpJObject.TryGetProperty("supportsFullBleed", out JsonElement supportsFullBleed))
            {
                this.SupportsFullBleed = supportsFullBleed.GetBoolean();
            }
            else
            {
                this.SupportsFullBleed = false;
            }

            //this.SetPropertiesJson(wpJObject["preconfiguredEntries"][0]["properties"].ToString(/*Formatting.None*/));
            this.SetPropertiesJson(wpJObject.GetProperty("preconfiguredEntries").EnumerateArray().First().GetProperty("properties"));

            if (clientSideWebPartPropertiesUpdater != null)
            {
                this.propertiesJson = clientSideWebPartPropertiesUpdater(this.propertiesJson);
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
                if (this.Section.Type == CanvasSectionTemplate.OneColumnFullWidth)
                {
                    if (!this.SupportsFullBleed)
                    {
                        throw new Exception("You cannot host this web part inside a one column full width section, only webparts that support full bleed are allowed");
                    }
                }

                ClientSideWebPartControlData controlData = null;
                if (this.UsingSpControlDataOnly)
                {
                    controlData = new ClientSideWebPartControlDataOnly();
                }
                else
                {
                    controlData = new ClientSideWebPartControlData();
                }

                // Obtain the json data
                controlData.ControlType = this.ControlType;
                controlData.Id = this.InstanceId.ToString("D");
                controlData.WebPartId = this.WebPartId;
                controlData.Position = new ClientSideCanvasControlPosition()
                {
                    ZoneIndex = this.Section.Order,
                    SectionIndex = this.Column.Order,
                    SectionFactor = this.Column.ColumnFactor,
                    LayoutIndex = this.Column.LayoutIndex,
                    ControlIndex = controlIndex,
                };

                if (this.section.Type == CanvasSectionTemplate.OneColumnVerticalSection)
                {
                    if (this.section.Columns.First().Equals(this.Column))
                    {
                        controlData.Position.SectionFactor = 12;
                    }
                }

                controlData.Emphasis = new ClientSideSectionEmphasis()
                {
                    ZoneEmphasis = this.Column.VerticalSectionEmphasis.HasValue ? this.Column.VerticalSectionEmphasis.Value : this.Section.ZoneEmphasis,
                };

                // Set the control's data version to the latest version...default was 1.0, but some controls use a higher version
                //var webPartType = ClientSidePage.NameToClientSideWebPartEnum(controlData.WebPartId);
                var webPartType = Page.IdToDefaultWebPart(controlData.WebPartId);

                // if we read the control from the page then the value might already be set to something different than 1.0...if so, leave as is
                if (this.DataVersion == "1.0")
                {
                    if (webPartType == DefaultWebPart.Image)
                    {
                        this.dataVersion = "1.8";
                    }
                    else if (webPartType == DefaultWebPart.ImageGallery)
                    {
                        this.dataVersion = "1.6";
                    }
                    else if (webPartType == DefaultWebPart.People)
                    {
                        this.dataVersion = "1.2";
                    }
                    else if (webPartType == DefaultWebPart.DocumentEmbed)
                    {
                        this.dataVersion = "1.1";
                    }
                    else if (webPartType == DefaultWebPart.ContentRollup)
                    {
                        this.dataVersion = "2.1";
                    }
                    else if (webPartType == DefaultWebPart.QuickLinks)
                    {
                        this.dataVersion = "2.0";
                    }
                }

                // Set the web part preview image url
                //if (this.ServerProcessedContent != null && this.ServerProcessedContent["imageSources"] != null)
                if (this.ServerProcessedContent.TryGetProperty("imageSources", out JsonElement imageSources))
                {
                    //foreach (JProperty property in this.ServerProcessedContent["imageSources"])
                    foreach(var property in imageSources.EnumerateObject())
                    {
                        if (!string.IsNullOrEmpty(property.Value.ToString()))
                        {
                            this.WebPartPreviewImage = property.Value.ToString().ToLower();
                            break;
                        }
                    }
                }

                ClientSideWebPartData webpartData = new ClientSideWebPartData() { Id = controlData.WebPartId, InstanceId = controlData.Id, Title = this.Title, Description = this.Description, DataVersion = this.DataVersion, Properties = "jsonPropsToReplacePnPRules", DynamicDataPaths = "jsonDynamicDataPathsToReplacePnPRules", DynamicDataValues = "jsonDynamicDataValuesToReplacePnPRules", ServerProcessedContent = "jsonServerProcessedContentToReplacePnPRules" };

                if (this.UsingSpControlDataOnly)
                {
                    (controlData as ClientSideWebPartControlDataOnly).WebPartData = "jsonWebPartDataToReplacePnPRules";
                    //this.jsonControlData = JsonConvert.SerializeObject(controlData);
                    this.jsonControlData = JsonSerializer.Serialize(controlData);
                    //this.jsonWebPartData = JsonConvert.SerializeObject(webpartData);
                    this.JsonWebPartData = JsonSerializer.Serialize(webpartData);
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", this.Properties.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", this.ServerProcessedContent.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonDynamicDataPathsToReplacePnPRules\"", this.DynamicDataPaths.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonDynamicDataValuesToReplacePnPRules\"", this.DynamicDataValues.ToString(/*Formatting.None*/));
                    this.jsonControlData = this.jsonControlData.Replace("\"jsonWebPartDataToReplacePnPRules\"", this.JsonWebPartData);
                }
                else
                {
                    //this.jsonControlData = JsonConvert.SerializeObject(controlData);
                    this.jsonControlData = JsonSerializer.Serialize(controlData);
                    //this.jsonWebPartData = JsonConvert.SerializeObject(webpartData);
                    this.JsonWebPartData = JsonSerializer.Serialize(webpartData);
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", this.Properties.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", this.ServerProcessedContent.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonDynamicDataPathsToReplacePnPRules\"", this.DynamicDataPaths.ToString(/*Formatting.None*/));
                    this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonDynamicDataValuesToReplacePnPRules\"", this.DynamicDataValues.ToString(/*Formatting.None*/));
                }
            }
            else
            {
                HeaderControlData webpartData = new HeaderControlData() { Id = this.WebPartId, InstanceId = this.InstanceId.ToString("D"), Title = this.Title, Description = this.Description, DataVersion = this.DataVersion, Properties = "jsonPropsToReplacePnPRules", ServerProcessedContent = "jsonServerProcessedContentToReplacePnPRules" };
                this.canvasDataVersion = this.DataVersion;
                //this.jsonWebPartData = JsonConvert.SerializeObject(webpartData);
                this.JsonWebPartData = JsonSerializer.Serialize(webpartData);
                this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonPropsToReplacePnPRules\"", this.Properties.ToString(/*Formatting.None*/));
                this.JsonWebPartData = this.JsonWebPartData.Replace("\"jsonServerProcessedContentToReplacePnPRules\"", this.ServerProcessedContent.ToString(/*Formatting.None*/));
                this.jsonControlData = this.JsonWebPartData;
            }

            StringBuilder html = new StringBuilder(100);
            if (this.UsingSpControlDataOnly || IsHeaderControl)
            {
                html.Append($@"<div {CanvasControlAttribute}=""{this.CanvasControlData}"" {CanvasDataVersionAttribute}=""{this.DataVersion}"" {ControlDataAttribute}=""{this.JsonControlData.Replace("\"", "&quot;")}""></div>");
            }
            else
            {
                html.Append($@"<div {CanvasControlAttribute}=""{this.CanvasControlData}"" {CanvasDataVersionAttribute}=""{this.DataVersion}"" {ControlDataAttribute}=""{this.JsonControlData.Replace("\"", "&quot;")}"">");
                html.Append($@"<div {WebPartAttribute}=""{this.WebPartData}"" {WebPartDataVersionAttribute}=""{this.DataVersion}"" {WebPartDataAttribute}=""{this.JsonWebPartData.Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;")}"">");
                html.Append($@"<div {WebPartComponentIdAttribute}="""">");
                html.Append(this.WebPartId);
                html.Append("</div>");
                html.Append($@"<div {WebPartHtmlPropertiesAttribute}=""{this.HtmlProperties}"">");
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
        protected virtual void RenderHtmlProperties(ref StringBuilder htmlWriter)
        {
            //if (this.ServerProcessedContent != null)
            if (!this.ServerProcessedContent.Equals(default))
            {
                //if (this.ServerProcessedContent["searchablePlainTexts"] != null)
                if (this.ServerProcessedContent.TryGetProperty("searchablePlainTexts", out JsonElement searchablePlainTexts))
                {
                    //foreach (JProperty property in this.ServerProcessedContent["searchablePlainTexts"])
                    foreach (var property in searchablePlainTexts.EnumerateObject())
                    {
                        htmlWriter.Append($@"<div data-sp-prop-name=""{property.Name}"" data-sp-searchableplaintext=""true"">");
                        htmlWriter.Append(property.Value.GetString());
                        htmlWriter.Append("</div>");
                    }
                }

                //if (this.ServerProcessedContent["imageSources"] != null)
                if (this.ServerProcessedContent.TryGetProperty("imageSources", out JsonElement imageSources))
                {
                    //foreach (JProperty property in this.ServerProcessedContent["imageSources"])
                    foreach (var property in imageSources.EnumerateObject())
                    {
                        htmlWriter.Append($@"<img data-sp-prop-name=""{property.Name}""");

                        if (!string.IsNullOrEmpty(property.Value.ToString()))
                        {
                            //htmlWriter.Append($@" src=""{property.Value.Value<string>()}""");
                            htmlWriter.Append($@" src=""{property.Value.GetString()}""");
                        }
                        htmlWriter.Append("></img>");
                    }
                }

                //if (this.ServerProcessedContent["links"] != null)
                if (this.ServerProcessedContent.TryGetProperty("links", out JsonElement links))
                {
                    //foreach (JProperty property in this.ServerProcessedContent["links"])
                    foreach (var property in links.EnumerateObject())
                    {
                        //htmlWriter.Append($@"<a data-sp-prop-name=""{property.Name}"" href=""{property.Value.Value<string>()}""></a>");
                        htmlWriter.Append($@"<a data-sp-prop-name=""{property.Name}"" href=""{property.Value.GetString()}""></a>");
                    }
                }

                //if (this.ServerProcessedContent["htmlStrings"] != null)
                if (this.ServerProcessedContent.TryGetProperty("htmlStrings", out JsonElement htmlStrings))
                {
                    //foreach (JProperty property in this.ServerProcessedContent["htmlStrings"])
                    foreach (var property in htmlStrings.EnumerateObject())
                    {
                        //htmlWriter.Append($@"<div data-sp-prop-name=""{property.Name}"">{property.Value.ToString()}</div>");
                        htmlWriter.Append($@"<div data-sp-prop-name=""{property.Name}"">{property.Value.GetString()}</div>");
                    }
                }
            }
            else
            {
                htmlWriter.Append(this.HtmlPropertiesData);
            }
        }
        #endregion

        #region Internal and private methods
        internal override void FromHtml(IElement element)
        {
            base.FromHtml(element);

            // Set/update dataVersion if it was provided as html attribute
            var webPartDataVersion = element.GetAttribute(WebPartDataVersionAttribute);
            if (!string.IsNullOrEmpty(webPartDataVersion))
            {
                this.dataVersion = element.GetAttribute(WebPartDataVersionAttribute);
            }

            // load data from the data-sp-controldata attribute
            //var jsonSerializerSettings = new JsonSerializerSettings()
            //{
            //    MissingMemberHandling = MissingMemberHandling.Ignore
            //};
            var jsonSerializerSettings = new JsonSerializerOptions() { IgnoreNullValues = true };
            //this.spControlData = JsonConvert.DeserializeObject<ClientSideWebPartControlData>(element.GetAttribute(CanvasControl.ControlDataAttribute), jsonSerializerSettings);
            this.SpControlData = JsonSerializer.Deserialize<ClientSideWebPartControlData>(element.GetAttribute(CanvasControl.ControlDataAttribute), jsonSerializerSettings);
            this.controlType = this.SpControlData.ControlType;

            var wpDiv = element.GetElementsByTagName("div").Where(a => a.HasAttribute(ClientSideWebPart.WebPartDataAttribute)).FirstOrDefault();

            // Some components on the page (e.g. web parts configured with isDomainIsolated = true) are rendered differently in the HTML
            if (wpDiv == null)
            {
                throw new Exception("Oops...seems we end up here anyway");
                /* DON'T THINK WE STILL NEED THIS
                JObject wpJObject = JObject.Parse(element.GetAttribute(CanvasControl.ControlDataAttribute));
                if (wpJObject["webPartData"] != null)
                {
                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["title"] != null)
                    {
                        this.title = wpJObject["webPartData"]["title"].Value<string>();
                    }
                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["description"] != null)
                    {
                        this.description = wpJObject["webPartData"]["description"].Value<string>();
                    }
                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["properties"] != null)
                    {
                        this.PropertiesJson = wpJObject["webPartData"]["properties"].ToString(Formatting.None);
                    }
                    // Check for fullbleed supporting web parts
                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["properties"] != null && wpJObject["webPartData"]["properties"]["isFullWidth"] != null)
                    {
                        this.supportsFullBleed = wpJObject["webPartData"]["properties"]["isFullWidth"].Value<Boolean>();
                    }

                    // Store the server processed content as that's needed for full fidelity
                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["serverProcessedContent"] != null && wpJObject["webPartData"]["serverProcessedContent"].ToString() != "")
                    {
                        this.serverProcessedContent = (JObject)wpJObject["webPartData"]["serverProcessedContent"];
                    }

                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["dynamicDataPaths"] != null)
                    {
                        this.dynamicDataPaths = (JObject)wpJObject["webPartData"]["dynamicDataPaths"];
                    }

                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["dynamicDataValues"] != null)
                    {
                        this.dynamicDataValues = (JObject)wpJObject["webPartData"]["dynamicDataValues"];
                    }

                    if (wpJObject["webPartData"] != null && wpJObject["webPartData"]["id"] != null)
                    {
                        this.webPartId = wpJObject["webPartData"]["id"].Value<string>();
                    }
                }
                else
                {
                    // Header controls (like in topic pages)
                    if (wpJObject["title"] != null)
                    {
                        this.title = wpJObject["title"].Value<string>();
                    }
                    if (wpJObject["description"] != null)
                    {
                        this.description = wpJObject["description"].Value<string>();
                    }
                    if (wpJObject["properties"] != null)
                    {
                        this.PropertiesJson = wpJObject["properties"].ToString(Formatting.None);
                    }
                    // Check for fullbleed supporting web parts
                    if (wpJObject["properties"] != null && wpJObject["properties"]["isFullWidth"] != null)
                    {
                        this.supportsFullBleed = wpJObject["properties"]["isFullWidth"].Value<Boolean>();
                    }

                    // Store the server processed content as that's needed for full fidelity
                    if (wpJObject["serverProcessedContent"] != null && wpJObject["serverProcessedContent"].ToString() != "")
                    {
                        this.serverProcessedContent = (JObject)wpJObject["serverProcessedContent"];
                    }

                    // Set/update dataVersion if it was set in the json data
                    if (wpJObject["dataVersion"] != null && !string.IsNullOrEmpty(wpJObject["dataVersion"].ToString(Formatting.None)))
                    {
                        this.dataVersion = wpJObject["dataVersion"].ToString(Formatting.None).Trim('"');
                    }

                    if (wpJObject["id"] != null)
                    {
                        this.webPartId = wpJObject["id"].Value<string>();
                    }

                    if (wpJObject["instanceId"] != null)
                    {
                        if (Guid.TryParse(wpJObject["instanceId"].Value<string>(), out Guid instanceGuid))
                        {
                            this.instanceId = instanceGuid;
                        }
                    }
                }
                this.usingSpControlDataOnly = true;
                */
            }
            else
            {
                this.WebPartData = wpDiv.GetAttribute(ClientSideWebPart.WebPartAttribute);

                // Decode the html encoded string
                var decoded = WebUtility.HtmlDecode(wpDiv.GetAttribute(ClientSideWebPart.WebPartDataAttribute));
                //JObject wpJObject = JObject.Parse(decoded);
                var wpJObject = JsonDocument.Parse(decoded).RootElement;

                //this.title = wpJObject["title"] != null ? wpJObject["title"].Value<string>() : "";
                if (wpJObject.TryGetProperty("title", out JsonElement titleProperty))
                {
                    this.Title = titleProperty.GetString();
                }
                else
                {
                    this.Title = "";
                }

                //this.description = wpJObject["description"] != null ? wpJObject["description"].Value<string>() : "";
                if (wpJObject.TryGetProperty("description", out JsonElement descriptionProperty))
                {
                    this.Description = descriptionProperty.GetString();
                }
                else
                {
                    this.Description = "";
                }

                // Set property to trigger correct loading of properties 
                //this.PropertiesJson = wpJObject["properties"].ToString(Formatting.None);
                this.PropertiesJson = wpJObject.GetProperty("properties").ToString();

                // Set/update dataVersion if it was set in the json data
                //if (!string.IsNullOrEmpty(wpJObject["dataVersion"].ToString(/*Formatting.None*/)))
                //{
                //    this.dataVersion = wpJObject["dataVersion"].ToString(/*Formatting.None*/).Trim('"');
                //}
                if (wpJObject.TryGetProperty("dataVersion", out JsonElement dataVersion))
                {
                    this.dataVersion = dataVersion.GetString();
                }

                // Check for fullbleed supporting web parts
                //if (wpJObject["properties"] != null && wpJObject["properties"]["isFullWidth"] != null)
                //{
                //    this.supportsFullBleed = wpJObject["properties"]["isFullWidth"].Value<Boolean>();
                //}
                if (wpJObject.TryGetProperty("properties", out JsonElement properties))
                {
                    if (properties.TryGetProperty("isFullWidth", out JsonElement isFullWidth))
                    {
                        this.SupportsFullBleed = isFullWidth.GetBoolean();
                    }
                }

                // Store the server processed content as that's needed for full fidelity
                //if (wpJObject["serverProcessedContent"] != null && wpJObject["serverProcessedContent"].ToString() != "")
                //{
                //    this.serverProcessedContent = (JObject)wpJObject["serverProcessedContent"];
                //}

                if (wpJObject.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                {
                    this.ServerProcessedContent = serverProcessedContent;
                }

                //if (wpJObject["dynamicDataPaths"] != null)
                //{
                //    this.dynamicDataPaths = (JObject)wpJObject["dynamicDataPaths"];
                //}
                if (wpJObject.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
                {
                    this.DynamicDataPaths = dynamicDataPaths;
                }

                //if (wpJObject["dynamicDataValues"] != null)
                //{
                //    this.dynamicDataValues = (JObject)wpJObject["dynamicDataValues"];
                //}
                if (wpJObject.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
                {
                    this.DynamicDataValues = dynamicDataValues;
                }

                //this.webPartId = wpJObject["id"].Value<string>();
                this.WebPartId = wpJObject.GetProperty("id").GetString();

                var wpHtmlProperties = wpDiv.GetElementsByTagName("div").Where(a => a.HasAttribute(ClientSideWebPart.WebPartHtmlPropertiesAttribute)).FirstOrDefault();
                this.HtmlPropertiesData = wpHtmlProperties.InnerHtml;
                this.HtmlProperties = wpHtmlProperties.GetAttribute(ClientSideWebPart.WebPartHtmlPropertiesAttribute);
            }
        }

        private void SetPropertiesJson(/*string json*/JsonElement parsedJson)
        {
            //if (String.IsNullOrEmpty(json))
            //{
            //    json = "{}";
            //}

            this.propertiesJson = parsedJson.ToString();

            //var parsedJson = JObject.Parse(json);

            // If the passed structure is the top level JSON structure, which it typically is, then grab the properties from it
            //if (parsedJson["webPartData"] != null && parsedJson["webPartData"]["properties"] != null)
            //{
            //    this.properties = (JObject)parsedJson["webPartData"]["properties"];
            //}
            //else if (parsedJson["properties"] != null)
            //{
            //    this.properties = (JObject)parsedJson["properties"];
            //}
            //else
            //{
            //    this.properties = parsedJson;
            //}

            if (parsedJson.TryGetProperty("webPartData", out JsonElement webPartData))
            {
                if (webPartData.TryGetProperty("properties", out JsonElement properties))
                {
                    this.Properties = properties;
                }

                if (webPartData.TryGetProperty("dataVersion", out JsonElement dataVersion))
                {
                    this.dataVersion = dataVersion.GetString().Trim('"');
                }

                if (webPartData.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                {
                    this.ServerProcessedContent = serverProcessedContent;
                }

                if (webPartData.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
                {
                    this.DynamicDataPaths = dynamicDataPaths;
                }

                if (webPartData.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
                {
                    this.DynamicDataValues = dynamicDataValues;
                }
            }
            else
            {
                if (parsedJson.TryGetProperty("properties", out JsonElement properties))
                {
                    this.Properties = properties;
                }
                else
                {
                    this.Properties = parsedJson;
                }

                if (parsedJson.TryGetProperty("dataVersion", out JsonElement dataVersion))
                {
                    this.dataVersion = dataVersion.GetString().Trim('"');
                }

                if (parsedJson.TryGetProperty("serverProcessedContent", out JsonElement serverProcessedContent))
                {
                    this.ServerProcessedContent = serverProcessedContent;
                }

                if (parsedJson.TryGetProperty("dynamicDataPaths", out JsonElement dynamicDataPaths))
                {
                    this.DynamicDataPaths = dynamicDataPaths;
                }

                if (parsedJson.TryGetProperty("dynamicDataValues", out JsonElement dynamicDataValues))
                {
                    this.DynamicDataValues = dynamicDataValues;
                }
            }

            // Get the web part data version if supplied by the web part json properties
            //if (parsedJson["webPartData"] != null && parsedJson["webPartData"]["dataVersion"] != null)
            //{
            //    this.dataVersion = parsedJson["webPartData"]["dataVersion"].ToString(/*Formatting.None*/).Trim('"');

            //}
            //else if (parsedJson["dataVersion"] != null)
            //{
            //    this.dataVersion = parsedJson["dataVersion"].ToString(/*Formatting.None*/).Trim('"');
            //}

            // If the web part has the serverProcessedContent property then keep this one as it might be needed as input to render the web part HTML later on
            //if (parsedJson["webPartData"] != null && parsedJson["webPartData"]["serverProcessedContent"] != null)
            //{
            //    this.serverProcessedContent = (JObject)parsedJson["webPartData"]["serverProcessedContent"];
            //}
            //else if (parsedJson["serverProcessedContent"] != null)
            //{
            //    this.serverProcessedContent = (JObject)parsedJson["serverProcessedContent"];
            //}

            //if (parsedJson["webPartData"] != null && parsedJson["webPartData"]["dynamicDataPaths"] != null)
            //{
            //    this.dynamicDataPaths = (JObject)parsedJson["webPartData"]["dynamicDataPaths"];
            //}
            //else if (parsedJson["dynamicDataPaths"] != null)
            //{
            //    this.dynamicDataPaths = (JObject)parsedJson["dynamicDataPaths"];
            //}

            //if (parsedJson["webPartData"] != null && parsedJson["webPartData"]["dynamicDataValues"] != null)
            //{
            //    this.dynamicDataValues = (JObject)parsedJson["webPartData"]["dynamicDataValues"];
            //}
            //else if (parsedJson["dynamicDataValues"] != null)
            //{
            //    this.dynamicDataValues = (JObject)parsedJson["dynamicDataValues"];
            //}
        }
        #endregion
    }
}
