using System;

namespace PnP.Core.Model.SharePoint
{
    internal class Thumbnail : IThumbnail
    {
        public string SetId { get; set; }

        public string Size { get; set; }

        public Uri Url { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
