using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Interface for base app operations, like Deploy, Install, etc.
    /// </summary>
    public interface IAppOperations
    {
        /// <summary>
        /// Deploys / trusts an app in the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment for tenant scoped apps.</param>
        /// <returns><em>true</em> if deployment was successful.</returns>
        bool Deploy(Guid id, bool skipFeatureDeployment = true);

        /// <summary>
        /// Deploys / trusts an app in the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment for tenant scoped apps.</param>
        /// <returns><em>true</em> if deployment was successful.</returns>
        Task<bool> DeployAsync(Guid id, bool skipFeatureDeployment = true);

        /// <summary>
        /// Retracts the app in the app catalog. Notice that this will not remove the app from the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if retract was successful.</returns>
        bool Retract(Guid id);

        /// <summary>
        /// Retracts the app in the app catalog. Notice that this will not remove the app from the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if retract was successful.</returns>
        Task<bool> RetractAsync(Guid id);

        /// <summary>
        /// Removes the app from the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if remove was successful.</returns>
        bool Remove(Guid id);

        /// <summary>
        /// Removes the app from the app catalog.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if remove was successful.</returns>
        Task<bool> RemoveAsync(Guid id);

        /// <summary>
        /// Installs the app from the app catalog in a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if installation was successful.</returns>
        bool Install(Guid id);

        /// <summary>
        /// Installs the app from the app catalog in a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if installation was successful.</returns>
        Task<bool> InstallAsync(Guid id);

        /// <summary>
        /// Upgrades the app in a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if upgrade was successful.</returns>
        bool Upgrade(Guid id);

        /// <summary>
        /// Upgrades the app in a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if upgrade was successful.</returns>
        Task<bool> UpgradeAsync(Guid id);

        /// <summary>
        /// Uninstalls the app from a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if uninstall was successful.</returns>
        bool Uninstall(Guid id);

        /// <summary>
        /// Uninstalls the app from a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if uninstall was successful.</returns>
        Task<bool> UninstallAsync(Guid id);

        /// <summary>
        /// Approves all permissions requested by the app.
        /// </summary>
        /// <param name="aadPermissions">The requested AAD permissions, e.g. </param>
        /// <returns></returns>
        Task<IOAuth2PermissionGrant[]> ApproveAsync(string aadPermissions);
    }
}
