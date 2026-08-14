using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Lists
{
    public class ExtractListsConfiguration
    {
        [JsonPropertyName("includeHiddenLists")]
        public bool IncludeHiddenLists { get; set; }

        [JsonPropertyName("lists")]
        public List<Lists.ExtractListsListsConfiguration> Lists { get; set; } = new List<Lists.ExtractListsListsConfiguration>();

        public bool HasLists
        {
            get
            {
                return this.Lists != null && this.Lists.Count > 0;
            }
        }
    }
}
