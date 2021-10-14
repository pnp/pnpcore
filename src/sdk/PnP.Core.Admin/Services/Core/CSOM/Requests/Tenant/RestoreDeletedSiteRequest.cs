using System;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class RestoreDeletedSiteRequest : BaseSiteOperationRequest
    {
        internal RestoreDeletedSiteRequest(Uri siteUrl) : base("RestoreDeletedSite", siteUrl)
        {
        }
    }
}
