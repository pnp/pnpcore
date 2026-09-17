using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{currentuserid}",
      Description = "Returns the id of the current user",
      Example = "{currentuserid}",
      Returns = "12")]
    internal class CurrentUserIdToken : VolatileTokenDefinition
    {
        public CurrentUserIdToken(PnPContext context)
            : base(context, "{currentuserid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.CurrentUser).ConfigureAwait(false);
                CacheValue = web.CurrentUser.Id.ToString();
            }
            return CacheValue;
        }
    }
}