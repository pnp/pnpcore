using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.SearchConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class SharePointAdmin : ISharePointAdmin
    {
        private readonly PnPContext context;

        internal SharePointAdmin(PnPContext pnpContext)
        {
            context = pnpContext;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantAdminCenterUriAsync(VanityUrlOptions vanityUrlOptions = null)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var uri = context.Web.Url;

            var uriParts = uri.Host.Split('.');

            if (uriParts[1].Equals("sharepoint", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetTenantAdminCenterUriForStandardTenants(uri);
            }
            else
            {
                // Tenant is using vanity urls
                if (vanityUrlOptions == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                if (vanityUrlOptions.AdminCenterUri == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                return vanityUrlOptions.AdminCenterUri;
            }
        }

        internal static Uri GetTenantAdminCenterUriForStandardTenants(Uri uri)
        {
            var uriParts = uri.Host.Split('.');
            if (uriParts[0].EndsWith("-admin", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0]}.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0].Replace("-my", "")}-admin.{string.Join(".", uriParts.Skip(1))}");
            }

            return new Uri($"https://{uriParts[0]}-admin.{string.Join(".", uriParts.Skip(1))}");
        }

        public Uri GetTenantAdminCenterUri(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantAdminCenterUriAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantPortalUriAsync(VanityUrlOptions vanityUrlOptions = null)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var uri = context.Web.Url;

            var uriParts = uri.Host.Split('.');

            if (uriParts[1].Equals("sharepoint", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetTenantPortalUriForStandardTenants(uri);
            }
            else
            {
                // Tenant is using vanity urls
                if (vanityUrlOptions == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                if (vanityUrlOptions.PortalUri == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                return vanityUrlOptions.PortalUri;
            }
        }

        internal static Uri GetTenantPortalUriForStandardTenants(Uri uri)
        {
            var uriParts = uri.Host.Split('.');
            if (uriParts[0].EndsWith("-admin", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0].Replace("-admin", "")}.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0].Replace("-my", "")}.{string.Join(".", uriParts.Skip(1))}");
            }

            return new Uri($"https://{uriParts[0]}.{string.Join(".", uriParts.Skip(1))}");
        }

        public Uri GetTenantPortalUri(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantPortalUriAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantMySiteHostUriAsync(VanityUrlOptions vanityUrlOptions = null)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var uri = context.Web.Url;

            var uriParts = uri.Host.Split('.');

            if (uriParts[1].Equals("sharepoint", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetTenantMySiteHostUriForStandardTenants(uri);
            }
            else
            {
                // Tenant is using vanity urls
                if (vanityUrlOptions == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                if (vanityUrlOptions.MySiteHostUri == null)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_MissingVanityUrlDetails);
                }

                return vanityUrlOptions.MySiteHostUri;
            }
        }

        public Uri GetTenantMySiteHostUri(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantMySiteHostUriAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        internal static Uri GetTenantMySiteHostUriForStandardTenants(Uri uri)
        {
            var uriParts = uri.Host.Split('.');
            if (uriParts[0].EndsWith("-admin", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0].Replace("-admin", "")}-my.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my", StringComparison.InvariantCulture))
            {
                return new Uri($"https://{uriParts[0]}.{string.Join(".", uriParts.Skip(1))}");
            }

            return new Uri($"https://{uriParts[0]}-my.{string.Join(".", uriParts.Skip(1))}");
        }

        public async Task<PnPContext> GetTenantAdminCenterContextAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            // We've already established a connection to the SharePoint Tenant admin center site
            var tenantAdminCenterUri = await GetTenantAdminCenterUriAsync(vanityUrlOptions).ConfigureAwait(false);
            if (context.Uri == tenantAdminCenterUri)
            {
                return context;
            }

            return await context.CloneAsync(tenantAdminCenterUri).ConfigureAwait(false);
        }

        public PnPContext GetTenantAdminCenterContext(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantAdminCenterContextAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISharePointUser>> GetTenantAdminsAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using (var tenantAdminCenterContext = await GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                return await tenantAdminCenterContext.Web.SiteUsers.Where(p => p.IsSiteAdmin == true).ToListAsync().ConfigureAwait(false);
            }
        }

        public List<ISharePointUser> GetTenantAdmins(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantAdminsAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<bool> IsCurrentUserTenantAdminAsync()
        {
            var result = await (context.Web.WithHeaders(new Dictionary<string, string>() { { "ConsistencyLevel", "eventual" } }) as Web).RawRequestAsync(
                new ApiCall("me/memberOf?$count=true&$search=\"displayName: Company Administrator\" OR \"displayName: Global Administrator\"", ApiType.Graph), HttpMethod.Get).ConfigureAwait(false);

            var json = JsonSerializer.Deserialize<JsonElement>(result.Json);

            if (json.GetProperty("value").ValueKind != JsonValueKind.Undefined)
            {
                // "62e90394-69f5-4237-9190-012177145e10" = global tenant admin role template id
                return json.GetProperty("value").EnumerateArray().Any(r => r.GetProperty("roleTemplateId").GetString() == "62e90394-69f5-4237-9190-012177145e10");
            }
            return false;
        }

        public bool IsCurrentUserTenantAdmin()
        {
            return IsCurrentUserTenantAdminAsync().GetAwaiter().GetResult();
        }

        public async Task<ITenantProperties> GetTenantPropertiesAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new GetTenantPropertiesRequest()
                };

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
                (result.ApiCall.CSOMRequests[0].Result as IDataModelWithContext).PnPContext = tenantAdminContext;

                return result.ApiCall.CSOMRequests[0].Result as ITenantProperties;
            }
        }

        public ITenantProperties GetTenantProperties(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantPropertiesAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        #region Get Search Configuration

        public async Task<string> GetTenantSearchConfigurationXmlAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                ApiCall apiCall = new ApiCall(new List<IRequest<object>> { new ExportSearchConfigurationRequest(SearchObjectLevel.SPSiteSubscription) });

                var result = await (tenantAdminContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

                return result.ApiCall.CSOMRequests[0].Result.ToString();
            }
        }

        public string GetTenantSearchConfigurationXml(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantSearchConfigurationXmlAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IManagedProperty>> GetTenantSearchConfigurationManagedPropertiesAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            var searchConfiguration = await GetTenantSearchConfigurationXmlAsync(vanityUrlOptions).ConfigureAwait(false);

            return SearchConfigurationHandler.GetManagedPropertiesFromConfigurationXml(searchConfiguration);
        }

        public List<IManagedProperty> GetTenantSearchConfigurationManagedProperties(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantSearchConfigurationManagedPropertiesAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task SetTenantSearchConfigurationXmlAsync(string configuration, VanityUrlOptions vanityUrlOptions = null)
        {
            if (string.IsNullOrEmpty(configuration))
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                ApiCall apiCall = new(new List<IRequest<object>> { new ImportSearchConfigurationRequest(SearchObjectLevel.SPSiteSubscription, configuration) });

                await (tenantAdminContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void SetTenantSearchConfigurationXml(string configuration, VanityUrlOptions vanityUrlOptions = null)
        {
            SetTenantSearchConfigurationXmlAsync(configuration, vanityUrlOptions).GetAwaiter().GetResult();
        }
        #endregion

        #region Hub site functionality
        public async Task ConnectToHubSiteAsync(Uri siteCollectionUrlToConnect, Uri hubSiteCollectionUrl, VanityUrlOptions vanityUrlOptions = null)
        {
            using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync(vanityUrlOptions).ConfigureAwait(false))
            {
                ApiCall apiCall = new(new List<IRequest<object>> { new ConnectSiteToHubSiteRequest(hubSiteCollectionUrl, siteCollectionUrlToConnect) });

                await (tenantAdminContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void ConnectToHubSite(Uri siteCollectionUrlToConnect, Uri hubSiteCollectionUrl, VanityUrlOptions vanityUrlOptions = null)
        {
            ConnectToHubSiteAsync(siteCollectionUrlToConnect, hubSiteCollectionUrl, vanityUrlOptions).GetAwaiter().GetResult();
        }
        #endregion
    }
}
