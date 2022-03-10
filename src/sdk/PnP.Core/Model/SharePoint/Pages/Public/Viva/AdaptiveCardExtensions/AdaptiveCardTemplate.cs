using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva
{
    public class AdaptiveCardTemplate
    {
        public string Type { get; set; } = "primaryText";
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
        public List<AdaptiveCardAction> Actions { get; set; }
    }
}
