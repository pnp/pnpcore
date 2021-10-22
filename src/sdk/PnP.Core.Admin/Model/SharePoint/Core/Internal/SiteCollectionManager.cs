using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SiteCollectionManager : ISiteCollectionManager
    {
        private readonly PnPContext context;

        internal SiteCollectionManager(PnPContext pnpContext)
        {
            context = pnpContext;
        }

        public async Task<List<ISiteCollection>> GetSiteCollectionsAsync(bool ignoreUserIsSharePointAdmin = false)
        {
            return await SiteCollectionEnumerator.GetAsync(context, ignoreUserIsSharePointAdmin).ConfigureAwait(false);
        }

        public List<ISiteCollection> GetSiteCollections(bool ignoreUserIsSharePointAdmin = false)
        {
            return GetSiteCollectionsAsync(ignoreUserIsSharePointAdmin).GetAwaiter().GetResult();
        }

        public async Task<List<ISiteCollectionWithDetails>> GetSiteCollectionsWithDetailsAsync()
        {
            return await SiteCollectionEnumerator.GetWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<ISiteCollectionWithDetails> GetSiteCollectionsWithDetails()
        {
            return GetSiteCollectionsWithDetailsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<IRecycledSiteCollection>> GetRecycledSiteCollectionsAsync()
        {
            return await SiteCollectionEnumerator.GetRecycledWithDetailsViaTenantAdminHiddenListAsync(context).ConfigureAwait(false);
        }

        public List<IRecycledSiteCollection> GetRecycledSiteCollections()
        {
            return GetRecycledSiteCollectionsAsync().GetAwaiter().GetResult();
        }

        public async Task<PnPContext> CreateSiteCollectionAsync(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            return await SiteCollectionCreator.CreateSiteCollectionAsync(context, siteToCreate, creationOptions).ConfigureAwait(false);
        }

        public PnPContext CreateSiteCollection(CommonSiteOptions siteToCreate, SiteCreationOptions creationOptions = null)
        {
            return CreateSiteCollectionAsync(siteToCreate, creationOptions).GetAwaiter().GetResult();
        }

        public async Task RecycleSiteCollectionAsync(Uri siteToDelete, string webTemplate)
        {
            await SiteCollectionManagement.RecycleSiteCollectionAsync(context, siteToDelete, webTemplate).ConfigureAwait(false);
        }

        public void RecycleSiteCollection(Uri siteToDelete, string webTemplate)
        {
            RecycleSiteCollectionAsync(siteToDelete, webTemplate).GetAwaiter().GetResult();
        }

        public void RestoreSiteCollection(Uri siteToRestore)
        {
            RestoreSiteCollectionAsync(siteToRestore).GetAwaiter().GetResult();
        }

        public async Task RestoreSiteCollectionAsync(Uri siteToRestore)
        {
            await SiteCollectionManagement.RestoreSiteCollectionAsync(context, siteToRestore).ConfigureAwait(false);
        }

        public async Task DeleteSiteCollectionAsync(Uri siteToDelete, string webTemplate)
        {
            await SiteCollectionManagement.DeleteSiteCollectionAsync(context, siteToDelete, webTemplate).ConfigureAwait(false);
        }

        public void DeleteSiteCollection(Uri siteToDelete, string webTemplate)
        {
            DeleteSiteCollectionAsync(siteToDelete, webTemplate).GetAwaiter().GetResult();
        }

        public async Task<ISiteCollectionProperties> GetSiteCollectionPropertiesAsync(Uri site)
        {
            return await SiteCollectionManagement.GetSiteCollectionPropertiesByUrlAsync(context, site, true).ConfigureAwait(true);
        }

        public ISiteCollectionProperties GetSiteCollectionProperties(Uri site)
        {
            return GetSiteCollectionPropertiesAsync(site).GetAwaiter().GetResult();
        }
    }
}
