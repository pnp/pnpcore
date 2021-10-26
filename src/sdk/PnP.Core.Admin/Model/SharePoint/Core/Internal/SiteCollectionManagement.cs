using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model;
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

        internal static async Task<bool> RecycleSiteCollectionAsync(PnPContext context, Uri siteToRecycle)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                // When recycling a group connected site then use the groupsitemanager endpoint. This will also recycle the connected Microsoft 365 group
                if (await HasGroupAsync(context, siteToRecycle).ConfigureAwait(false))
                {
                    await (context.Web as Web).RawRequestAsync(new ApiCall($"_api/GroupSiteManager/Delete?siteUrl='{siteToRecycle.AbsoluteUri}'", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    List<IRequest<object>> csomRequests = new List<IRequest<object>>
                    {
                        new RemoveSiteRequest(siteToRecycle)
                    };

                    var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);

                    await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
                    return false;
                }
            }
        }

        internal static async Task DeleteSiteCollectionAsync(PnPContext context, Uri siteToDelete)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                // first recycle the site collection
                bool siteHasGroup = await RecycleSiteCollectionAsync(context, siteToDelete).ConfigureAwait(false);

                // Only supporting permanent delete of non group connected sites. For group connected sites permanently deleting the group will
                // trigger the deletion of the linked resources (like the SharePoint site).  
                if (!siteHasGroup)
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

        private static async Task<bool> HasGroupAsync(PnPContext context, Uri siteToCheck)
        {
            if (context.Uri == siteToCheck)
            {
                return context.Site.GroupId != Guid.Empty;
            }
            else
            {
                using (var siteContext = await context.CloneAsync(siteToCheck).ConfigureAwait(false))
                {
                    return siteContext.Site.GroupId != Guid.Empty;
                }
            }
        }

        internal static async Task<ISiteCollectionProperties> GetSiteCollectionPropertiesByUrlAsync(PnPContext context, Uri siteUrl, bool detailed)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new GetSitePropertiesRequest(siteUrl, detailed)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                (result.ApiCall.CSOMRequests[0].Result as IDataModelWithContext).PnPContext = tenantAdminContext;

                return result.ApiCall.CSOMRequests[0].Result as ISiteCollectionProperties;
            }
        }

        internal static async Task UpdateSiteCollectionPropertiesAsync(PnPContext context, SiteCollectionProperties properties)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new SetSitePropertiesRequest(properties)
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                await WaitForSpoOperationCompleteAsync(tenantAdminContext, result.ApiCall.CSOMRequests[0].Result as SpoOperation).ConfigureAwait(false);
            }
        }

    }
}
