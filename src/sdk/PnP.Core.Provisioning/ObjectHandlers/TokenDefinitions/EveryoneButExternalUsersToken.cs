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
                    // PnP Core already implements the whole of what this token did by hand:
                    // the modern rolemanager claim first, falling back to the language specific
                    // display name on tenants too old to have it. Its fallback table is larger
                    // than PnP Framework's, so this is a strict improvement.
                    ISharePointUser user = await Context.Web.EnsureEveryoneExceptExternalUsersAsync().ConfigureAwait(false);
                    CacheValue = user?.LoginName ?? string.Empty;
                }
                catch (System.Exception)
                {
                    // Matches PnP Framework: an unresolvable claim yields an empty token rather
                    // than failing the whole provisioning run.
                    CacheValue = string.Empty;
                }
            }
            return CacheValue;
        }
    }
}