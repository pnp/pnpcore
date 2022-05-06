using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options for the team tag
    /// </summary>
    public class TeamTagOptions
    {

        /// <summary>
        /// Display name of the tag
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Members associated with the tags
        /// </summary>
        public List<TeamTagUserOptions> Members { get; set; }

    }
}
