using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the card designer ACE
    /// </summary>
    public class CardDesignerACE : AdaptiveCardExtension<CardDesignerProps>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CardDesignerACE(CardSize cardSize = CardSize.Medium) : base(cardSize)
        {
            Id = VivaDashboard.DefaultACEToId(DefaultACE.CardDesigner);
            InstanceId = Guid.NewGuid();
        }
    }

    /// <summary>
    /// Card designer ACE properties
    /// </summary>
    public class CardDesignerProps : ACEProperties
    {
        /// <summary>
        /// Represents type of the Card primaryText, heading or image
        /// </summary>
        [JsonPropertyName("templateType")]
        public string TemplateType { get; set; } = "primaryText";

        /// <summary>
        /// Icon source type of the card
        /// </summary>
        [JsonPropertyName("cardIconSourceType")]
        public int CardIconSourceType { get; set; } = 1;
        
        /// <summary>
        /// Image source type of the card
        /// </summary>
        [JsonPropertyName("cardImageSourceType")]
        public int CardImageSourceType { get; set; } = 1;
        
        /// <summary>
        /// Describes action on card clicked
        /// </summary>
        [JsonPropertyName("cardSelectionAction")]
        public AdaptiveCardAction CardSelectionAction { get; set; }
        
        /// <summary>
        /// The number of buttons that will be rendered by ACE
        /// </summary>
        [JsonPropertyName("numberCardButtonActions")]
        public int NumberCardButtonActions { get; set; } = 1;
        
        /// <summary>
        /// Describes actions on card buttons clicked
        /// </summary>
        [JsonPropertyName("cardButtonActions")]
        public List<ButtonAction> CardButtonActions { get; set; }
        
        /// <summary>
        /// Lists of supported QuickViews
        /// </summary>
        [JsonPropertyName("quickViews")]
        public List<QuickView> QuickViews { get; set; }
        
        /// <summary>
        /// Returns true if ACE should use QuickView
        /// </summary>
        [JsonPropertyName("isQuickViewConfigured")]
        public bool QuickViewConfigured { get; set; }
        
        /// <summary>
        /// Current card quick view index
        /// </summary>
        [JsonPropertyName("currentQuickViewIndex")]
        public int CurrentQuickViewIndex { get; set; }
        
        /// <summary>
        /// Card data type
        /// </summary>
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }
        
        /// <summary>
        /// SharePoint request url for the card
        /// </summary>
        [JsonPropertyName("spRequestUrl")]
        public string SPRequestUrl { get; set; }
        
        /// <summary>
        /// Request url for the card
        /// </summary>
        [JsonPropertyName("requestUrl")]
        public string RequestUrl { get; set; }
        
        /// <summary>
        /// Microsoft Graph request url for the card
        /// </summary>
        [JsonPropertyName("graphRequestUrl")]
        public string GraphRequestUrl { get; set; }
        
        /// <summary>
        /// Card primary text
        /// </summary>
        [JsonPropertyName("primaryText")]
        public string PrimaryText { get; set; }
        
        /// <summary>
        /// Card icon image settings
        /// </summary>
        [JsonPropertyName("cardIconCustomImageSettings")]
        public CustomImageSettings CustomImageSettings { get; set; }
    }

    /// <summary>
    /// Card icon image settings
    /// </summary>
    public class CustomImageSettings
    {
        /// <summary>
        /// Image type
        /// </summary>
        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;
        
        /// <summary>
        /// Image alternative text
        /// </summary>
        [JsonPropertyName("altText")]
        public string AltText { get; set; }
        
        /// <summary>
        /// Image url
        /// </summary>
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
    }
}
