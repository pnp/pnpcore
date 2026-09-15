using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{fileuniqueidencoded:[siteRelativePath]}",
     Description = "Returns the html safe encoded unique id of a file which is being provisioned by the current template.",
     Example = "{fileuniqueidencoded:sitepages/home.aspx}",
     Returns = "f2cd6d5b%2D1391%2D480e%2Da3dc%2D7f7f96137382")]
    internal class FileUniqueIdEncodedToken : TokenDefinition
    {
        private readonly string _value = null;
        public FileUniqueIdEncodedToken(PnPContext context, string siteRelativePath, Guid uniqueId)
            : base(context, $"{{fileuniqueidencoded:{Regex.Escape(siteRelativePath)}}}")
        {
            _value = uniqueId.ToString().Replace("-", "%2D");
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

