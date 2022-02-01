using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva
{
    /// <summary>
    /// Abstract class representing ACE
    /// </summary>
    public class AdaptiveCardExtension
    {
        /// <summary>
        /// Gets or sets JsonProperty "id"
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "instanceId"
        /// </summary>
        [JsonPropertyName("instanceId")]
        public Guid InstanceId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets JsonProperty "title"
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "description"
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// Web properties as configurable
        /// </summary>
        public virtual object Properties { get; set; }

    }
    public class AdaptiveCardExtension<T> : AdaptiveCardExtension
    {
        new public T Properties
        {
            get
            {
                return (T)base.Properties;
            }
            set
            {
                base.Properties = value;
            }
        }
    }
    public class GenericAdaptiveCardExtension : AdaptiveCardExtension<JsonElement>
    {

    }
}
