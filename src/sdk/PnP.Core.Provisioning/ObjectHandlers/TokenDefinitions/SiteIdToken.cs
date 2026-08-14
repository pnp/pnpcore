using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{siteid}",
      Description = "Returns the id of the current site",
      Example = "{siteid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class SiteIdToken : VolatileTokenDefinition
    {
        public SiteIdToken(PnPContext context)
            : base(context, "{siteid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.Id).ConfigureAwait(false);
                CacheValue = web.Id.ToString();
            }
            return CacheValue;
        }
    }
}