using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{themecatalog}",
      Description = "Returns the server relative url of the theme catalog",
      Example = "{themecatalog}",
      Returns = "/sites/mysitecollection/_catalogs/theme")]
    internal class ThemeCatalogToken : TokenDefinition
    {
        public ThemeCatalogToken(PnPContext context)
            : base(context, "{themecatalog}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                // The theme catalog only exists on the root web, which is why CSOM walked to
                // Site.RootWeb before asking for it.
                CacheValue = await CatalogResolver.GetCatalogUrlAsync(
                    Context.Site.RootWeb, ListTemplateType.ThemeCatalog).ConfigureAwait(false);
            }
            return CacheValue;
        }
    }
}