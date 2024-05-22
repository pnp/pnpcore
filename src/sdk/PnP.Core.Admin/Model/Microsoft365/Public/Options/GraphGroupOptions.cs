using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Admin.Model.Microsoft365
{
    /// <summary>
    /// Contains the available options for creating a group with Graph Api
    /// </summary>
    public class GraphGroupOptions
    {
        /// <summary>
        /// Name of the Microsoft 365 Group
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Description of the Microsoft 365 Group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Is this group mail enabled
        /// </summary>
        public bool MailEnabled { get; set; }

        /// <summary>
        /// Mail nickname of this Microsoft 365 group
        /// </summary>
        public string MailNickname { get; set; }

        /// <summary>
        /// Classification of this group
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Visibility of this group
        /// </summary>
        public string Visibility { get; set; }

        /// <summary>
        /// Group types
        /// </summary>
        public List<string> GroupTypes { get; internal set; }

        /// <summary>
        /// Owners data, list of UPN's of the users who need to be added as owner
        /// </summary>
        [JsonPropertyName("owners@odata.bind")]
        public string[] Owners { get; set; }

        /// <summary>
        /// Members data, list of UPN's of the users who need to be added as owner
        /// </summary>
        [JsonPropertyName("members@odata.bind")]
        public string[] Members { get; set; }

        /// <summary>
        /// Preferred data location
        /// </summary>
        public string PreferredDataLocation { get; set; }

        /// <summary>
        /// If it is a security enabled group
        /// </summary>
        public bool SecurityEnabled { get; } = false;

        /// <summary>
        /// Allows defining the resource behavior options for the group
        /// See https://learn.microsoft.com/en-us/graph/group-set-options#configure-groups
        /// </summary>
        public List<string> ResourceBehaviorOptions { get; set; }

        /// <summary>
        /// Allows defining creation options for SharePoint Site Creation
        /// </summary>
        public List<string> CreationOptions { get; set; }

        /// <summary>
        /// Option to add custom data to the post request for creating a group like the custom property for a EducationClass
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; } = null;
    }
}
