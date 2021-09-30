using System.Text.Json.Serialization;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SpoOperation
    {
        [JsonPropertyName("_ObjectIdentity_")]
        public string ObjectIdentity { get; set; }

        public int PollingInterval { get; set; }

        public bool IsComplete { get; set; }
    }
}
