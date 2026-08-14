using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies a tenant template's <c>&lt;pnp:Tenant&gt;</c> element, once, before any site in the
    /// sequence is created.
    /// </summary>
    internal class ObjectHierarchyTenant : ObjectHierarchyHandlerBase
    {
        public override string Name => "Tenant Settings";

        public override bool WillProvision(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ApplyConfiguration configuration)
        {
            ProvisioningTenant tenant = hierarchy?.Tenant;

            _willProvision ??= tenant != null
                && (tenant.AppCatalog != null
                    || tenant.ContentDeliveryNetwork != null
                    || tenant.SiteDesigns?.Count > 0
                    || tenant.SiteScripts?.Count > 0
                    || tenant.StorageEntities?.Count > 0
                    || tenant.WebApiPermissions?.Count > 0
                    || tenant.Themes?.Count > 0
                    || tenant.SPUsersProfiles?.Count > 0
                    || tenant.Office365GroupLifecyclePolicies?.Count > 0
                    || tenant.Office365GroupsSettings?.Properties?.Count > 0
                    || tenant.SharingSettings != null);

            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningHierarchy hierarchy, string sequenceId,
            ExtractConfiguration configuration)
        {
            _willExtract ??= false;
            return _willExtract.Value;
        }

        public override Task<ProvisioningHierarchy> ExtractObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(hierarchy);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningHierarchy hierarchy,
            string sequenceId, TokenParser parser, ApplyConfiguration configuration)
        {
            var inner = new ObjectTenant
            {
                MessagesDelegate = MessagesDelegate,
            };

            return await inner.ApplyTenantAsync(context, hierarchy?.Tenant, hierarchy?.Connector, parser)
                .ConfigureAwait(false);
        }
    }
}
