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
        private PnPContext context;

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

            if (uriParts[1].Equals("SharePoint", StringComparison.InvariantCultureIgnoreCase))
            {
                if (uriParts[0].EndsWith("-admin"))
                {
                    return new Uri(uri.OriginalString);
                }

                if (!uriParts[0].EndsWith("-admin"))
                {
                    return new Uri($"https://{uriParts[0]}-admin.{string.Join(".", uriParts.Skip(1))}");
                }
            }
            else
            {
                // Tenant is using vanity urls
                // TODO: check for alternative method
            }

            return null;
        }

        public Uri GetTenantAdminCenterUri()
        {
            return GetTenantAdminCenterUriAsync().GetAwaiter().GetResult();
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
