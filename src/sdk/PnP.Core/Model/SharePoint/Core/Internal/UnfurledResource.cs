using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal class UnfurledResource : IUnfurledResource
    {
        public UnfurlLinkType LinkType { get; set; }

        public Uri Resource { get; set; }

        public Guid SiteId { get; set; }

        public Guid WebId { get; set; }

        public Uri WebUrl { get; set; }

        public Guid ListId { get; set; }

        public string ListDisplayName { get; set; }

        public Uri ListUrl { get; set; }

        public int ListItemId { get; set; }

        public string FileDriveId { get; set; }

        public string FileDriveItemId { get; set; }

        public Guid FileUniqueId { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime LastModified { get; set; }

        public string LastModifiedBy { get; set; }

        public List<IThumbnail> Thumbnails { get; set; }
    }
}
