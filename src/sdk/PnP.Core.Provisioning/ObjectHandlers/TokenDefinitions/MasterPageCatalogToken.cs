using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{masterpagecatalog}",
      Description = "Returns the server relative url of the master page catalog",
      Example = "{masterpagecatalog}",
      Returns = "/sites/mysitecollection/_catalogs/masterpage")]
    internal class MasterPageCatalogToken : TokenDefinition
    {
        public MasterPageCatalogToken(PnPContext context)
            : base(context, "{masterpagecatalog}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
                ISite site = await Context.Site.GetAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                bool isSubSite = !web.ServerRelativeUrl.TrimEnd('/')
                    .Equals(site.ServerRelativeUrl.TrimEnd('/'), System.StringComparison.OrdinalIgnoreCase);

                IWeb catalogWeb = isSubSite ? Context.Site.RootWeb : Context.Web;
                CacheValue = await CatalogResolver.GetCatalogUrlAsync(
                    catalogWeb, ListTemplateType.MasterPageCatalog).ConfigureAwait(false);
            }
            return CacheValue;
        }
    }
}
