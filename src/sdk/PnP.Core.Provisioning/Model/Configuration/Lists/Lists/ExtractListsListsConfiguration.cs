using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Lists.Lists
{
    public class ExtractListsListsConfiguration
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("includeItems")]
        public bool IncludeItems { get; set; }

        [JsonPropertyName("includeFiles")]
        public bool IncludeFiles { get; set; }

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

        /// <summary>
        /// Whether the list's folders are extracted into the list instance, each with its property bag.
        /// </summary>
        [JsonPropertyName("includeFolders")]
        public bool IncludeFolders { get; set; }

        /// <summary>
        /// How many levels of folders <see cref="IncludeFolders"/> extracts: 1 takes only the folders at the root of
        /// the list. 0, the default, takes every level.
        /// </summary>
        [JsonPropertyName("maxFolderDepth")]
        public int MaxFolderDepth { get; set; }

        /// <summary>
        /// Whether the unique permissions of the list's folders and items are extracted with them. This reads the
        /// permissions of every item, so it is off by default.
        /// </summary>
        [JsonPropertyName("includeSecurity")]
        public bool IncludeSecurity { get; set; }

        /// <summary>
        /// Whether the urls and ids of the site and site collection are replaced with tokens in the extracted item
        /// values, so that links in the data rows point at the site the template is applied to rather than back at
        /// the one it came from.
        /// </summary>
        [JsonPropertyName("tokenizeUrls")]
        public bool TokenizeUrls { get; set; }
    }
}
