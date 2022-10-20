using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ACE card button action
    /// </summary>
    public class ButtonAction
    {
        /// <summary>
        /// Button title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Supported values: positive or default
        /// </summary>
        [JsonPropertyName("style")]
        public string Style { get; set; }

        /// <summary>
        /// Is the button visible
        /// </summary>
        [JsonPropertyName("isVisible")]
        public bool? IsVisible { get; set; }

        /// <summary>
        /// Action to execute
        /// </summary>
        [JsonPropertyName("action")]
        public AdaptiveCardAction Action { get; set; }
    }

    /// <summary>
    /// ACE card action
    /// </summary>
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

    /// <summary>
    /// ACE card action
    /// </summary>
    /// <typeparam name="T">Type ACE card action</typeparam>
    public class AdaptiveCardAction<T> : AdaptiveCardAction
    {
        /// <summary>
        /// ACE card action parameters
        /// </summary>
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

    /// <summary>
    /// External link ACE action
    /// </summary>
    public class ExternalLinkAction : AdaptiveCardAction<ExternalLinkActionParameter>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExternalLinkAction()
        {
            Type = "ExternalLink";
        }
    }

    /// <summary>
    /// External link parameter
    /// </summary>
    public class ExternalLinkActionParameter
    {
        /// <summary>
        /// External link target
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }
    }

    /// <summary>
    /// Quick view ACE action
    /// </summary>
    public class QuickViewAction : AdaptiveCardAction<QuickViewActionParameter>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public QuickViewAction()
        {
            Type = "QuickView";
        }
    }

    /// <summary>
    /// Quick view ACE action parameters
    /// </summary>
    public class QuickViewActionParameter
    {
        /// <summary>
        /// Selected view
        /// </summary>
        [JsonPropertyName("view")]
        public string View { get; set; }
    }
}
