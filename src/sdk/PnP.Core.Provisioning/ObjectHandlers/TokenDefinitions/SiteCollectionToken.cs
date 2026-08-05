using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{sitecollection}",
        Description = "Returns the server relative url of the site collection",
        Example = "{sitecollection}",
        Returns = "/sites/mysitecollection")]
    internal class SiteCollectionToken : VolatileTokenDefinition
    {
        public SiteCollectionToken(PnPContext context)
            : base(context, "{sitecollection}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                // ISite carries ServerRelativeUrl directly, so unlike CSOM there is no need to
                // walk to the root web to get it.
                ISite site = await Context.Site.GetAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);
                CacheValue = site.ServerRelativeUrl.TrimEnd('/');
            }
            return CacheValue;
        }
    }
}