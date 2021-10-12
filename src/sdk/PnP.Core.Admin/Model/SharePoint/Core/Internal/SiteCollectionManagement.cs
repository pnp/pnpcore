using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal static class SiteCollectionManagement
    {

        internal static async Task RecycleSiteCollectionAsync(PnPContext context, Uri siteToRecycle, string webTemplate)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                // When recycling a group connected site then use the groupsitemanager endpoint. This will also recycle the connected Microsoft 365 group
                if (HasGroup(webTemplate))
                {
                    await (context.Web as Web).RawRequestAsync(new ApiCall($"_api/GroupSiteManager/Delete?siteUrl='{siteToRecycle.AbsoluteUri}'", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
                }
                else
                {
                    List<IRequest<object>> csomRequests = new List<IRequest<object>>
                    {
                        new RemoveSiteRequest(siteToRecycle)
                    };

                    var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                    await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
                }
            }
        }

        internal static async Task DeleteSiteCollectionAsync(PnPContext context, Uri siteToDelete, string webTemplate)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                // first recycle the site collection
                await RecycleSiteCollectionAsync(tenantAdminContext, siteToDelete, webTemplate).ConfigureAwait(false);

                // Only supporting permanent delete of non group connected sites. For group connected sites permanently deleting the group will
                // trigger the deletion of the linked resources (like the SharePoint site).  
                if (!HasGroup(webTemplate))
                {
                    // once that's done remove the site collection from the recycle bin
                    List<IRequest<object>> csomRequests = new List<IRequest<object>>
                    {
                        new RemoveDeletedSiteRequest(siteToDelete)
                    };

                    var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                    await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
                }
            }
        }

        internal static async Task RestoreSiteCollectionAsync(PnPContext context, Uri siteToRecycle)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new RestoreDeletedSiteRequest(siteToRecycle)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
            }
        }

        internal static async Task WaitForSpoOperationCompleteAsync(PnPContext tenantAdminContext, SpoOperation operation, int maxStatusChecks = 10)
        {
            if (operation.IsComplete)
            {
                return;
            }

            List<IRequest<object>> csomRequests = new List<IRequest<object>>
            {
                new SpoOperationRequest(operation.ObjectIdentity)
            };

            bool operationComplete = false;
            var statusCheckAttempt = 1;

            do
            {
                await tenantAdminContext.WaitAsync(TimeSpan.FromMilliseconds(operation.PollingInterval)).ConfigureAwait(false);

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                operation = result.ApiCall.CSOMRequests[0].Result as SpoOperation;

                if (operation.IsComplete)
                {
                    operationComplete = true;
                }

                statusCheckAttempt++;
            }
            while (!operationComplete && statusCheckAttempt <= maxStatusChecks);

        }

        private static bool HasGroup(string webTemplate)
        {
            return webTemplate.Equals(PnPAdminConstants.TeamSiteTemplate, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
