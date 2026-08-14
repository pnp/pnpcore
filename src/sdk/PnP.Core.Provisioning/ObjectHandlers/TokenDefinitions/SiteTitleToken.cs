using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitetitle}",
      Description = "Returns the title of the current site",
      Example = "{sitetitle}",
      Returns = "My Site")]
    [TokenDefinitionDescription(
      Token = "{sitename}",
      Description = "Returns the title of the current site",
      Example = "{sitename}",
      Returns = "My Site")]
    internal class SiteTitleToken : VolatileTokenDefinition
    {
        public SiteTitleToken(PnPContext context) : base(context, "{sitetitle}", "{sitename}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.Title).ConfigureAwait(false);
                CacheValue = web.Title;
            }
            return CacheValue;
        }
    }
}