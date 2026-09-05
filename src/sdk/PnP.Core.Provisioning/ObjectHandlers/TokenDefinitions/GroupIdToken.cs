using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{groupid:[groupname]}",
     Description = "Returns the id of a SharePoint group given its name",
     Example = "{groupid:My Site Owners}",
     Returns = "6")]
    internal class GroupIdToken : TokenDefinition
    {
        private readonly string _groupId = string.Empty;
        public GroupIdToken(PnPContext context, string name, string groupId)
            : base(context, $"{{groupid:{Regex.Escape(name)}}}")
        {
            _groupId = groupId;
        }

        public GroupIdToken(PnPContext context, string name, int groupId)
            : base(context, $"{{groupid:{Regex.Escape(name)}}}")
        {
            _groupId = groupId.ToString();
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