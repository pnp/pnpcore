using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines a site collection living in the tenant's recyle bin
    /// </summary>
    public interface IRecycledSiteCollection
    {
        /// <summary>
        /// The id of the site collection
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The URL of the site collection
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Name of the site collection
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The id of the connected Microsoft 365 group
        /// </summary>
        public Guid GroupId { get; }

        /// <summary>
        /// Date time when this site collection was created
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Date time when this site collection was deleted
        /// </summary>
        public DateTime TimeDeleted { get; }

        /// <summary>
        /// The user that created this site collection
        /// </summary>
        public string CreatedBy { get; }

        /// <summary>
        /// The user that deleted this site collection
        /// </summary>
        public string DeletedBy { get; }

        /// <summary>
        /// Returns the name of the site collection owner
        /// </summary>
        public string SiteOwnerName { get; }

        /// <summary>
        /// Returns the email address of teh site collection owner
        /// </summary>
        public string SiteOwnerEmail { get; }

        /// <summary>
        /// The storage quota in bytes for this site collection
        /// </summary>
        public long StorageQuota { get; }

        /// <summary>
        /// The storage quota used for this site collection
        /// </summary>
        public long StorageUsed { get; }

        /// <summary>
        /// The name of the applied template
        /// </summary>
        public string TemplateName { get; }
    }
}
