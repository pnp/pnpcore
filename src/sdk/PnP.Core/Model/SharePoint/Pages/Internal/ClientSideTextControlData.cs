using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Control data for controls of type 4 (= text control)
    /// </summary>
    internal class ClientSideTextControlData : ClientSideCanvasControlData
    {
        /// <summary>
        /// Gets or sets JsonProperty "editorType"
        /// </summary>
        [JsonPropertyName("editorType")]
        public string EditorType { get; set; }
    }
}
