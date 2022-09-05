using PnP.Core.Model;
using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Metadata for a site collection catalog response.
    /// </summary>
    [ConcreteType(typeof(AppCatalogSite))]
    public interface IAppCatalogSite
    {
        /// <summary>
        /// The absolute URL for a site collection app catalog.
        /// </summary>
        string AbsoluteUrl { get; set; }

        /// <summary>
        /// The site collection id of a site collection app catalog.
        /// </summary>
        Guid SiteID { get; set; }
    }
}
