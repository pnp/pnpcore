﻿using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// SharePoint Site Collection features
    /// </summary>
    public interface ISiteCollectionManager
    {
        /// <summary>
        /// Returns the list of site collections. When using application permissions or a delegated permissions 
        /// for a SharePoint admin account all site collections are returned, otherwise only the site collections
        /// accessible by the requesting user are returned. Under the covers this method uses different approaches:
        /// - Application permissions: using the Sites endpoint via Graph
        /// - Delegated permissions, user is SharePoint Tenant Admin: querying the sites list maintained in the SharePoint Tenant Admin site
        /// - Delegated permissions, non admin: using the Search endpoint via Graph
        /// </summary>
        /// <param name="ignoreUserIsSharePointAdmin">When set to true and when the user is SharePoint admin then only return the site collections accessible by the user</param>
        /// <param name="filter">Optional filter to scope the returned site collections</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections</returns>
        Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the list of site collections. When using application permissions or a delegated permissions 
        /// for a SharePoint admin account all site collections are returned, otherwise only the site collections
        /// accessible by the requesting user are returned. Under the covers this method uses different approaches:
        /// - Application permissions: using the Sites endpoint via Graph
        /// - Delegated permissions, user is SharePoint Tenant Admin: querying the sites list maintained in the SharePoint Tenant Admin site
        /// - Delegated permissions, non admin: using the Search endpoint via Graph
        /// </summary>
        /// <param name="ignoreUserIsSharePointAdmin">When set to true and when the user is SharePoint admin then only return the site collections accessible by the user</param>
        /// <param name="filter">Optional filter to scope the returned site collections</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections</returns>
        List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of the site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections with details</returns>
        Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of the site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections with details</returns>
        List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns details about the requested site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="url">Uri of the site collection to get details for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>Site collection details, null if the passed site was not found</returns>
        Task<ISiteCollectionWithDetails> GetSiteCollectionWithDetailsAsync(Uri url, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns details about the requested site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="url">Uri of the site collection to get details for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>Site collection details, null if the passed site was not found</returns>
        ISiteCollectionWithDetails GetSiteCollectionWithDetails(Uri url, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of the recycled site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections with details</returns>
        Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of the recycled site collections in the current tenant including details about the site. This method
        /// queries a hidden list in the SharePoint Tenant Admin site and therefore requires the user or application to 
        /// have the proper permissions
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A list of site collections with details</returns>
        List<IRecycledSiteCollection> GetRecycledSiteCollections(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns a list of all sub sites for the passed in site collection
        /// If the current context or passed url are for a sub web then the all sub webs of that sub web are returned
        /// </summary>
        /// <returns>List of webs with details</returns>
        /// <param name="url">Optional URL of the site collection to get the sub sites for. If null the sub sites are retreived for the current site collection</param>
        /// <param name="skipAppWebs">Skips the SharePoint app webs (APP#0)</param>
        Task<List<IWebWithDetails>> GetSiteCollectionWebsWithDetailsAsync(Uri url = null, bool skipAppWebs = true);

        /// <summary>
        /// Returns a list of all sub sites for the passed in site collection
        /// If the current context or passed url are for a sub web then the all sub webs of that sub web are returned
        /// </summary>
        /// <returns>List of webs with details</returns>
        /// <param name="url">Optional URL of the site collection to get the sub sites for. If null the sub sites are retreived for the current site collection</param>
        /// <param name="skipAppWebs">Skips the SharePoint app webs (APP#0)</param>
        List<IWebWithDetails> GetSiteCollectionWebsWithDetails(Uri url = null, bool skipAppWebs = true);

        /// <summary>
        /// Creates a site collection and returns a <see cref="PnPContext"/> to start using the created site collection
        /// </summary>
        /// <param name="siteToCreate">Information about the site collection to create. 
        /// Pass in a <see cref="CommunicationSiteOptions"/>, <see cref="TeamSiteOptions"/>, <see cref="TeamSiteWithoutGroupOptions"/> or <see cref="ClassicSiteOptions"/> instance.</param>
        /// <param name="creationOptions">Options that control the site creation process</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A <see cref="PnPContext"/> to start using the created site collection</returns>
        Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Creates a site collection and returns a <see cref="PnPContext"/> to start using the created site collection
        /// </summary>
        /// <param name="siteToCreate">Information about the site collection to create. 
        /// Pass in a <see cref="CommunicationSiteOptions"/>, <see cref="TeamSiteOptions"/>, <see cref="TeamSiteWithoutGroupOptions"/> or <see cref="ClassicSiteOptions"/> instance.</param>
        /// <param name="creationOptions">Options that control the site creation process</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>A <see cref="PnPContext"/> to start using the created site collection</returns>
        PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Check if site exists
        /// </summary>
        /// <param name="url">Site url</param>
        /// <returns>True if the site exists, false otherwise</returns>
        Task<bool> SiteExistsAsync(Uri url);

        /// <summary>
        /// Check if site exists
        /// </summary>
        /// <param name="url">Site url</param>
        /// <returns>True if the site exists, false otherwise</returns>
        bool SiteExists(Uri url);

        /// <summary>
        /// Recycle a site collection. The site collection ends up in the recycle bin and can be restored. When the site collection
        /// has a connected group then also that group is automatically recycled
        /// </summary>
        /// <param name="siteToDelete">Site collection to recycle</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task RecycleSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Recycle a site collection. The site collection ends up in the recycle bin and can be restored. When the site collection
        /// has a connected group then also that group is automatically recycled
        /// </summary>
        /// <param name="siteToDelete">Site collection to recycle</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void RecycleSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Restores a site collection from the recycle bin. When the site collection
        /// has a connected group then also that group is automatically restored
        /// </summary>
        /// <param name="siteToRestore">Site collection to restore</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task RestoreSiteCollectionAsync(Uri siteToRestore, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Restores a site collection from the recycle bin. When the site collection
        /// has a connected group then also that group is automatically restored
        /// </summary>
        /// <param name="siteToRestore">Site collection to restore</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void RestoreSiteCollection(Uri siteToRestore, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Deletes a site collection. The deleted site collection is also removed from the recycle bin!
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task DeleteSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Deletes a site collection. The deleted site collection is also removed from the recycle bin!
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void DeleteSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Deletes a site collection from the recycle bin.
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete from the recycle bin</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task DeleteRecycledSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Deletes a site collection from the recycle bin.
        /// </summary>
        /// <param name="siteToDelete">Site collection to delete from the recycle bin</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void DeleteRecycledSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the properties of a site collection
        /// </summary>
        /// <param name="site">Site collection to get the properties for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>The site collection properties</returns>
        Task<ISiteCollectionProperties> GetSiteCollectionPropertiesAsync(Uri site, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Returns the properties of a site collection
        /// </summary>
        /// <param name="site">Site collection to get the properties for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>The site collection properties</returns>
        ISiteCollectionProperties GetSiteCollectionProperties(Uri site, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Connects an existing site collection to a new Microsoft 365 group
        /// </summary>
        /// <param name="siteGroupConnectOptions">Information needed to handle the connection of the site collection to a new Microsoft 365 group. </param>
        /// <param name="creationOptions">Options to control the connect to site process</param>
        Task ConnectSiteCollectionToGroupAsync(ConnectSiteToGroupOptions siteGroupConnectOptions, CreationOptions creationOptions = null);

        /// <summary>
        /// Connects an existing site collection to a new Microsoft 365 group
        /// </summary>
        /// <param name="siteGroupConnectOptions">Information needed to handle the connection of the site collection to a new Microsoft 365 group. </param>
        /// <param name="creationOptions">Options to control the connect to site process</param>
        void ConnectSiteCollectionToGroup(ConnectSiteToGroupOptions siteGroupConnectOptions, CreationOptions creationOptions = null);

        /// <summary>
        /// Gets the administrators of the site collection
        /// </summary>
        /// <param name="site">Url of the site collection to get the administrators for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>The list of site collection administrators</returns>
        Task<List<ISiteCollectionAdmin>> GetSiteCollectionAdminsAsync(Uri site, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Gets the administrators of the site collection
        /// </summary>
        /// <param name="site">Url of the site collection to get the administrators for</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns>The list of site collection administrators</returns>
        List<ISiteCollectionAdmin> GetSiteCollectionAdmins(Uri site, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Sets the administrators of the site collection by providing the list of login names. The first in the list will be the primary admin, the others will be
        /// secondary admins. When the site collection is group connected you can also opt to set group owners as they are also SharePoint site collection administrators.
        /// To stay in sync with with SharePoint Tenant admin center does, when adding a group owner the user is also added as group member.
        /// </summary>
        /// <param name="site">Url of the site collection to set the administrators for</param>
        /// <param name="sharePointAdminLoginNames">List of SharePoint Admins login names (e.g. i:0#.f|membership|anna@contoso.onmicrosoft.com) to set as admin</param>
        /// <param name="ownerGroupAzureAdUserIds">List of Azure AD user ids to set as admin via adding them to the connected Microsoft 365 group owners</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task SetSiteCollectionAdminsAsync(Uri site, List<string> sharePointAdminLoginNames = null, List<Guid> ownerGroupAzureAdUserIds = null, VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Sets the administrators of the site collection by providing the list of login names. The first in the list will be the primary admin, the others will be
        /// secondary admins. When the site collection is group connected you can also opt to set group owners as they are also SharePoint site collection administrators.
        /// To stay in sync with with SharePoint Tenant admin center does, when adding a group owner the user is also added as group member.
        /// </summary>
        /// <param name="site">Url of the site collection to set the administrators for</param>
        /// <param name="sharePointAdminLoginNames">List of SharePoint Admins login names (e.g. i:0#.f|membership|anna@contoso.onmicrosoft.com) to set as admin</param>
        /// <param name="ownerGroupAzureAdUserIds">List of Azure AD user ids to set as admin via adding them to the connected Microsoft 365 group owners</param>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void SetSiteCollectionAdmins(Uri site, List<string> sharePointAdminLoginNames = null, List<Guid> ownerGroupAzureAdUserIds = null, VanityUrlOptions vanityUrlOptions = null);

        #region Modernization

        /// <summary>
        /// Hides the Add Microsoft Teams banner. Only works when the site collection was already connected to an Microsoft 365 group
        /// </summary>
        /// <param name="site">Url of the site collection to hide the Add Teams prompt for</param>
        /// <returns>True if hidden</returns>
        Task<bool> HideAddTeamsPromptAsync(Uri site);

        /// <summary>
        /// Hides the Add Microsoft Teams banner. Only works when the site collection was already connected to an Microsoft 365 group
        /// </summary>
        /// <param name="site">Url of the site collection to hide the Add Teams prompt for</param>
        /// <returns>True if hidden</returns>
        bool HideAddTeamsPrompt(Uri site);

        /// <summary>
        /// Checks if the Add Microsoft Teams banner is hidden. Only works when the site collection was already connected to an Microsoft 365 group
        /// </summary>
        /// <param name="site">Url of the site collection to check the Add Teams prompt status for</param>
        /// <returns>True if hidden, false otherwise.</returns>
        Task<bool> IsAddTeamsPromptHiddenAsync(Uri site);

        /// <summary>
        /// Checks if the Add Microsoft Teams banner is hidden. Only works when the site collection was already connected to an Microsoft 365 group.
        /// </summary>
        /// <param name="site">Url of the site collection to check the Add Teams prompt status for</param>
        /// <returns>True if hidden, false otherwise.</returns>
        bool IsAddTeamsPromptHidden(Uri site);

        /// <summary>
        /// Enables the communication site features on this team site using the Topic design. 
        /// Requirements:
        /// - Only works when the site collection was not connected to an Microsoft 365 group
        /// - Web is root web of the site collection, cannot be applied to sub sites
        /// - Web template is "STS#0" or "EHS#1" (so TeamSite)
        /// </summary>
        /// <param name="site">Url of the team site collection to enable communication site features for</param>
        /// <returns></returns>
        Task EnableCommunicationSiteFeaturesAsync(Uri site);

        /// <summary>
        /// Enables the communication site features on this team site using the Topic design. 
        /// Requirements:
        /// - Only works when the site collection was not connected to an Microsoft 365 group
        /// - Web is root web of the site collection, cannot be applied to sub sites
        /// - Web template is "STS#0" or "EHS#1" (so TeamSite)
        /// </summary>
        /// <param name="site">Url of the team site collection to enable communication site features for</param>
        /// <returns></returns>
        void EnableCommunicationSiteFeatures(Uri site);

        /// <summary>
        /// Enables the communication site features on this team site using the Topic design. 
        /// Requirements:
        /// - Use 96c933ac-3698-44c7-9f4a-5fd17d71af9e (Topic), 6142d2a0-63a5-4ba0-aede-d9fefca2c767 (Showcase) or f6cc5403-0d63-442e-96c0-285923709ffc (Blank) as design package id
        /// - Only works when the site collection was not connected to an Microsoft 365 group
        /// - Web is root web of the site collection, cannot be applied to sub sites
        /// - Web template is "STS#0" or "EHS#1" (so TeamSite)
        /// </summary>
        /// <param name="site">Url of the team site collection to enable communication site features for</param>
        /// <param name="designPackageId">Design package id to apply</param>
        /// <returns></returns>
        Task EnableCommunicationSiteFeaturesAsync(Uri site, Guid designPackageId);

        /// <summary>
        /// Enables the communication site features on this team site using the Topic design. 
        /// Requirements:
        /// - Use 96c933ac-3698-44c7-9f4a-5fd17d71af9e (Topic), 6142d2a0-63a5-4ba0-aede-d9fefca2c767 (Showcase) or f6cc5403-0d63-442e-96c0-285923709ffc (Blank) as design package id
        /// - Only works when the site collection was not connected to an Microsoft 365 group
        /// - Web is root web of the site collection, cannot be applied to sub sites
        /// - Web template is "STS#0" or "EHS#1" (so TeamSite)
        /// </summary>
        /// <param name="site">Url of the team site collection to enable communication site features for</param>
        /// <param name="designPackageId">Design package id to apply</param>
        /// <returns></returns>
        void EnableCommunicationSiteFeatures(Uri site, Guid designPackageId);

        #endregion

        /// <summary>
        /// Get the current context
        /// </summary>
        /// <returns></returns>
        PnPContext GetContext();
    }
}
