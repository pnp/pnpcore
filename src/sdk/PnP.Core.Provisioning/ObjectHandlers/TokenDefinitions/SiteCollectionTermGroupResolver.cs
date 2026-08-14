using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    /// <summary>
    /// Finds the term group scoped to the current site collection, which backs both
    /// <c>{sitecollectiontermgroupid}</c> and <c>{sitecollectiontermgroupname}</c>.
    /// </summary>
    internal static class SiteCollectionTermGroupResolver
    {
        internal static async Task<ITermGroup> GetAsync(PnPContext context)
        {
            try
            {
                ISite site = await context.Site.GetAsync(s => s.Id).ConfigureAwait(false);

                await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(
                    g => g.Id, g => g.Name, g => g.Scope)).ConfigureAwait(false);

                string siteId = site.Id.ToString();

                return context.TermStore.Groups.AsRequested().FirstOrDefault(g =>
                    g.Scope == TermGroupScope.SiteCollection
                    && !string.IsNullOrEmpty(g.Id)
                    && g.Id.EndsWith(siteId, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
