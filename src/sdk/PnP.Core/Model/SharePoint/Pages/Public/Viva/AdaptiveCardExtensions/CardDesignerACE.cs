using PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva
{
    public class CardDesignerACE : AdaptiveCardExtension<CardDesignerProps>
    {
        public CardDesignerACE()
        {
            Id = "9593e615-7320-4b8b-be98-09b97112b12f";
            InstanceId = Guid.NewGuid();
        }
    }
    public class CardDesignerProps : ACEProperties
    {
        /// <summary>
        /// Represents type of the Card primaryText, heading or image
        /// </summary>
        [JsonPropertyName("templateType")]
        public string TemplateType { get; set; } = "primaryText";
        [JsonPropertyName("cardIconSourceType")]
        public int CardIconSourceType { get; set; } = 1;
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
        /// 
        /// </summary>
        [JsonPropertyName("currentQuickViewIndex")]
        public int CurrentQuickViewIndex { get; set; }
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }
        [JsonPropertyName("spRequestUrl")]
        public string SPRequestUrl { get; set; }
        [JsonPropertyName("requestUrl")]
        public string RequestUrl { get; set; }
        [JsonPropertyName("graphRequestUrl")]
        public string GraphRequestUrl { get; set; }
        [JsonPropertyName("primaryText")]
        public string PrimaryText { get; set; }
        [JsonPropertyName("cardIconCustomImageSettings")]
        public CustomImageSettings CustomImageSettings { get; set; }
    }
    public class CustomImageSettings
    {
        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;
        [JsonPropertyName("altText")]
        public string AltText { get; set; }
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }
    }
}
