using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// When you're using a vanity URL tenant you can specify your custom URLs here
    /// </summary>
    public class VanityUrlOptions
    {
        /// <summary>
        /// Sets the vanity portal URL (e.g. https://portal.contoso.com)
        /// </summary>
        public Uri PortalUri { get; set; }

        /// <summary>
        /// Sets the vanity my site host URL (e.g. https://my.contoso.com)
        /// </summary>
        public Uri MySiteHostUri { get; set; }

        /// <summary>
        /// Sets the admin center URL (e.g. https://sharepointadmin.contoso.com)
        /// </summary>
        public Uri AdminCenterUri { get; set; }
    }
}
