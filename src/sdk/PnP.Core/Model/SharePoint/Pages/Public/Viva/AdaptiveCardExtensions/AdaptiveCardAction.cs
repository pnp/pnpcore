using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva
{

    public class ButtonAction
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// Supported values: positive or defailt
        /// </summary>
        [JsonPropertyName("style")]
        public string Style { get; set; }
        /// <summary>
        /// Action to execute
        /// </summary>
        [JsonPropertyName("action")]
        public AdaptiveCardAction Action { get; set; }
    }
    public class AdaptiveCardAction
    {
        /// <summary>
        /// Supported values Action.OpenUrl, Action.Submit, Action.ShowCard, Action.Execute, ExternalLink,QuickView
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        /// <summary>
        /// Action parameters
        /// </summary>
        [JsonPropertyName("parameters")]
        public object Parameters { get; set; }
    }

    public class AdaptiveCardAction<T> : AdaptiveCardAction
    {
        new public T Parameters
        {
            get
            {
                return (T)base.Parameters;
            }
            set
            {
                base.Parameters = value;
            }
        }
    }

    public class ExternalLinkAction : AdaptiveCardAction<ExternalLinkActionParameter>
    {
        public ExternalLinkAction()
        {
            Type = "ExternalLink";
        }
    }

    public class ExternalLinkActionParameter
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }
    }
    public class QuickViewAction : AdaptiveCardAction<QuickViewActionParameter>
    {
        public QuickViewAction()
        {
            Type = "QuickView";
        }
    }

    public class QuickViewActionParameter
    {
        [JsonPropertyName("view")]
        public string View { get; set; }
    }
}
