using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class LegacySiteCollectionPermission : ILegacySiteCollectionPermission
    {
        public Guid SiteId { get; set;  }

        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public LegacySiteCollectionPermissionRight Right { get; set; }
    }
}
