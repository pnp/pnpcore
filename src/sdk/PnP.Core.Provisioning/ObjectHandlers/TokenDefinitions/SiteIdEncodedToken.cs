using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{siteidencoded}",
      Description = "Returns the id of the current site, with dashes percent-encoded",
      Example = "{siteidencoded}",
      Returns = "f2cd6d5b%2D1391%2D480e%2Da3dc%2D7f7f96137382")]
    internal class SiteIdEncodedToken : VolatileTokenDefinition
    {
        public SiteIdEncodedToken(PnPContext context)
            : base(context, "{siteidencoded}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.Id).ConfigureAwait(false);
                CacheValue = web.Id.ToString().Replace("-", "%2D");
            }
            return CacheValue;
        }
    }
}