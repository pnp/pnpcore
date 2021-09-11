using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
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
            if (uriParts[0].EndsWith("-admin"))
            {
                return new Uri($"https://{uriParts[0]}.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my"))
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
            if (uriParts[0].EndsWith("-admin"))
            {
                return new Uri($"https://{uriParts[0].Replace("-admin", "")}.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my"))
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
            if (uriParts[0].EndsWith("-admin"))
            {
                return new Uri($"https://{uriParts[0].Replace("-admin", "")}-my.{string.Join(".", uriParts.Skip(1))}");
            }

            if (uriParts[0].EndsWith("-my"))
            {
                return new Uri($"https://{uriParts[0]}.{string.Join(".", uriParts.Skip(1))}");
            }

            return new Uri($"https://{uriParts[0]}-my.{string.Join(".", uriParts.Skip(1))}");
        }

        public async Task<PnPContext> GetTenantAdminCenterContextAsync()
        {
            return await context.CloneAsync(await GetTenantAdminCenterUriAsync().ConfigureAwait(false));
        }

        public PnPContext GetTenantAdminCenterContext()
        {
            return GetTenantAdminCenterContextAsync().GetAwaiter().GetResult();
        }

        public async Task<Uri> GetTenantAppCatalogUriAsync()
        {
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/SP_TenantSettings_Current", ApiType.SPORest), HttpMethod.Get);

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
            var result = await (context.Web as Web).RawRequestAsync(new ApiCall("_api/web/EnsureTenantAppCatalog(callerId='pnpcoresdk')", ApiType.SPORest), HttpMethod.Post);
            var root = JsonDocument.Parse(result.Json).RootElement.GetProperty("d").GetProperty("EnsureTenantAppCatalog");
            return root.GetBoolean();
        }

        public bool EnsureTenantAppCatalog()
        {
            return EnsureTenantAppCatalogAsync().GetAwaiter().GetResult();
        }

    }
}
