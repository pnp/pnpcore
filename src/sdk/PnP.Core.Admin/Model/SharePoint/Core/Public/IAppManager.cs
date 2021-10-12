using System;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// App Admin features
    /// </summary>
    public interface IAppManager
    {
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
