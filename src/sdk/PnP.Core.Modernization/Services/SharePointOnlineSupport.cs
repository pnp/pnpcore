
using System;

namespace PnP.Core.Modernization.Services
{
    internal static class SharePointOnlineSupport
    {
        internal static void Validate(PnPContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Validate(context.Uri);
        }

        internal static void Validate(Uri site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }
            if (!site.IsAbsoluteUri || site.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(site.UserInfo) ||
                !(site.Host.EndsWith(".sharepoint.com", StringComparison.OrdinalIgnoreCase) ||
                  site.Host.EndsWith(".sharepoint.us", StringComparison.OrdinalIgnoreCase) ||
                  site.Host.EndsWith(".sharepoint-mil.us", StringComparison.OrdinalIgnoreCase) ||
                  site.Host.EndsWith(".sharepoint.cn", StringComparison.OrdinalIgnoreCase) ||
                  site.Host.EndsWith(".sharepoint.de", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException("PnP Core modernization supports SharePoint Online only. On-premises SharePoint sources and destinations are not supported.");
            }
        }
    }
}
