using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Lists.Lists
{

    public class ExtractListsQueryConfiguration
    {
        [JsonPropertyName("camlQuery")]
        public string CamlQuery { get; set; }

        [JsonPropertyName("rowLimit")]
        public int RowLimit { get; set; }

        [JsonPropertyName("viewFields")]
        public List<string> ViewFields { get; set; } = new List<string>();

        [JsonPropertyName("includeAttachments")]
        public bool IncludeAttachments { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
    }
}
