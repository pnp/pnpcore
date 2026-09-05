using PnP.Core.Model.SharePoint;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    /// <summary>
    /// Resolves the server relative url of a site catalog list (theme gallery, master page
    /// gallery, ...) from its list template type.
    /// </summary>
    internal static class CatalogResolver
    {
        internal static async Task<string> GetCatalogUrlAsync(IWeb web, ListTemplateType templateType)
        {
            await web.LoadAsync(w => w.Lists.QueryProperties(
                l => l.TemplateType,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            IList catalog = web.Lists.AsRequested().FirstOrDefault(l => l.TemplateType == templateType);

            return catalog?.RootFolder?.ServerRelativeUrl;
        }
    }
}
