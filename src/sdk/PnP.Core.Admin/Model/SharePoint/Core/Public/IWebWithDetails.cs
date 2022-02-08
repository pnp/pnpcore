using PnP.Core.Model;
using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// A SharePoint web with details reference
    /// </summary>
    [ConcreteType(typeof(WebWithDetails))]
    public interface IWebWithDetails
    {
        /// <summary>
        /// The id of the site collection
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Title of the web
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of the web
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Server relative url of the web
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Language of the web
        /// </summary>
        public Language Language { get; }

        /// <summary>
        /// Url of the web
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Gets the name of the site definition or site template that was used to create the site.
        /// </summary>
        public string WebTemplate { get; }

        /// <summary>
        /// Gets the web template configuration of the site.
        /// </summary>
        public string WebTemplateConfiguration { get; }

        /// <summary>
        /// Date time when this site collection was created
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Date time when there was the last item modified
        /// </summary>
        public DateTime LastItemModifiedDate { get; }

        /// <summary>
        /// Date time when there was the list item modified by a user
        /// </summary>
        public DateTime LastItemUserModifiedDate { get; }

    }
}
