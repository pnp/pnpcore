using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SharePointAdmin : ISharePointAdmin
    {
        private readonly PnPContext context;

        internal SharePointAdmin(PnPContext pnpContext)
        {
            context = pnpContext;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantAdminCenterUriAsync()
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
                // TODO: check for alternative method
                throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_VanityUrl);
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

        public Uri GetTenantAdminCenterUri()
        {
            return GetTenantAdminCenterUriAsync().GetAwaiter().GetResult();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantPortalUriAsync()
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
                // TODO: check for alternative method
                throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_VanityUrl);
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

        public Uri GetTenantPortalUri()
        {
            return GetTenantPortalUriAsync().GetAwaiter().GetResult();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Uri> GetTenantMySiteHostUriAsync()
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
                // TODO: check for alternative method
                throw new ClientException(ErrorType.Unsupported, PnPCoreAdminResources.Exception_VanityUrl);
            }
        }

        public Uri GetTenantMySiteHostUri()
        {
            return GetTenantMySiteHostUriAsync().GetAwaiter().GetResult();
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

        public async Task<PnPContext> GetTenantAdminCenterContextAsync()
        {
            // We've already established a connection to the SharePoint Tenant admin center site
            var tenantAdminCenterUri = await GetTenantAdminCenterUriAsync().ConfigureAwait(false);
            if (context.Uri == tenantAdminCenterUri)
            {
                return context;
            }

            return await context.CloneAsync(tenantAdminCenterUri).ConfigureAwait(false);
        }

        public PnPContext GetTenantAdminCenterContext()
        {
            return GetTenantAdminCenterContextAsync().GetAwaiter().GetResult();
        }

        public async Task<List<ISharePointUser>> GetTenantAdminsAsync()
        {
            using (var tenantAdminCenterContext = await GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                return await tenantAdminCenterContext.Web.SiteUsers.Where(p => p.IsSiteAdmin == true).ToListAsync().ConfigureAwait(false);    
            }
        }

        public List<ISharePointUser> GetTenantAdmins()
        {
            return GetTenantAdminsAsync().GetAwaiter().GetResult();
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

        public async Task<Uri> GetTenantAppCatalogUriAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/SP_TenantSettings_Current", ApiType.SPORest), HttpMethod.Get).ConfigureAwait(false);

            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("d").GetProperty("CorporateCatalogUrl");

            if (root.ValueKind == JsonValueKind.String)
            {
                return new Uri(root.GetString());
            }

            return null;
        }

        public Uri GetTenantAppCatalogUri()
        {
            return GetTenantAppCatalogUriAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> EnsureTenantAppCatalogAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/web/EnsureTenantAppCatalog(callerId='pnpcoresdk')", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("d").GetProperty("EnsureTenantAppCatalog");
            return root.GetBoolean();
        }

        public bool EnsureTenantAppCatalog()
        {
            return EnsureTenantAppCatalogAsync().GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false)
        {
            return await SiteCollectionEnumerator.GetAsync(context, ignoreUserIsSharePointAdmin).ConfigureAwait(false);
        }

        public List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false)
        {
            return GetSiteCollectionsAsync(ignoreUserIsSharePointAdmin).GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync()
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails()
        {
            return GetSiteCollectionsWithDetailsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync()
        {
            return await SiteCollectionEnumerator.GetRecycledWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<IRecycledSiteCollection> GetRecycledSiteCollections()
        {
            return GetRecycledSiteCollectionsAsync().GetAwaiter().GetResult();
        }

        public async Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            return await SiteCollectionCreator.CreateSiteCollectionAsync(context, siteToCreate, creationOptions).ConfigureAwait(false);
        }

        public PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            return CreateSiteCollectionAsync(siteToCreate, creationOptions).GetAwaiter().GetResult();
        }

        public async Task RecycleSiteCollectionAsync(Uri siteToDelete, string webTemplate)
        {
            await SiteCollectionManager.RecycleSiteCollectionAsync(context, siteToDelete, webTemplate).ConfigureAwait(false);
        }

        public void RecycleSiteCollection(Uri siteToDelete, string webTemplate)
        {
            RecycleSiteCollectionAsync(siteToDelete, webTemplate).GetAwaiter().GetResult();
        }

        public void RestoreSiteCollection(Uri siteToRestore)
        {
            RestoreSiteCollectionAsync(siteToRestore).GetAwaiter().GetResult();
        }

        public async Task RestoreSiteCollectionAsync(Uri siteToRestore)
        {
            await SiteCollectionManager.RestoreSiteCollectionAsync(context, siteToRestore).ConfigureAwait(false);
        }

        public async Task DeleteSiteCollectionAsync(Uri siteToDelete, string webTemplate)
        {
            await SiteCollectionManager.DeleteSiteCollectionAsync(context, siteToDelete, webTemplate).ConfigureAwait(false);
        }

        public void DeleteSiteCollection(Uri siteToDelete, string webTemplate)
        {
            DeleteSiteCollectionAsync(siteToDelete, webTemplate).GetAwaiter().GetResult();
        }
    }
}
