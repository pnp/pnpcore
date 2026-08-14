using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{o365groupid:[groupname]}",
     Description = "Returns the id of an Office 365 Group",
     Example = "{o365groupid:CompanyManagement}",
     Returns = "6")]
    internal class O365GroupIdToken : TokenDefinition
    {
        private readonly string _groupId = string.Empty;
        public O365GroupIdToken(PnPContext context, string name, string groupId)
            : base(context, $"{{o365groupid:{Regex.Escape(name)}}}")
        {
            _groupId = groupId;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _groupId;
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}