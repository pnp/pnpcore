using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Class representing the json control data that will describe a control versus the zones and sections on a page
    /// </summary>
    internal class ClientSideCanvasControlPosition : ClientSideCanvasPosition
    {
        /// <summary>
        /// Gets or sets JsonProperty "controlIndex"
        /// </summary>
        [JsonPropertyName("controlIndex")]
        public float ControlIndex { get; set; }
    }
}
