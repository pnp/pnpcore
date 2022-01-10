using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class MenuState
    {
        public MenuState()
        {
            Nodes = new List<MenuNode>();
        }

        public string FriendlyUrlPrefix { get; set; }

        public List<MenuNode> Nodes { get; set; }

        public string SimpleUrl { get; set; }

        public string SPSitePrefix { get; set; }

        public string SPWebPrefix { get; set; }

        public string StartingNodeKey { get; set; }

        public string StartingNodeTitle { get; set; }

        public string Version { get; set; }

    }

    internal sealed class MenuStateWrapper
    {
        [JsonPropertyName("menuState")]
        public MenuState MenuState { get; set; }
    }
}
