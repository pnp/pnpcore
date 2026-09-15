using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{listurl:[name]}",
     Description = "Returns a site relative url of the list given its name",
     Example = "{listid:My List}",
     Returns = "Lists/MyList")]
    internal class ListUrlToken : TokenDefinition
    {
        private readonly string _listUrl = null;
        public ListUrlToken(PnPContext context, string name, string url)
            : base(context, $"{{listurl:{Regex.Escape(name)}}}")
        {
            _listUrl = url;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _listUrl;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}