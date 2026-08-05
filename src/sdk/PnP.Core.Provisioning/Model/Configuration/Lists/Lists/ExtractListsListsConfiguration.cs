using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Lists.Lists
{
    public class ExtractListsListsConfiguration
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("includeItems")]
        public bool IncludeItems { get; set; }

        [JsonPropertyName("keyColumn")]
        public string KeyColumn { get; set; }

        [JsonPropertyName("updateBehavior")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UpdateBehavior UpdateBehavior { get; set; }

        [JsonPropertyName("skipEmptyFields")]
        public bool SkipEmptyFields { get; set; }

        [JsonPropertyName("query")]
        public ExtractListsQueryConfiguration Query { get; set; } = new ExtractListsQueryConfiguration();

        [JsonPropertyName("removeExistingContentTypes")]
        public bool RemoveExistingContentTypes { get; set; }

    }
}
