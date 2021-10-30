using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class AppCatalogSite : IAppCatalogSite
    {
        public string AbsoluteUrl { get; set; }
        public Guid SiteID { get; set; }
    }
}
