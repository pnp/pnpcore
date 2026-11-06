using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{site}",
      Description = "Returns the server relative url of the current site",
      Example = "{site}",
      Returns = "/sites/mysitecollection/mysite")]
    internal class SiteToken : VolatileTokenDefinition
    {
        public SiteToken(PnPContext context)
            : base(context, "{site}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
                CacheValue = web.ServerRelativeUrl.TrimEnd('/');
            }
            return CacheValue;
        }
    }
}