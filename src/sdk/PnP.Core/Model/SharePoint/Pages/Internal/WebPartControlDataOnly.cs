using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Control data for controls of type 3 (= client side web parts) which persist using the data-sp-controldata property only
    /// </summary>
    internal sealed class WebPartControlDataOnly : WebPartControlData
    {
        [JsonPropertyName("webPartData")]
        public string WebPartData { get; set; }
    }
}
