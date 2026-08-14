using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{filelistitemid:[siteRelativePath]}",
     Description = "Returns the listitem id of a file which is being provisioned by the current template.",
     Example = "{filelistitemid:/library/folder/file.docx}",
     Returns = "54")]
    internal class FileListItemIdToken : TokenDefinition
    {
        private readonly string _value = null;
        public FileListItemIdToken(PnPContext context, string siteRelativePath, int id)
            : base(context, $"{{filelistitemid:{Regex.Escape(siteRelativePath)}}}")
        {
            _value = id.ToString();
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

