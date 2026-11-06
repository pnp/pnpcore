using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant.Teams
{
    /// <summary>
    /// Which Microsoft Teams a tenant template extract describes. With neither
    /// <see cref="IncludeAllTeams"/> nor <see cref="TeamSiteUrls"/> set, the teams behind the site
    /// collections the sequence takes are extracted.
    /// </summary>
    public class ExtractTeamsConfiguration
    {
        /// <summary>
        /// Extract every team in the tenant.
        /// </summary>
        [JsonPropertyName("includeAllTeams")]
        public bool IncludeAllTeams { get; set; }

        /// <summary>
        /// Also extract the messages posted in each channel, up to 50 of them per channel.
        /// </summary>
        [JsonPropertyName("includeMessages")]
        public bool IncludeMessages { get; set; }

        /// <summary>
        /// The urls of the sites whose teams to extract.
        /// </summary>
        [JsonPropertyName("teamSiteUrls")]
        public List<string> TeamSiteUrls { get; set; } = new List<string>();

        /// <summary>
        /// Record the id of each team's group in the template. Applying such a template configures the
        /// team of that existing group, making the group a team when it is not one, rather than creating
        /// a new team.
        /// </summary>
        [JsonPropertyName("includeGroupId")]
        public bool IncludeGroupId { get; set; }
    }
}
