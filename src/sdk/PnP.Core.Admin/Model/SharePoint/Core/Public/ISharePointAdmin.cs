using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// SharePoint Admin features
    /// </summary>
    public interface ISharePointAdmin
    {
        /// <summary>
        /// Returns the SharePoint tenant admin center url
        /// </summary>
        /// <returns>SharePoint tenant admin center url</returns>
        Task<Uri> GetTenantAdminCenterUriAsync();

        /// <summary>
        /// Returns the SharePoint tenant admin center url
        /// </summary>
        /// <returns>SharePoint tenant admin center url</returns>
        Uri GetTenantAdminCenterUri();

        /// <summary>
        /// Returns a <see cref="PnPContext"/> for the tenant's SharePoint admin center site
        /// </summary>
        /// <returns><see cref="PnPContext"/> for the tenant's SharePoint admin center</returns>
        Task<PnPContext> GetTenantAdminCenterContextAsync();

        /// <summary>
        /// Returns a <see cref="PnPContext"/> for the tenant's SharePoint admin center site
        /// </summary>
        /// <returns><see cref="PnPContext"/> for the tenant's SharePoint admin center</returns>
        PnPContext GetTenantAdminCenterContext();

        /// <summary>
        /// Returns the URI of the current tenant app catalog
        /// </summary>
        /// <returns></returns>
        Task<Uri> GetTenantAppCatalogUriAsync();

        /// <summary>
        /// Returns the URI of the current tenant app catalog
        /// </summary>
        /// <returns></returns>
        Uri GetTenantAppCatalogUri();

        /// <summary>
        /// Ensures there's a tenant app catalog, if not present it will be created
        /// </summary>
        /// <returns>True if the app catalog was created, false if the app catalog already existed</returns>
        Task<bool> EnsureTenantAppCatalogAsync();

        /// <summary>
        /// Ensures there's a tenant app catalog, if not present it will be created
        /// </summary>
        /// <returns>True if the app catalog was created, false if the app catalog already existed</returns>
        bool EnsureTenantAppCatalog();

    }
}
