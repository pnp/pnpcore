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
    }
}
