using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// SharePoint Admin features
    /// </summary>
    public interface ISharePointAdmin
    {
        /// <summary>
        /// Returns the SharePoint tenant admin center url (e.g. https://contoso-admin.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant admin center url</returns>
        Task<Uri> GetTenantAdminCenterUriAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the SharePoint tenant admin center url (e.g. https://contoso-admin.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant admin center url</returns>
        Uri GetTenantAdminCenterUri(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the SharePoint tenant portal url (e.g. https://contoso.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant portal url</returns>
        Task<Uri> GetTenantPortalUriAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the SharePoint tenant portal url (e.g. https://contoso.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant portal url</returns>
        Uri GetTenantPortalUri(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the SharePoint tenant my site host url (e.g. https://contoso-my.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant my site host url</returns>
        Task<Uri> GetTenantMySiteHostUriAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the SharePoint tenant my site host url (e.g. https://contoso-my.sharepoint.com)
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>SharePoint tenant my site host url</returns>
        Uri GetTenantMySiteHostUri(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a <see cref="PnPContext"/> for the tenant's SharePoint admin center site
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns><see cref="PnPContext"/> for the tenant's SharePoint admin center</returns>
        Task<PnPContext> GetTenantAdminCenterContextAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a <see cref="PnPContext"/> for the tenant's SharePoint admin center site
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns><see cref="PnPContext"/> for the tenant's SharePoint admin center</returns>
        PnPContext GetTenantAdminCenterContext(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of <see cref="ISharePointUser"/>s who are SharePoint Online Tenant admin
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>List of SharePoint Online Tenant admins</returns>
        Task<List<ISharePointUser>> GetTenantAdminsAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of <see cref="ISharePointUser"/>s who are SharePoint Online Tenant admin
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>List of SharePoint Online Tenant admins</returns>
        List<ISharePointUser> GetTenantAdmins(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Checks if the current user is SharePoint Online tenant admin
        /// </summary>
        /// <returns>True if the user is a SharePoint Online tenant admin, false otherwise</returns>
        Task<bool> IsCurrentUserTenantAdminAsync();

        /// <summary>
        /// Checks if the current user is SharePoint Online tenant admin
        /// </summary>
        /// <returns>True if the user is a SharePoint Online tenant admin, false otherwise</returns>
        bool IsCurrentUserTenantAdmin();

        /// <summary>
        /// Gets the properties of this tenant
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>Properties of the tenant</returns>
        Task<ITenantProperties> GetTenantPropertiesAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Gets the properties of this tenant
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>Properties of the tenant</returns>
        ITenantProperties GetTenantProperties(VanityUrlOptions vanityUrlOptions = null);

    }
}
