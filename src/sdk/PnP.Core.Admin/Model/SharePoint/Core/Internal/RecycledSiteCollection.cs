using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class RecycledSiteCollection : IRecycledSiteCollection
    {
        public Guid Id { get; set; }

        public Uri Url { get; set; }

        public string Name { get; set; }

        public Guid GroupId { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime TimeDeleted { get; set; }

        public string CreatedBy { get; set; }

        public string DeletedBy { get; set; }

        public string SiteOwnerName { get; set; }

        public string SiteOwnerEmail { get; set; }

        public long StorageQuota { get; set; }

        public long StorageUsed { get; set; }

        public string TemplateName { get; set; }
    }
}
