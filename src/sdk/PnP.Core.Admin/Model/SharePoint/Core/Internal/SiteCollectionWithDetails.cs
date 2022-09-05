using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class SiteCollectionWithDetails : SiteCollection, ISiteCollectionWithDetails
    {
        public DateTime TimeCreated { get; set; }

        public string CreatedBy { get; set; }

        public bool ShareByEmailEnabled { get; set; }

        public bool ShareByLinkEnabled { get; set; }

        public string SiteOwnerName { get; set; }

        public string SiteOwnerEmail { get; set; }

        public long StorageQuota { get; set; }

        public long StorageUsed { get; set; }

        public int TemplateId { get; set; }

        public string TemplateName { get; set; }
    }
}
