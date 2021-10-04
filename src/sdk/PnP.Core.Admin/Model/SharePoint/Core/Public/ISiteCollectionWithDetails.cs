using PnP.Core.Model;
using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// A SharePoint site collection with details reference
    /// </summary>
    [ConcreteType(typeof(SiteCollectionWithDetails))]
    public interface ISiteCollectionWithDetails: ISiteCollection
    {
        /// <summary>
        /// Date time when this site collection was created
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// The user that created this site collection
        /// </summary>
        public string CreatedBy { get; }

        /// <summary>
        /// Indicates whether this site collection can be externally shared via an email
        /// </summary>
        public bool ShareByEmailEnabled { get; }

        /// <summary>
        /// Indicates whether this site collection can be externally shared via a link
        /// </summary>
        public bool ShareByLinkEnabled { get; }

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
        /// The id of the applied template (if any)
        /// </summary>
        public int TemplateId { get; }

        /// <summary>
        /// The name of the applied template
        /// </summary>
        public string TemplateName { get; }
    }
}
