using PnP.Core.Services;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class App : IApp
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string AadAppId { get; set; }
        public string AadPermissions { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version AppCatalogVersion { get; set; }

        public bool CanUpgrade { get; set; }
        public string CDNLocation { get; set; }
        public bool ContainsTenantWideExtension { get; set; }
        public bool CurrentVersionDeployed { get; set; }
        public bool Deployed { get; set; }
        public string ErrorMessage { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version InstalledVersion { get; set; }

        public bool IsClientSideSolution { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsPackageDefaultSkipFeatureDeployment { get; set; }
        public bool IsValidAppPackage { get; set; }
        public Guid ProductId { get; set; }
        public string ShortDescription { get; set; }
        public bool SkipDeploymentFeature { get; set; }
        public string ThumbnailUrl { get; set; }
        public PnPContext PnPContext { get; set; }

        
        public IOAuth2PermissionGrant[] ApprovePermissionRequests()
        {
            return ApprovePermissionRequestsAsync().GetAwaiter().GetResult();
        }

        public async Task<IOAuth2PermissionGrant[]> ApprovePermissionRequestsAsync()
        {
            return await GetAppManager().ApproveAsync( AadPermissions ).ConfigureAwait(false);
        }

        public bool Deploy(bool skipFeatureDeployment = true)
        {
            return DeployAsync(skipFeatureDeployment).GetAwaiter().GetResult();
        }

        public async Task<bool> DeployAsync(bool skipFeatureDeployment = true)
        {
            return await GetAppManager().DeployAsync(Id, skipFeatureDeployment).ConfigureAwait(false);
        }

        public bool Install()
        {
            return InstallAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> InstallAsync()
        {
            return await GetAppManager().InstallAsync(Id).ConfigureAwait(false);
        }

        public bool Remove()
        {
            return RemoveAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveAsync()
        {
            return await GetAppManager().RemoveAsync(Id).ConfigureAwait(false);
        }

        public bool Retract()
        {
            return RetractAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> RetractAsync()
        {
            return await GetAppManager().RetractAsync(Id).ConfigureAwait(false);
        }

        public bool Uninstall()
        {
            return UninstallAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> UninstallAsync()
        {
            return await GetAppManager().UninstallAsync(Id).ConfigureAwait(false);
        }

        public bool Upgrade()
        {
            return UpgradeAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> UpgradeAsync()
        {
            return await GetAppManager().UpgradeAsync(Id).ConfigureAwait(false);
        }

        protected IAppOperations GetAppManager()
        {
            if (this is TenantApp)
            {
                return PnPContext.GetTenantAppManager();
            }
            else
            {
                return PnPContext.GetSiteCollectionAppManager();
            }
        }
    }
}