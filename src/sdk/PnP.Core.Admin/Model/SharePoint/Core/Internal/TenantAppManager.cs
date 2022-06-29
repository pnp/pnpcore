using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class TenantAppManager : AppManager<ITenantApp>, ITenantAppManager
    {
        protected override string Scope => "tenant";

        internal TenantAppManager(PnPContext pnpContext) : base(pnpContext)
        {
        }

        /*

        Currently SyncToTeams API has issues and doesn't work with OAuth access tokens
        https://github.com/SharePoint/sp-dev-docs/issues/7441
         
        public void SyncToTeams(Guid id)
        {
            SyncToTeamsAsync(id).GetAwaiter().GetResult();
        }

        public async Task SyncToTeamsAsync(Guid id)
        {
            await SyncToTeamsInternalAsync("SyncSolutionToTeamsByUniqueId", id).ConfigureAwait(false);
        }

        public void SyncToTeams(int id)
        {
            SyncToTeamsAsync(id).GetAwaiter().GetResult();
        }

        public async Task SyncToTeamsAsync(int id)
        {
            await SyncToTeamsInternalAsync("SyncSolutionToTeams", id).ConfigureAwait(false);
        }

        private async Task SyncToTeamsInternalAsync(string method, object id)
        {
            var apiCall = GetGenericApiCallByObjectId(method, id);

            await ExecuteGenericApiPostAsync(apiCall, (response) =>
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                var result = document.Get(method);

                if (result == null)
                {
                    throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
                }

                return true;
            }).ConfigureAwait(false);
        }

        */

        public bool SolutionContainsTeamsComponent(Guid id)
        {
            return SolutionContainsTeamsComponentAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> SolutionContainsTeamsComponentAsync(Guid id)
        {
            var apiCall = GetGenericApiCallByObjectId("SolutionContainsTeamsComponent", id);

            return await ExecuteGenericApiPostAsync(apiCall, (response) =>
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                var result = document.Get("value");

                if (result == null)
                {
                    throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
                }

                return result.Value.GetBoolean();
            }).ConfigureAwait(false);
        }

        public IList<IAppCatalogSite> GetSiteCollectionAppCatalogs()
        {
            return GetSiteCollectionAppCatalogsAsync().GetAwaiter().GetResult();
        }

        public async Task<IList<IAppCatalogSite>> GetSiteCollectionAppCatalogsAsync()
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/SiteCollectionAppCatalogsSites", ApiType.SPORest);
            var pnpContext = await GetTenantAppCatalogContextAsync().ConfigureAwait(false);

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                var response = await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                return GetModelListFromJson<IAppCatalogSite>(response.Json);
            }).ConfigureAwait(false);
        }

        public void EnsureSiteCollectionAppCatalog(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            EnsureSiteCollectionAppCatalogAsync(siteCollectionAbsoluteUri, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task EnsureSiteCollectionAppCatalogAsync(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            var existingSiteCollectionAppCatalogs = await GetSiteCollectionAppCatalogsAsync().ConfigureAwait(false);
            if(existingSiteCollectionAppCatalogs.FirstOrDefault(p=>p.AbsoluteUrl.Equals(siteCollectionAbsoluteUri.ToString(), StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                // no site collection app catalog yet, so create one
                await AddSiteCollectionAppCatalogAsync(siteCollectionAbsoluteUri, vanityUrlOptions).ConfigureAwait(false);
            }
        }

        public void RemoveSiteCollectionAppCatalog(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            RemoveSiteCollectionAppCatalogAsync(siteCollectionAbsoluteUri, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task RemoveSiteCollectionAppCatalogAsync(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteCollectionAbsoluteUri == null)
            {
                throw new ArgumentNullException(nameof(siteCollectionAbsoluteUri));
            }

            ApiCall apiCall = new ApiCall(new List<IRequest<object>> { new RemoveSiteCollectionAppCatalogRequest(siteCollectionAbsoluteUri) });
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                await (tenantAdminContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void AddSiteCollectionAppCatalog(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            AddSiteCollectionAppCatalogAsync(siteCollectionAbsoluteUri, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task AddSiteCollectionAppCatalogAsync(Uri siteCollectionAbsoluteUri, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteCollectionAbsoluteUri == null)
            {
                throw new ArgumentNullException(nameof(siteCollectionAbsoluteUri));
            }

            ApiCall apiCall = new ApiCall(new List<IRequest<object>> { new AddSiteCollectionAppCatalogRequest(siteCollectionAbsoluteUri) });
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {

                await (tenantAdminContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public IList<ITenantApp> GetStoreApps()
        {
            return GetStoreAppsAsync().GetAwaiter().GetResult();
        }

        public async Task<IList<ITenantApp>> GetStoreAppsAsync()
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/StoreApps", ApiType.SPORest);
            var response = await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return GetModelListFromJson<ITenantApp>(response.Json);
        }

        public ITenantApp AddAndDeployStoreApp(string storeAssetId, string cultureName, bool skipFeatureDeployment = true, bool overwrite = true)
        {
            return AddAndDeployStoreAppAsync(storeAssetId, cultureName, skipFeatureDeployment, overwrite).GetAwaiter().GetResult();
        }

        public async Task<ITenantApp> AddAndDeployStoreAppAsync(string storeAssetId, string cultureName, bool skipFeatureDeployment = true, bool overwrite = true)
        {
            var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/AddAndDeployStoreAppById(storeAssetId='{storeAssetId}',overwrite='{overwrite}',cmu='{cultureName}',skipFeatureDeployment='{skipFeatureDeployment}')", ApiType.SPORest);

            var pnpContext = await GetTenantAppCatalogContextAsync().ConfigureAwait(false);

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                var list = await pnpContext.Web.Lists.GetByServerRelativeUrlAsync($"{pnpContext.Uri.LocalPath}/appcatalog").ConfigureAwait(false);
                var query = new CamlQueryOptions
                {
                    ViewXml = $"<View><Query><Where><Contains><FieldRef Name='FileLeafRef'/><Value Type='Text'>{storeAssetId}</Value></Contains></Where></Query></View>"
                };

                await list.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);
                var item = list.Items.AsRequested().Single();

                return await GetAvailableAsync(item.Title).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public bool IsAppUpgradeAvailable(int id)
        {
            return IsAppUpgradeAvailableAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> IsAppUpgradeAvailableAsync(int id)
        {
            var apiCall = GetGenericApiCallByObjectId("IsAppUpgradeAvailable", id);

            return await ExecuteGenericApiPostAsync(apiCall, (response) =>
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                var result = document.Get("IsUpgradeAvailable");

                if (result == null)
                {
                    throw new ClientException(PnPCoreAdminResources.Exception_UnexpectedJson);
                }

                return result.Value.GetBoolean();
            }, false).ConfigureAwait(false);
        }

        public bool IsAppUpgradeAvailable(Guid id)
        {
            return IsAppUpgradeAvailableAsync(id).GetAwaiter().GetResult();
        }

        public async Task<bool> IsAppUpgradeAvailableAsync(Guid id)
        {
            var pnpContext = await GetTenantAppCatalogContextAsync().ConfigureAwait(false);

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                var list = await pnpContext.Web.Lists.GetByServerRelativeUrlAsync($"{pnpContext.Uri.LocalPath}/appcatalog").ConfigureAwait(false);
                var query = new CamlQueryOptions
                {
                    ViewXml = $"<View><Query><Where><Contains><FieldRef Name='UniqueId'/><Value Type='Text'>{id}</Value></Contains></Where></Query></View>"
                };
                await list.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);
                var item = list.Items.AsRequested().Single();

                return await IsAppUpgradeAvailableAsync(item.Id).ConfigureAwait(false);

            }).ConfigureAwait(false);
        }

        public Stream DownloadTeamsSolution(int id)
        {
            return DownloadTeamsSolutionAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Stream> DownloadTeamsSolutionAsync(int id)
        {
            return await DownloadTeamsSolutionInternalAsync(id, "DownloadTeamsSolution").ConfigureAwait(false);
        }

        public Stream DownloadTeamsSolution(Guid id)
        {
            return DownloadTeamsSolutionAsync(id).GetAwaiter().GetResult();
        }

        public async Task<Stream> DownloadTeamsSolutionAsync(Guid id)
        {
            return await DownloadTeamsSolutionInternalAsync(id, "DownloadTeamsSolutionByUniqueId").ConfigureAwait(false);
        }

        private async Task<Stream> DownloadTeamsSolutionInternalAsync(object id, string method)
        {
            var pnpContext = await GetTenantAppCatalogContextAsync().ConfigureAwait(false);

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                var apiCall = new ApiCall($"_api/web/{Scope}appcatalog/{method}('{id}')/DownloadTeams", ApiType.SPORest)
                {
                    ExpectBinaryResponse = true,
                    Interactive = true
                };

                var response = await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                response.BinaryContent.Seek(0, SeekOrigin.Begin);

                return response.BinaryContent;
            }).ConfigureAwait(false);
        }

        private async Task<P> ExecuteGenericApiPostAsync<P>(ApiCall apiCall, Func<ApiCallResponse, P> handler, bool switchToTenantCatalogContext = true)
        {
            PnPContext pnpContext;
            if (switchToTenantCatalogContext)
            {
                pnpContext = await GetTenantAppCatalogContextAsync().ConfigureAwait(false);
            }
            else
            {
                pnpContext = context;
            }

            return await ExecuteWithDisposeAsync(pnpContext, async () =>
            {
                var response = await (pnpContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                return handler(response);
            }).ConfigureAwait(false);
        }

        private ApiCall GetGenericApiCallByObjectId(string method, object id)
        {
            return new ApiCall($"_api/web/{Scope}appcatalog/{method}('{id}')", ApiType.SPORest);
        }
    }
}
