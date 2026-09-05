using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{hosturl}",
      Description = "Returns the host url of the current tenant",
      Example = "{hosturl}",
      Returns = "https://contoso.sharepoint.com")]
    public class HostUrlToken : TokenDefinition
    {
        public HostUrlToken(PnPContext context) : base(context, "{hosturl}")
        {
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = $"{Context.Uri.Scheme}://{Context.Uri.DnsSafeHost.ToLowerInvariant().Replace("-admin", "")}";
            }
            return Task.FromResult(CacheValue);
        }
    }
}