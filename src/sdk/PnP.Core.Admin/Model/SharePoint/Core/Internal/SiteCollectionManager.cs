using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal sealed class SiteCollectionManager : ISiteCollectionManager
    {
        private readonly PnPContext context;

        internal SiteCollectionManager(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        public async Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default, VanityUrlOptions vanityUrlOptions = null)
        {
            return await SiteCollectionEnumerator.GetAsync(context, vanityUrlOptions, ignoreUserIsSharePointAdmin, filter).ConfigureAwait(false);
        }

        public List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetSiteCollectionsAsync(ignoreUserIsSharePointAdmin, filter, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync(VanityUrlOptions vanityUrlOptions = null, bool includeSharedAndPrivateTeamChannelSites = false)
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context, vanityUrlOptions: vanityUrlOptions, includeSharedAndPrivateTeamChannelSites).ConfigureAwait(false);
        }

        public List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails(VanityUrlOptions vanityUrlOptions = null, bool includeSharedAndPrivateTeamChannelSites = false)
        {
            return GetSiteCollectionsWithDetailsAsync(vanityUrlOptions, includeSharedAndPrivateTeamChannelSites).GetAwaiter().GetResult();
        }

        public async Task<ISiteCollectionWithDetails> GetSiteCollectionWithDetailsAsync(Uri url, VanityUrlOptions vanityUrlOptions = null)
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context, url, vanityUrlOptions: vanityUrlOptions).ConfigureAwait(false);
        }

        public ISiteCollectionWithDetails GetSiteCollectionWithDetails(Uri url, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetSiteCollectionWithDetailsAsync(url, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            return await SiteCollectionEnumerator.GetRecycledWithDetailsViaTenantAdminHiddenListAsync(context, vanityUrlOptions: vanityUrlOptions).ConfigureAwait(false);
        }

        public List<IRecycledSiteCollection> GetRecycledSiteCollections(VanityUrlOptions vanityUrlOptions = null)
        {
            return GetRecycledSiteCollectionsAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IWebWithDetails>> GetSiteCollectionWebsWithDetailsAsync(Uri url = null, bool skipAppWebs = true)
        {
            return await WebEnumerator.GetWithDetailsAsync(context, url, skipAppWebs).ConfigureAwait(false);
        }

        public List<IWebWithDetails> GetSiteCollectionWebsWithDetails(Uri url = null, bool skipAppWebs = true)
        {
            return GetSiteCollectionWebsWithDetailsAsync(url, skipAppWebs).GetAwaiter().GetResult();
        }

        public async Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteToCreate == null)
            {
                throw new ArgumentNullException(nameof(siteToCreate));
            }

            return await SiteCollectionCreator.CreateSiteCollectionAsync(context, siteToCreate, creationOptions, vanityUrlOptions).ConfigureAwait(false);
        }

        public PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null, VanityUrlOptions vanityUrlOptions = null)
        {
            return CreateSiteCollectionAsync(siteToCreate, creationOptions, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<bool> SiteExistsAsync(Uri url)
        {
            try
            {
                using (PnPContext cloneContext = await context.CloneAsync(url).ConfigureAwait(false))
                {
                    return true;
                }
            }
            catch (SharePointRestServiceException ex)
            {
                if (ex.IsUnableToAccessSiteException() || ex.IsCannotGetSiteException())
                {
                    return true;
                }

                return false;
            }
        }

        public bool SiteExists(Uri url)
        {
            return SiteExistsAsync(url).GetAwaiter().GetResult();
        }

        public async Task RecycleSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteToDelete == null)
            {
                throw new ArgumentNullException(nameof(siteToDelete));
            }
            
            await SiteCollectionManagement.RecycleSiteCollectionAsync(context, siteToDelete, vanityUrlOptions).ConfigureAwait(false);
        }

        public void RecycleSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            RecycleSiteCollectionAsync(siteToDelete, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public void RestoreSiteCollection(Uri siteToRestore, VanityUrlOptions vanityUrlOptions = null)
        {
            RestoreSiteCollectionAsync(siteToRestore, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task RestoreSiteCollectionAsync(Uri siteToRestore, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteToRestore == null)
            {
                throw new ArgumentNullException(nameof(siteToRestore));
            }

            await SiteCollectionManagement.RestoreSiteCollectionAsync(context, siteToRestore, vanityUrlOptions).ConfigureAwait(false);
        }

        public async Task DeleteSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteToDelete == null)
            {
                throw new ArgumentNullException(nameof(siteToDelete));
            }

            await SiteCollectionManagement.DeleteSiteCollectionAsync(context, siteToDelete, vanityUrlOptions).ConfigureAwait(false);
        }

        public void DeleteSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            DeleteSiteCollectionAsync(siteToDelete, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task DeleteRecycledSiteCollectionAsync(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            if (siteToDelete == null)
            {
                throw new ArgumentNullException(nameof(siteToDelete));
            }

            await SiteCollectionManagement.DeleteRecycledSiteCollectionAsync(context, siteToDelete, vanityUrlOptions).ConfigureAwait(false);
        }

        public void DeleteRecycledSiteCollection(Uri siteToDelete, VanityUrlOptions vanityUrlOptions = null)
        {
            DeleteRecycledSiteCollectionAsync(siteToDelete, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<ISiteCollectionProperties> GetSiteCollectionPropertiesAsync(Uri site, VanityUrlOptions vanityUrlOptions = null)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return await SiteCollectionManagement.GetSiteCollectionPropertiesByUrlAsync(context, site, true, vanityUrlOptions).ConfigureAwait(false);
        }

        public ISiteCollectionProperties GetSiteCollectionProperties(Uri site, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetSiteCollectionPropertiesAsync(site, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task ConnectSiteCollectionToGroupAsync(ConnectSiteToGroupOptions siteGroupConnectOptions, CreationOptions creationOptions = null)
        {
            if (siteGroupConnectOptions == null)
            {
                throw new ArgumentNullException(nameof(siteGroupConnectOptions));
            }

            await SiteCollectionCreator.ConnectGroupToSiteAsync(context, siteGroupConnectOptions, creationOptions).ConfigureAwait(false);
        }

        public void ConnectSiteCollectionToGroup(ConnectSiteToGroupOptions siteGroupConnectOptions, CreationOptions creationOptions = null)
        {
            ConnectSiteCollectionToGroupAsync(siteGroupConnectOptions, creationOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollectionAdmin>> GetSiteCollectionAdminsAsync(Uri site, VanityUrlOptions vanityUrlOptions = null)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return await SiteCollectionManagement.GetSiteCollectionAdminsAsync(context, site, vanityUrlOptions).ConfigureAwait(false);
        }

        public List<ISiteCollectionAdmin> GetSiteCollectionAdmins(Uri site, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetSiteCollectionAdminsAsync(site, vanityUrlOptions).GetAwaiter().GetResult();
        }
        
        public async Task SetSiteCollectionAdminsAsync(Uri site, List<string> sharePointAdminLoginNames = null, 
            List<Guid> ownerGroupAzureAdUserIds = null, CollectionUpdateOptions collectionUpdateOptions = CollectionUpdateOptions.AddOnly,
            VanityUrlOptions vanityUrlOptions = null)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            await SiteCollectionManagement.SetSiteCollectionAdminsAsync(context, site, sharePointAdminLoginNames, ownerGroupAzureAdUserIds, collectionUpdateOptions, vanityUrlOptions).ConfigureAwait(false);
        }
        
        public void SetSiteCollectionAdmins(Uri site, List<string> sharePointAdminLoginNames = null, 
            List<Guid> ownerGroupAzureAdUserIds = null, CollectionUpdateOptions collectionUpdateOptions = CollectionUpdateOptions.AddOnly,  
            VanityUrlOptions vanityUrlOptions = null)
        {
            SetSiteCollectionAdminsAsync(site, sharePointAdminLoginNames, ownerGroupAzureAdUserIds, collectionUpdateOptions, vanityUrlOptions).GetAwaiter().GetResult();
        }

        #region Modernization
        public async Task<bool> HideAddTeamsPromptAsync(Uri url)
        {
            return await SiteCollectionModernizer.HideAddTeamsPromptAsync(context, url).ConfigureAwait(false);
        }

        public bool HideAddTeamsPrompt(Uri url)
        {
            return HideAddTeamsPromptAsync(url).GetAwaiter().GetResult();
        }

        public async Task<bool> IsAddTeamsPromptHiddenAsync(Uri url)
        {
            return await SiteCollectionModernizer.IsAddTeamsPromptHiddenAsync(context, url).ConfigureAwait(false);
        }

        public bool IsAddTeamsPromptHidden(Uri url)
        {
            return IsAddTeamsPromptHiddenAsync(url).GetAwaiter().GetResult();
        }

        public async Task EnableCommunicationSiteFeaturesAsync(Uri url, Guid designPackageId)
        {
            await SiteCollectionModernizer.EnableCommunicationSiteFeaturesAsync(context, url, designPackageId).ConfigureAwait(false);
        }

        public void EnableCommunicationSiteFeatures(Uri url, Guid designPackageId)
        {
            EnableCommunicationSiteFeaturesAsync(url, designPackageId).GetAwaiter().GetResult();
        }

        public async Task EnableCommunicationSiteFeaturesAsync(Uri url)
        {
            await EnableCommunicationSiteFeaturesAsync(url, PnPAdminConstants.CommunicationSiteDesignTopic).ConfigureAwait(false);
        }

        public void EnableCommunicationSiteFeatures(Uri url)
        {
            EnableCommunicationSiteFeaturesAsync(url).GetAwaiter().GetResult();
        }
        #endregion

        #region Azure ACS principals and SharePoint AddIn discovery

        public async Task<List<IACSPrincipal>> GetSiteCollectionACSPrincipalsAsync(bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null)
        {
            var results = await LegacyPrincipalManagement.GetACSPrincipalsAsync(context, null, includeSubsites, false, vanityUrlOptions).ConfigureAwait(false);
            return results.Cast<IACSPrincipal>().ToList();
        }

        public List<IACSPrincipal> GetSiteCollectionACSPrincipals(bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetSiteCollectionACSPrincipalsAsync(includeSubsites, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IACSPrincipal>> GetTenantAndSiteCollectionACSPrincipalsAsync(List<ILegacyServicePrincipal> legacyServicePrincipals, bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null)
        {
            var results = await LegacyPrincipalManagement.GetACSPrincipalsAsync(context, legacyServicePrincipals, includeSubsites, false, vanityUrlOptions).ConfigureAwait(false);
            return results.Cast<IACSPrincipal>().ToList();
        }

        public List<IACSPrincipal> GetTenantAndSiteCollectionACSPrincipals(List<ILegacyServicePrincipal> legacyServicePrincipals, bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantAndSiteCollectionACSPrincipalsAsync(legacyServicePrincipals, includeSubsites, vanityUrlOptions).GetAwaiter().GetResult();
        }

        public async Task<List<IACSPrincipal>> GetTenantACSPrincipalsAsync(List<ILegacyServicePrincipal> legacyServicePrincipals, VanityUrlOptions vanityUrlOptions = null)
        {
            var results = await LegacyPrincipalManagement.GetACSPrincipalsAsync(context, legacyServicePrincipals, false, true, vanityUrlOptions).ConfigureAwait(false);
            return results.Cast<IACSPrincipal>().ToList();
        }

        public List<IACSPrincipal> GetTenantACSPrincipals(List<ILegacyServicePrincipal> legacyServicePrincipals, VanityUrlOptions vanityUrlOptions = null)
        {
            return GetTenantACSPrincipalsAsync(legacyServicePrincipals, vanityUrlOptions).GetAwaiter().GetResult();
        }


        public async Task<List<ILegacyServicePrincipal>> GetLegacyServicePrincipalsAsync(bool includeExpiredPrincipals = false)
        {
            return await LegacyPrincipalManagement.GetValidLegacyServicePrincipalAppIdsAsync(context, includeExpiredPrincipals).ConfigureAwait(false);
        }

        public List<ILegacyServicePrincipal> GetLegacyServicePrincipals(bool includeExpiredPrincipals = false)
        {
            return GetLegacyServicePrincipalsAsync(includeExpiredPrincipals).GetAwaiter().GetResult();
        }

        public async Task<List<ISharePointAddIn>> GetSiteCollectionSharePointAddInsAsync(bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null, bool loadLegacyPrincipalData = true)
        {
            var results = await LegacyPrincipalManagement.GetSharePointAddInsAsync(context, includeSubsites, vanityUrlOptions, loadLegacyPrincipalData).ConfigureAwait(false);
            return results.Cast<ISharePointAddIn>().ToList();
        }

        public List<ISharePointAddIn> GetSiteCollectionSharePointAddIns(bool includeSubsites = true, VanityUrlOptions vanityUrlOptions = null, bool loadLegacyPrincipalData = true)
        {
            return GetSiteCollectionSharePointAddInsAsync(includeSubsites, vanityUrlOptions, loadLegacyPrincipalData).GetAwaiter().GetResult();
        }
        #endregion
    }
}
