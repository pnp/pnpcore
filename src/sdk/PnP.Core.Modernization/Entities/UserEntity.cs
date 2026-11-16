using Newtonsoft.Json;
using System;

namespace PnP.Core.Modernization.Entities
{
    /// <summary>
    /// Class to hold information about a given user
    /// </summary>
    [Serializable]
    public class UserEntity
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Upn of the user
        /// </summary>
        [JsonProperty(PropertyName = "upn")]
        public string Upn { get; set; }

        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Role of the user
        /// </summary>
        [JsonProperty(PropertyName = "role")]
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
