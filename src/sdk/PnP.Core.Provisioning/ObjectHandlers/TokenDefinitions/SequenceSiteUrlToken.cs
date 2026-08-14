using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
    Token = "{sequencesiteurl:[provisioningid]}",
    Description = "Returns a full url of the site given its provisioning ID from the sequence",
    Example = "{sequencesiteurl:MYID}",
    Returns = "https://contoso.sharepoint.com/sites/mynewsite")]
    internal class SequenceSiteUrlUrlToken : TokenDefinition
    {
        private readonly string _url = null;
        public SequenceSiteUrlUrlToken(PnPContext context, string provisioningId, string url)
            : base(context, $"{{sequencesiteurl:{provisioningId}}}")
        {
            _url = url;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _url;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}
