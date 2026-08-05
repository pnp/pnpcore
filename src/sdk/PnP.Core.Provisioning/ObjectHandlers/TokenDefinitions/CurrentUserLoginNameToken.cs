using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{currentuserloginname}",
      Description = "Returns the login name of the current user",
      Example = "{currentuserloginname}",
      Returns = "i:0#.f|membership|user@contoso.onmicrosoft.com")]
    internal class CurrentUserLoginNameToken : TokenDefinition
    {
        public CurrentUserLoginNameToken(PnPContext context)
            : base(context, "{currentuserloginname}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.CurrentUser).ConfigureAwait(false);
                CacheValue = web.CurrentUser.LoginName;
            }
            return CacheValue;
        }
    }
}