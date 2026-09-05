using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{webname}",
      Description = "Returns the name of the current web (the last segment of its server relative url)",
      Example = "{webname}",
      Returns = "mysite")]
    internal class WebNameToken : VolatileTokenDefinition
    {
        public WebNameToken(PnPContext context) : base(context, "{webname}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
                string url = web.ServerRelativeUrl.TrimEnd('/');
                int lastSeparator = url.LastIndexOf('/');
                CacheValue = lastSeparator == -1 ? string.Empty : url.Substring(lastSeparator + 1);
            }
            return CacheValue;
        }
    }
}