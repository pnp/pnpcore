using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectionid}",
      Description = "Returns the id of the current site collection",
      Example = "{sitecollectionid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class SiteCollectionIdToken : VolatileTokenDefinition
    {
        public SiteCollectionIdToken(PnPContext context)
            : base(context, "{sitecollectionid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                ISite site = await Context.Site.GetAsync(s => s.Id).ConfigureAwait(false);
                CacheValue = site.Id.ToString();
            }
            return CacheValue;
        }
    }
}