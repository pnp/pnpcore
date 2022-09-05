using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class MenuNode
    {
        public MenuNode()
        {
            Nodes = new List<MenuNode>();
        }

        public string FriendlyUrlSegment { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public string Key { get; set; }

        public List<MenuNode> Nodes { get; set; }

        public int NodeType { get; set; }

        public string SimpleUrl { get; set; }

        public string Title { get; set; }

    }
}
