using System;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal sealed class RemoveDeletedSiteRequest : BaseSiteOperationRequest
    {
        internal RemoveDeletedSiteRequest(Uri siteUrl) : base("RemoveDeletedSite", siteUrl)
        {
        }
    }
}
