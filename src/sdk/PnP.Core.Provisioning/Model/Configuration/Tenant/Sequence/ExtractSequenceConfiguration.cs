using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant.Sequence
{
    /// <summary>
    /// Which site collections a tenant template extract describes, as one sequence.
    /// </summary>
    public class ExtractSequenceConfiguration
    {
        /// <summary>
        /// The urls of the site collections to extract. A server relative url is resolved against the
        /// tenant of the context the extract runs with.
        /// </summary>
        [JsonPropertyName("siteUrls")]
        public List<string> SiteUrls { get; set; } = new List<string>();

        /// <summary>
        /// How many levels of subsites to extract when <see cref="IncludeSubsites"/> is set: 1 takes only
        /// the subsites of a site collection's root web, and 0, the default, takes every level.
        /// </summary>
        [JsonPropertyName("maxSubsiteDepth")]
        public int MaxSubsiteDepth { get; set; }

        /// <summary>
        /// Also extract the site collections joined to any hub site named in <see cref="SiteUrls"/>.
        /// Listing them needs read access to the SharePoint admin center.
        /// </summary>
        [JsonPropertyName("includeJoinedSites")]
        public bool IncludeJoinedSites { get; set; }

        /// <summary>
        /// Also extract the subsites of each site collection, each with a template of its own.
        /// </summary>
        [JsonPropertyName("includeSubsites")]
        public bool IncludeSubsites { get; set; }
    }
}
