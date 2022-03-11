using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Abstract class representing ACE
    /// </summary>
    public class AdaptiveCardExtension
    {
        private string id;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="cardSize"></param>
        public AdaptiveCardExtension(CardSize cardSize = CardSize.Medium)
        {
            CardSize = cardSize;
        }

        /// <summary>
        /// Gets or sets JsonProperty "id"
        /// </summary>
        [JsonPropertyName("id")]
        public string Id
        {
            get { return id; }
            set 
            { 
                id = value;
                if (!string.IsNullOrEmpty(id))
                {
                    ACEType = VivaDashboard.IdToDefaultACE(id);
                }
            }
        }

        /// <summary>
        /// Gets or sets JsonProperty "instanceId"
        /// </summary>
        [JsonPropertyName("instanceId")]
        public Guid InstanceId { get; internal set; } = Guid.NewGuid();

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
        /// Typed properties of the ACE
        /// </summary>
        public virtual object Properties { get; set; }

        /// <summary>
        /// Json properties of the ACE once added to a dashboard
        /// </summary>
        [JsonIgnore]        
        public JsonElement JsonProperties { get; internal set; }

        /// <summary>
        /// Icon used by the ACE
        /// </summary>
        [JsonPropertyName("iconProperty")]
        public string IconProperty { get; set; }

        /// <summary>
        /// Size of the ACE
        /// </summary>
        [JsonPropertyName("cardSize")]
        public CardSize CardSize { get; set; }

        /// <summary>
        /// Order of this ACE inside the dashboard
        /// </summary>
        [JsonIgnore]
        public int Order { get; internal set; }

        /// <summary>
        /// Type of this ACE
        /// </summary>
        [JsonIgnore]
        public DefaultACE ACEType { get; internal set; }
    }

    /// <summary>
    /// Defines an ACE
    /// </summary>
    /// <typeparam name="T">Type of the ACE</typeparam>
    public class AdaptiveCardExtension<T> : AdaptiveCardExtension
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="cardSize"></param>
        public AdaptiveCardExtension(CardSize cardSize = CardSize.Medium) : base(cardSize)
        {
        }

        /// <summary>
        /// Properties of the ACE
        /// </summary>
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

    /// <summary>
    /// Representents a generic, non typed, ACE
    /// </summary>
    public class GenericAdaptiveCardExtension : AdaptiveCardExtension<JsonElement>
    {

    }
}
