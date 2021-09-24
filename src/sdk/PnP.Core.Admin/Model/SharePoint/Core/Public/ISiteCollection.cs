using PnP.Core.Model;
using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// A SharePoint site collection reference
    /// </summary>
    [ConcreteType(typeof(SiteCollection))]
    public interface ISiteCollection
    {
        /// <summary>
        /// The id of the site collection
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Graph id of the site collection
        /// </summary>
        public string GraphId { get; }

        /// <summary>
        /// The URL of the site collection
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// Id of the root web of the site collection
        /// </summary>
        public Guid RootWebId { get; }

        /// <summary>
        /// Description of the root web of the site collection
        /// </summary>
        public string RootWebDescription { get; }


    }
}
