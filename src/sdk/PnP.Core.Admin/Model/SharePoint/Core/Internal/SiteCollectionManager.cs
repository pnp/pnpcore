using PnP.Core.Services;
using System;
using System.Collections.Generic;
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

        public async Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default)
        {
            return await SiteCollectionEnumerator.GetAsync(context, ignoreUserIsSharePointAdmin, filter).ConfigureAwait(false);
        }

        public List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false, SiteCollectionFilter filter = SiteCollectionFilter.Default)
        {
            return GetSiteCollectionsAsync(ignoreUserIsSharePointAdmin, filter).GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync()
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails()
        {
            return GetSiteCollectionsWithDetailsAsync().GetAwaiter().GetResult();
        }

        public async Task<ISiteCollectionWithDetails> GetSiteCollectionWithDetailsAsync(Uri url)
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context, url).ConfigureAwait(false);
        }

        public ISiteCollectionWithDetails GetSiteCollectionWithDetails(Uri url)
        {
            return GetSiteCollectionWithDetailsAsync(url).GetAwaiter().GetResult();
        }

        public async Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync()
        {
            return await SiteCollectionEnumerator.GetRecycledWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<IRecycledSiteCollection> GetRecycledSiteCollections()
        {
            return GetRecycledSiteCollectionsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<IWebWithDetails>> GetSiteCollectionWebsWithDetailsAsync(Uri url = null, bool skipAppWebs = true)
        {
            return await WebEnumerator.GetWithDetailsAsync(context, url, skipAppWebs).ConfigureAwait(false);
        }

        public List<IWebWithDetails> GetSiteCollectionWebsWithDetails(Uri url = null, bool skipAppWebs = true)
        {
            return GetSiteCollectionWebsWithDetailsAsync(url, skipAppWebs).GetAwaiter().GetResult();
        }

        public async Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            if (siteToCreate == null)
            {
                throw new ArgumentNullException(nameof(siteToCreate));
            }

            return await SiteCollectionCreator.CreateSiteCollectionAsync(context, siteToCreate, creationOptions).ConfigureAwait(false);
        }

        public PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            return CreateSiteCollectionAsync(siteToCreate, creationOptions).GetAwaiter().GetResult();
        }

        public async Task RecycleSiteCollectionAsync(Uri siteToDelete)
        {
            if (siteToDelete == null)
            {
                throw new ArgumentNullException(nameof(siteToDelete));
            }
            
            await SiteCollectionManagement.RecycleSiteCollectionAsync(context, siteToDelete).ConfigureAwait(false);
        }

        public void RecycleSiteCollection(Uri siteToDelete)
        {
            RecycleSiteCollectionAsync(siteToDelete).GetAwaiter().GetResult();
        }

        public void RestoreSiteCollection(Uri siteToRestore)
        {
            RestoreSiteCollectionAsync(siteToRestore).GetAwaiter().GetResult();
        }

        public async Task RestoreSiteCollectionAsync(Uri siteToRestore)
        {
            if (siteToRestore == null)
            {
                throw new ArgumentNullException(nameof(siteToRestore));
            }

            await SiteCollectionManagement.RestoreSiteCollectionAsync(context, siteToRestore).ConfigureAwait(false);
        }

        public async Task DeleteSiteCollectionAsync(Uri siteToDelete)
        {
            if (siteToDelete == null)
            {
                throw new ArgumentNullException(nameof(siteToDelete));
            }

            await SiteCollectionManagement.DeleteSiteCollectionAsync(context, siteToDelete).ConfigureAwait(false);
        }

        public void DeleteSiteCollection(Uri siteToDelete)
        {
            DeleteSiteCollectionAsync(siteToDelete).GetAwaiter().GetResult();
        }

        public async Task<ISiteCollectionProperties> GetSiteCollectionPropertiesAsync(Uri site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return await SiteCollectionManagement.GetSiteCollectionPropertiesByUrlAsync(context, site, true).ConfigureAwait(false);
        }

        public ISiteCollectionProperties GetSiteCollectionProperties(Uri site)
        {
            return GetSiteCollectionPropertiesAsync(site).GetAwaiter().GetResult();
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

        public async Task<List<ISiteCollectionAdmin>> GetSiteCollectionAdminsAsync(Uri site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return await SiteCollectionManagement.GetSiteCollectionAdminsAsync(context, site).ConfigureAwait(false);
        }

        public List<ISiteCollectionAdmin> GetSiteCollectionAdmins(Uri site)
        {
            return GetSiteCollectionAdminsAsync(site).GetAwaiter().GetResult();
        }

        public async Task SetSiteCollectionAdminsAsync(Uri site, List<string> sharePointAdminLoginNames = null, List<Guid> ownerGroupAzureAdUserIds = null)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            await SiteCollectionManagement.SetSiteCollectionAdminsAsync(context, site, sharePointAdminLoginNames, ownerGroupAzureAdUserIds).ConfigureAwait(false);
        }

        public void SetSiteCollectionAdmins(Uri site, List<string> sharePointAdminLoginNames = null, List<Guid> ownerGroupAzureAdUserIds = null)
        {
            SetSiteCollectionAdminsAsync(site, sharePointAdminLoginNames, ownerGroupAzureAdUserIds).GetAwaiter().GetResult();
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
    }
}
