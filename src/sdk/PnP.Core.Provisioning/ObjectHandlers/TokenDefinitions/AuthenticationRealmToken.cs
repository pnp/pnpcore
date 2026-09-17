using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{authenticationrealm}",
      Description = "Returns the authentication realm of the current tenant",
      Example = "{authenticationrealm}",
      Returns = "d1a6d3a0-1e33-4c9f-9a4d-7bd8a6a1a1a1")]
    [TokenDefinitionDescription(
      Token = "{realm}",
      Description = "Returns the authentication realm of the current tenant",
      Example = "{realm}",
      Returns = "d1a6d3a0-1e33-4c9f-9a4d-7bd8a6a1a1a1")]
    internal class AuthenticationRealmToken : TokenDefinition
    {
        public AuthenticationRealmToken(PnPContext context)
            : base(context, "{authenticationrealm}", "{realm}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = (await Context.GetTenantIdAsync().ConfigureAwait(false)).ToString();
            }
            return CacheValue;
        }
    }
}
