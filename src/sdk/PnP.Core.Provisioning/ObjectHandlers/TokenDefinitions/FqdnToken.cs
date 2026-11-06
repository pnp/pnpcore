using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{fqdn}",
      Description = "Returns the fully qualified domain name of the current tenant",
      Example = "{fqdn}",
      Returns = "contoso.sharepoint.com")]
    public class FqdnToken : TokenDefinition
    {
        public FqdnToken(PnPContext context) : base(context, "{fqdn}")
        {
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = Context.Uri.DnsSafeHost.ToLowerInvariant().Replace("-admin", "");
            }
            return Task.FromResult(CacheValue);
        }
    }
}