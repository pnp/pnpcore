using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant.Teams
{
    public class ExtractTeamsConfiguration
    {
        [JsonPropertyName("includeAllTeams")]
        public bool IncludeAllTeams { get; set; }

        [JsonPropertyName("includeMessages")]
        public bool IncludeMessages { get; set; }

        [JsonPropertyName("teamSiteUrls")]
        public List<string> TeamSiteUrls { get; set; } = new List<string>();

        [JsonPropertyName("includeGroupId")]
        public bool IncludeGroupId { get; set; }
    }
}
