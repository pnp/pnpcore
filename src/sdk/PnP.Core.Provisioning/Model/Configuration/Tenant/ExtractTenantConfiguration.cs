using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant
{
    public class ExtractTenantConfiguration
    {
        /// <summary>
        /// If defined will extract site collections as defined in the SiteUrls array
        /// </summary>
        [JsonPropertyName("sequence")]
        public Sequence.ExtractSequenceConfiguration Sequence { get; set; }

        /// <summary>
        /// If defined will extract teams as defined
        /// </summary>
        [JsonPropertyName("teams")]
        public Teams.ExtractTeamsConfiguration Teams { get; set; }
    }
}
