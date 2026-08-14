using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
   Token = "{pageuniqueid:[siterelativepath]}",
   Description = "Returns the id of a client side page that is being provisioned through the current template",
   Example = "{pageuniqueid:SitePages/Home.aspx}",
   Returns = "767bc144-e605-4d8c-885a-3a980feb39c6")]
    internal class PageUniqueIdToken : TokenDefinition
    {
        private readonly string _value = null;
        public PageUniqueIdToken(PnPContext context, string siteRelativePath, Guid uniqueId)
            : base(context, $"{{pageuniqueid:{Regex.Escape(siteRelativePath)}}}")
        {
            _value = uniqueId.ToString();
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _value;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}

