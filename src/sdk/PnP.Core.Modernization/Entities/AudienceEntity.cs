using System.Collections.Generic;

namespace PnP.Core.Modernization.Entities
{
    /// <summary>
    /// Holds information about the defined audiences (used by modernization scanner)
    /// </summary>
    public class AudienceEntity
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public AudienceEntity()
        {
            this.GlobalAudiences = new List<string>();
            this.SecurityGroups = new List<string>();
            this.SharePointGroups = new List<string>();
        }

        /// <summary>
        /// List of defined global audiences
        /// </summary>
        public List<string> GlobalAudiences { get; set; }
        /// <summary>
        /// List of security group based audiences
        /// </summary>
        public List<string> SecurityGroups { get; set; }
        /// <summary>
        /// List of SharePoint group based audiences
        /// </summary>
        public List<string> SharePointGroups { get; set; }
    }
}
