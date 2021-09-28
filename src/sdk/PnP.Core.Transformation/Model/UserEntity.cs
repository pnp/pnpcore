using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Class to hold information about a given user
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Upn of the user
        /// </summary>
        [JsonPropertyName("upn")]
        public string Upn { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Role of the user
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; }

        /// <summary>
        /// Loginname of the user
        /// </summary>
        [JsonIgnore]
        public string LoginName { get; set; }

        /// <summary>
        /// Is this a group?
        /// </summary>
        [JsonIgnore]
        public bool IsGroup { get; set; }
    }
}
