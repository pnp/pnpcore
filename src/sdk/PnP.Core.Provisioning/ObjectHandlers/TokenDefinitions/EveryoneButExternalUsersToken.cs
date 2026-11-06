using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.Security;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
       Token = "{everyonebutexternalusers}",
       Description = "Returns the claim for everyone but external users in this tenant",
       Example = "{everyonebutexternalusers}",
       Returns = "c:0-.f|rolemanager|spo-grid-all-users/b6e37e85-1739-4512-888c-2078dc575169")]
    internal class EveryoneButExternalUsersToken : TokenDefinition
    {
        public EveryoneButExternalUsersToken(PnPContext context)
            : base(context, "{everyonebutexternalusers}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                try
                {
                    ISharePointUser user = await Context.Web.EnsureEveryoneExceptExternalUsersAsync().ConfigureAwait(false);
                    CacheValue = user?.LoginName ?? string.Empty;
                }
                catch (System.Exception)
                {
                    CacheValue = string.Empty;
                }
            }
            return CacheValue;
        }
    }
}