using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Admin.Model.Microsoft365.Public.Options
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
        /// Url of the SharePoint site connected to this Microsoft 365 group
        /// </summary>
        public Uri WebUrl { get; }

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
        public List<string> GroupTypes { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; }

        [JsonPropertyName("owners@odata.bind")]
        /// <summary>
        /// Owners data
        /// </summary>
        public string[] Owners { get; set; }

        [JsonPropertyName("members@odata.bind")]
        /// <summary>
        /// Members data
        /// </summary>
        public string[] Members { get; set; }


        /// <summary>
        /// Preferred data location
        /// </summary>
        public string PreferredDataLocation { get; set; }

        /// <summary>
        /// If it is a security enabled group
        /// </summary>
        public bool SecurityEnabled { get; set; }
    }
}
