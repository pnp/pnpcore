using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Lists
{
    public class ApplyListsConfiguration
    {
        [JsonPropertyName("ignoreDuplicateDataRowErrors")]
        public bool IgnoreDuplicateDataRowErrors { get; set; }
    }
}
