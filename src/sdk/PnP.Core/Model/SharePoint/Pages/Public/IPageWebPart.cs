using System;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// This class is used to instantiate modern web parts. Using this class you can instantiate a control and 
    /// add it on a <see cref="IPage"/>.
    /// </summary>
    public interface IPageWebPart : ICanvasControl
    {
        /// <summary>
        /// Value of the "data-sp-webpartdata" attribute
        /// </summary>
        public string JsonWebPartData { get; }

        /// <summary>
        /// Value of the "data-sp-htmlproperties" element
        /// </summary>
        public string HtmlPropertiesData { get; }

        /// <summary>
        /// Value of the "data-sp-htmlproperties" attribute
        /// </summary>
        public string HtmlProperties { get; }

        /// <summary>
        /// ID of the client side web part
        /// </summary>
        public string WebPartId { get; }

        /// <summary>
        /// Supports full bleed display experience
        /// </summary>
        public bool SupportsFullBleed { get; }

        /// <summary>
        /// Value of the "data-sp-webpart" attribute
        /// </summary>
        public string WebPartData { get; }

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
        public string WebPartPreviewImage { get; }

        /// <summary>
        /// Json serialized web part information. For 1st party web parts this ideally is the *full* JSON string 
        /// fetch via workbench or via copying it from an existing page. It's important that the serverProcessedContent
        /// element is included here!
        /// </summary>
        public string PropertiesJson { get; set; }

        /// <summary>
        /// Web properties as configurable <see cref="JsonElement"/>
        /// </summary>
        //public JObject Properties
        public JsonElement Properties { get; }

        /// <summary>
        /// ServerProcessedContent json node
        /// </summary>
        public JsonElement ServerProcessedContent { get; }

        /// <summary>
        /// DynamicDataPaths json node
        /// </summary>
        public JsonElement DynamicDataPaths { get; }

        /// <summary>
        /// DynamicDataValues json node
        /// </summary>
        public JsonElement DynamicDataValues { get; }

        /// <summary>
        /// Return <see cref="Type"/> of the client side web part
        /// </summary>
        public new Type Type { get; }

        /// <summary>
        /// This control lives in the page header (not removable control)
        /// </summary>
        public bool IsHeaderControl { get; set; }

        /// <summary>
        /// If this webpart is used inline in a text editor then this property points to the editor using it
        /// </summary>
        public string RichTextEditorInstanceId { get; set; }
    }
}
