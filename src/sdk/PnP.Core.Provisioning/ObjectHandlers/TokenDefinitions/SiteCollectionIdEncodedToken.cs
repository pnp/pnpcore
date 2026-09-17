using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectionidencoded}",
      Description = "Returns the id of the current site collection, with dashes percent-encoded",
      Example = "{sitecollectionidencoded}",
      Returns = "f2cd6d5b%2D1391%2D480e%2Da3dc%2D7f7f96137382")]
    internal class SiteCollectionIdEncodedToken : VolatileTokenDefinition
    {
        public SiteCollectionIdEncodedToken(PnPContext context)
            : base(context, "{sitecollectionidencoded}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                ISite site = await Context.Site.GetAsync(s => s.Id).ConfigureAwait(false);
                CacheValue = site.Id.ToString().Replace("-", "%2D");
            }
            return CacheValue;
        }
    }
}