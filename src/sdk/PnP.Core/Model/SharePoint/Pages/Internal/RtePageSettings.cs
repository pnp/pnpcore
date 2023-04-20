using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class RtePageSettings
    {
        [JsonPropertyName("contentVersion")]
        public int? contentVersion { get; set; }
    }
}
