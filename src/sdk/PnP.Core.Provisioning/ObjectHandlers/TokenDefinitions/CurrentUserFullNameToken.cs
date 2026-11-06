using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Security;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{currentuserfullname}",
      Description = "Returns the display name of the current user",
      Example = "{currentuserfullname}",
      Returns = "John Doe")]
    internal class CurrentUserFullNameToken : TokenDefinition
    {
        public CurrentUserFullNameToken(PnPContext context)
            : base(context, "{currentuserfullname}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.CurrentUser).ConfigureAwait(false);
                CacheValue = web.CurrentUser.Title;
            }
            return CacheValue;
        }
    }
}
