using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents an interface to work with Application Lifecycle Management (ALM) for the tenant app catalog.
    /// </summary>
    public interface ITenantAppManager : IAppManager<ITenantApp>
    {
        /// <summary>
        /// Returns the URI of the current tenant app catalog.
        /// </summary>
        /// <returns></returns>
        Task<Uri> GetTenantAppCatalogUriAsync();

        /// <summary>
        /// Returns the URI of the current tenant app catalog.
        /// </summary>
        /// <returns></returns>
        Uri GetTenantAppCatalogUri();

        /// <summary>
        /// Ensures there's a tenant app catalog, if not present it will be created.
        /// </summary>
        /// <returns>True if the app catalog was created, false if the app catalog already existed.</returns>
        Task<bool> EnsureTenantAppCatalogAsync();

        /// <summary>
        /// Ensures there's a tenant app catalog, if not present it will be created.
        /// </summary>
        /// <returns>True if the app catalog was created, false if the app catalog already existed.</returns>
        bool EnsureTenantAppCatalog();

        /// <summary>
        /// Indicates whether a solution contains MS Teams component. 
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if the solution contains teams component.</returns>
        bool SolutionContainsTeamsComponent(Guid id);

        /// <summary>
        /// Indicates whether a solution contains MS Teams component. 
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if the solution contains teams component.</returns>
        Task<bool> SolutionContainsTeamsComponentAsync(Guid id);

        /// <summary>
        /// Returns a list of site collection app catalogs in the tenant.
        /// </summary>
        /// <returns>A list of <see cref="IAppCatalogSite"/></returns>
        IList<IAppCatalogSite> GetSiteCollectionAppCatalogs();

        /// <summary>
        /// Returns a list of site collection app catalogs in the tenant.
        /// </summary>
        /// <returns>A list of <see cref="IAppCatalogSite"/></returns>
        Task<IList<IAppCatalogSite>> GetSiteCollectionAppCatalogsAsync();

        /// <summary>
        /// A list of apps, added to the tenant from the SharePoint Store.
        /// </summary>
        /// <returns>A list of <see cref="ITenantApp"/></returns>
        IList<ITenantApp> GetStoreApps();

        /// <summary>
        /// A list of apps, added to the tenant from the SharePoint Store.
        /// </summary>
        /// <returns>A list of <see cref="ITenantApp"/></returns>
        Task<IList<ITenantApp>> GetStoreAppsAsync();

        /// <summary>
        /// Uploads SharePoint Store app to the tenant app catalog and deploys it.
        /// </summary>
        /// <param name="storeAssetId">A unique store asset id. If you open the SharePoint Store app's home page, the url will be ...appStore.aspx/appDetail/WA200001111. The last part <em>WA200001111</em> will be your store asset id.</param>
        /// <param name="cultureName">4-letters culture name, i.e. "en-us", "de-de", etc.</param>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment and will install the app globally.</param>
        /// <param name="overwrite">Whether to overwrite if the app is already exisits in the tenant app catalog.</param>
        ITenantApp AddAndDeployStoreApp(string storeAssetId, string cultureName, bool skipFeatureDeployment = true, bool overwrite = true);

        /// <summary>
        /// Uploads SharePoint Store app to the tenant app catalog and deploys it.
        /// </summary>
        /// <param name="storeAssetId">A unique store asset id. If you open the SharePoint Store app's home page, the url will be ...appStore.aspx/appDetail/WA200001111. The last part <em>WA200001111</em> will be your store asset id.</param>
        /// <param name="cultureName">4-letters culture name, i.e. "en-us", "de-de", etc.</param>
        /// <param name="skipFeatureDeployment">If set to true will skip the feature deployment and will install the app globally.</param>
        /// <param name="overwrite">Whether to overwrite if the app is already exisits in the tenant app catalog.</param>
        Task<ITenantApp> AddAndDeployStoreAppAsync(string storeAssetId, string cultureName, bool skipFeatureDeployment = true, bool overwrite = true);

        /// <summary>
        /// Indicates whether the upgrade is available for the specific app on a site.
        /// </summary>
        /// <param name="id">List item id of the app in the AppCatalog list.</param>
        /// <returns><em>true</em> if update is available.</returns>
        bool IsAppUpgradeAvailable(int id);

        /// <summary>
        /// Indicates whether the upgrade is available for the specific app on a site.
        /// </summary>
        /// <param name="id">List item id of the app in the AppCatalog list.</param>
        /// <returns><em>true</em> if update is available.</returns>
        Task<bool> IsAppUpgradeAvailableAsync(int id);

        /// <summary>
        /// Indicates whether the upgrade is available for the specific app on a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if update is available.</returns>
        bool IsAppUpgradeAvailable(Guid id);

        /// <summary>
        /// Indicates whether the upgrade is available for the specific app on a site.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><em>true</em> if update is available.</returns>
        Task<bool> IsAppUpgradeAvailableAsync(Guid id);

        /// <summary>
        /// Downloads MS Teams package from SPFx solution as a stream. You can save the stream to a file with a .zip extension later on.
        /// </summary>
        /// <param name="id">List item id of the app in the AppCatalog list.</param>
        /// <returns><see cref="Stream"/> with the MS Teams binary package.</returns>
        Stream DownloadTeamsSolution(int id);

        /// <summary>
        /// Downloads MS Teams package from SPFx solution as a stream. You can save the stream to a file with a .zip extension later on.
        /// </summary>
        /// <param name="id">List item id of the app in the AppCatalog list.</param>
        /// <returns><see cref="Stream"/> with the MS Teams binary package.</returns>

        Task<Stream> DownloadTeamsSolutionAsync(int id);

        /// <summary>
        /// Downloads MS Teams package from SPFx solution as a stream. You can save the stream to a file with a .zip extension later on.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><see cref="Stream"/> with the MS Teams binary package.</returns>
        Stream DownloadTeamsSolution(Guid id);

        /// <summary>
        /// Downloads MS Teams package from SPFx solution as a stream. You can save the stream to a file with a .zip extension later on.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns><see cref="Stream"/> with the MS Teams binary package.</returns>
        Task<Stream> DownloadTeamsSolutionAsync(Guid id);
    }
}
