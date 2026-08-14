using PnP.Core.Provisioning.Attributes;
using System;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{fileuniqueid:[siteRelativePath]}",
     Description = "Returns the unique id of a file which is being provisioned by the current template.",
     Example = "{fileuniqueid:sitepages/home.aspx}",
     Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class FileUniqueIdToken : TokenDefinition
    {
        private readonly string _value = null;
        public FileUniqueIdToken(PnPContext context, string siteRelativePath, Guid uniqueId)
            : base(context, $"{{fileuniqueid:{Regex.Escape(siteRelativePath)}}}")
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

