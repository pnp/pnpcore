using PnP.Core.Model;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents a base interface for an app stored either in tenant or site collection app catalog.
    /// </summary>
    public interface IApp : IDataModelWithContext
    {
        /// <summary>
        /// Unique ID of the library list item of the app.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Title of the app.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Azure Active Directory Id of the SharePoint Online Client Extensibility Web Application Principal.
        /// It's not empty if your SPFx solution requests any AAD permissions.
        /// </summary>
        public string AadAppId { get; set; }

        /// <summary>
        /// The list of Azure Active Directory permissions your SPFx solution requests.
        /// </summary>
        public string AadPermissions { get; set; }

        /// <summary>
        /// Returns version of the app in the app catalog.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        public Version AppCatalogVersion { get; set; }

        /// <summary>
        /// Returns whether an existing instance of the app can be upgraded.
        /// True if there's newer version available in the app catalog compared to the instance in the site.
        /// </summary>
        public bool CanUpgrade { get; set; }

        /// <summary>
        /// Returns the url of CDN if your app is hosted on CDN. If it's hosted inside SharePoint, it returns "SharePoint Online".
        /// </summary>
        public string CDNLocation { get; set; }

        /// <summary>
        /// Indicates whether your app contains tenant wide extensions.
        /// </summary>
        public bool ContainsTenantWideExtension { get; set; }

        /// <summary>
        /// Indicates whether the current version of the app is deployed.
        /// </summary>
        public bool CurrentVersionDeployed { get; set; }

        /// <summary>
        /// Indicates whether the app has been deployed to the context site.
        /// True if particular app has been installed to the site.
        /// </summary>
        public bool Deployed { get; set; }

        /// <summary>
        /// Contains an error message if the app contains any problems during deployment.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Returns a version of the installed app in the site context.
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        public Version InstalledVersion { get; set; }

        /// <summary>
        /// Indicates whether the app is SharePoint Framework client-side solution.
        /// </summary>
        public bool IsClientSideSolution { get; set; }

        /// <summary>
        /// Whether the app is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Returns true, if the app's config (./config/package-solution.json) contains "skipFeatureDeployment" setting and it's set to true.
        /// </summary>
        public bool IsPackageDefaultSkipFeatureDeployment { get; set; }

        /// <summary>
        /// Indicates whether the app package is valid.
        /// </summary>
        public bool IsValidAppPackage { get; set; }

        /// <summary>
        /// The app's product id.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// The short description of the app.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Returns true, if the app was globally deployed.
        /// </summary>
        public bool SkipDeploymentFeature { get; set; }

        /// <summary>
        /// The thumbnail url of the app.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Deploys / trusts an app in the app catalog.
        /// </summary>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment for tenant scoped apps.</param>
        /// <returns><em>true</em> if deployment was successful.</returns>
        bool Deploy(bool skipFeatureDeployment = true);

        /// <summary>
        /// Deploys / trusts an app in the app catalog.
        /// </summary>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment for tenant scoped apps.</param>
        /// <returns><em>true</em> if deployment was successful.</returns>
        Task<bool> DeployAsync(bool skipFeatureDeployment = true);

        /// <summary>
        /// Approves All PermissionRequests 
        /// </summary>
        /// <returns><em>true</em> if all permissions have been approved successfully.</returns>
        IOAuth2PermissionGrant[] ApprovePermissionRequests();
        
        /// <summary>
        /// Approves All PermissionRequests 
        /// </summary>
        /// <returns></returns>
        Task<IOAuth2PermissionGrant[]> ApprovePermissionRequestsAsync();

        /// <summary>
        /// Retracts the app in the app catalog. Notice that this will not remove the app from the app catalog.
        /// </summary>
        /// <returns><em>true</em> if retract was successful.</returns>
        bool Retract();

        /// <summary>
        /// Retracts the app in the app catalog. Notice that this will not remove the app from the app catalog.
        /// </summary>
        /// <returns><em>true</em> if retract was successful.</returns>
        Task<bool> RetractAsync();

        /// <summary>
        /// Removes the app from the app catalog.
        /// </summary>
        /// <returns><em>true</em> if remove was successful.</returns>
        bool Remove();

        /// <summary>
        /// Removes the app from the app catalog.
        /// </summary>
        /// <returns><em>true</em> if remove was successful.</returns>
        Task<bool> RemoveAsync();

        /// <summary>
        /// Installs the app from the app catalog in a site.
        /// </summary>
        /// <returns><em>true</em> if installation was successful.</returns>
        bool Install();

        /// <summary>
        /// Installs the app from the app catalog in a site.
        /// </summary>
        /// <returns><em>true</em> if installation was successful.</returns>
        Task<bool> InstallAsync();

        /// <summary>
        /// Upgrades the app in a site.
        /// </summary>
        /// <returns><em>true</em> if upgrade was successful.</returns>
        bool Upgrade();

        /// <summary>
        /// Upgrades the app in a site.
        /// </summary>
        /// <returns><em>true</em> if upgrade was successful.</returns>
        Task<bool> UpgradeAsync();

        /// <summary>
        /// Uninstalls the app from a site.
        /// </summary>
        /// <returns><em>true</em> if uninstall was successful.</returns>
        bool Uninstall();

        /// <summary>
        /// Uninstalls the app from a site.
        /// </summary>
        /// <returns><em>true</em> if uninstall was successful.</returns>
        Task<bool> UninstallAsync();
    }
}
