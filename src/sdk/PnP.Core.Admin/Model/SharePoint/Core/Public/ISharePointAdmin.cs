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
        /// <returns>SharePoint tenant admin center url</returns>
        Task<Uri> GetTenantAdminCenterUriAsync();

        /// <summary>
        /// Returns the SharePoint tenant admin center url (e.g. https://contoso-admin.sharepoint.com)
        /// </summary>
        /// <returns>SharePoint tenant admin center url</returns>
        Uri GetTenantAdminCenterUri();

        /// <summary>
        /// Returns the SharePoint tenant portal url (e.g. https://contoso.sharepoint.com)
        /// </summary>
        /// <returns>SharePoint tenant portal url</returns>
        Task<Uri> GetTenantPortalUriAsync();

        /// <summary>
        /// Returns the SharePoint tenant portal url (e.g. https://contoso.sharepoint.com)
        /// </summary>
        /// <returns>SharePoint tenant portal url</returns>
        Uri GetTenantPortalUri();

        /// <summary>
        /// Returns the SharePoint tenant my site host url (e.g. https://contoso-my.sharepoint.com)
        /// </summary>
        /// <returns>SharePoint tenant my site host url</returns>
        Task<Uri> GetTenantMySiteHostUriAsync();

        /// <summary>
        /// Returns the SharePoint tenant my site host url (e.g. https://contoso-my.sharepoint.com)
        /// </summary>
        /// <returns>SharePoint tenant my site host url</returns>
        Uri GetTenantMySiteHostUri();

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
        /// Returns a list of <see cref="ISharePointUser"/>s who are SharePoint Online Tenant admin
        /// </summary>
        /// <returns>List of SharePoint Online Tenant admins</returns>
        Task<List<ISharePointUser>> GetTenantAdminsAsync();

        /// <summary>
        /// Returns a list of <see cref="ISharePointUser"/>s who are SharePoint Online Tenant admin
        /// </summary>
        /// <returns>List of SharePoint Online Tenant admins</returns>
        List<ISharePointUser> GetTenantAdmins();

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

        /// <summary>
        /// Returns the list of site collections. When using application permissions or a delegated permissions 
        /// for a SharePoint admin account all site collections are returned, otherwise only the site collections
        /// accessible by the requesting user are returned. Under the covers this method uses different approaches:
        /// - Application permissions: using the Sites endpoint via Graph
        /// - Delegated permissions, user is SharePoint Tenant Admin: querying the sites list maintained in the SharePoint Tenant Admin site
        /// - Delegated permissions, non admin: using the Search endpoint via Graph
        /// </summary>
        /// <param name="ignoreUserIsSharePointAdmin">When set to true and when the user is SharePoint admin then only return the site collections accessible by the user</param>
        /// <returns>A list of site collections</returns>
        Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false);

        /// <summary>
        /// Returns the list of site collections. When using application permissions or a delegated permissions 
        /// for a SharePoint admin account all site collections are returned, otherwise only the site collections
        /// accessible by the requesting user are returned. Under the covers this method uses different approaches:
        /// - Application permissions: using the Sites endpoint via Graph
        /// - Delegated permissions, user is SharePoint Tenant Admin: querying the sites list maintained in the SharePoint Tenant Admin site
        /// - Delegated permissions, non admin: using the Search endpoint via Graph
        /// </summary>
        /// <param name="ignoreUserIsSharePointAdmin">When set to true and when the user is SharePoint admin then only return the site collections accessible by the user</param>
        /// <returns>A list of site collections</returns>
        List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false);

        /// <summary>
        /// Returns a list of the site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <returns>A list of site collections with details</returns>
        Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync();

        /// <summary>
        /// Returns a list of the site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <returns>A list of site collections with details</returns>
        List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails();

        /// <summary>
        /// Returns a list of the recycled site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <returns>A list of site collections with details</returns>
        Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync();

        /// <summary>
        /// Returns a list of the recycled site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <returns>A list of site collections with details</returns>
        List<IRecycledSiteCollection> GetRecycledSiteCollections();

        /// <summary>
        /// Creates a site collection and returns a <see cref="PnPContext"/> to start using the created site collection
        /// </summary>
        /// <param name="siteToCreate">Information about the site collection to create. 
        /// Pass in a <see cref="CommunicationSiteOptions"/>, <see cref="TeamSiteOptions"/>, <see cref="TeamSiteWithoutGroupOptions"/> or <see cref="ClassicSiteOptions"/> instance.</param>
        /// <param name="creationOptions"></param>
        /// <returns>A <see cref="PnPContext"/> to start using the created site collection</returns>
        Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null);

        /// <summary>
        /// Creates a site collection and returns a <see cref="PnPContext"/> to start using the created site collection
        /// </summary>
        /// <param name="siteToCreate">Information about the site collection to create. 
        /// Pass in a <see cref="CommunicationSiteOptions"/>, <see cref="TeamSiteOptions"/>, <see cref="TeamSiteWithoutGroupOptions"/> or <see cref="ClassicSiteOptions"/> instance.</param>
        /// <param name="creationOptions"></param>
        /// <returns>A <see cref="PnPContext"/> to start using the created site collection</returns>
        PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null);

        /// <summary>
        /// Recycle a site collection. The site collection ends up in the recycle bin and can be restored. When the site collection
        /// has a connected group then also that group is automatically recycled
        /// </summary>
        /// <param name="siteToDelete">Site collection to recycle</param>
        /// <param name="webTemplate">The web template (e.g. STS#3, GROUP#0) of the site collection is used to determine the best delete approach</param>
        /// <returns></returns>
        Task RecycleSiteCollectionAsync(Uri siteToDelete, string webTemplate);

        /// <summary>
        /// Recycle a site collection. The site collection ends up in the recycle bin and can be restored. When the site collection
        /// has a connected group then also that group is automatically recycled
        /// </summary>
        /// <param name="siteToDelete">Site collection to recycle</param>
        /// <param name="webTemplate">The web template (e.g. STS#3, GROUP#0) of the site collection is used to determine the best delete approach</param>
        /// <returns></returns>
        void RecycleSiteCollection(Uri siteToDelete, string webTemplate);

        /// <summary>
        /// Restores a site collection from the recycle bin. When the site collection
        /// has a connected group then also that group is automatically restored
        /// </summary>
        /// <param name="siteToRestore">Site collection to restore</param>
        /// <returns></returns>
        Task RestoreSiteCollectionAsync(Uri siteToRestore);

        /// <summary>
        /// Restores a site collection from the recycle bin. When the site collection
        /// has a connected group then also that group is automatically restored
        /// </summary>
        /// <param name="siteToRestore">Site collection to restore</param>
        /// <returns></returns>
        void RestoreSiteCollection(Uri siteToRestore);

        /// <summary>
        /// Deletes a site collection. The deleted site collection is also removed from the recycle bin!
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete</param>
        /// <param name="webTemplate">The web template (e.g. STS#3, GROUP#0) of the site collection is used to determine the best delete approach</param>
        /// <returns></returns>
        Task DeleteSiteCollectionAsync(Uri siteToDelete, string webTemplate);

        /// <summary>
        /// Deletes a site collection. The deleted site collection is also removed from the recycle bin!
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete</param>
        /// <param name="webTemplate">The web template (e.g. STS#3, GROUP#0) of the site collection is used to determine the best delete approach</param>
        /// <returns></returns>
        void DeleteSiteCollection(Uri siteToDelete, string webTemplate);
    }
}
