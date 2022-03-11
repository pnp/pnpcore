using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ACE template
    /// </summary>
    public class AdaptiveCardTemplate
    {
        /// <summary>
        /// ACE template type
        /// </summary>
        public string Type { get; set; } = "primaryText";

        /// <summary>
        /// ACE body
        /// </summary>
        public List<AdaptiveCardControl> Body { get; set; } = new List<AdaptiveCardControl>()
        {
            new AdaptiveCardControl()
            {
                Type = "TextBlock",
                Size = "Medium",
                Weight = "Bolder",
                Text = "${Text}",
                Wrap = true
            }
        };

        /// <summary>
        /// ACE actions
        /// </summary>
        public List<AdaptiveCardAction> Actions { get; set; }
    }
}
