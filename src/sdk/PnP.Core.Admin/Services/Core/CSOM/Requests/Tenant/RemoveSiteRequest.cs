using System;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class RemoveSiteRequest : BaseSiteOperationRequest
    {
        internal RemoveSiteRequest(Uri siteUrl) : base("RemoveSite", siteUrl)
        {
        }
    }
}
