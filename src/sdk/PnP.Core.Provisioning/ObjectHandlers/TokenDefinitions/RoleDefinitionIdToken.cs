using PnP.Core.Provisioning.Attributes;
using System.Text.RegularExpressions;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
        Token = "{roledefinitionid:[rolename]}",
        Description = "Returns the id of the given role definition name",
        Example = "{roledefinitionid:My Role Definition}",
        Returns = "23")]
    internal class RoleDefinitionIdToken : TokenDefinition
    {
        private readonly int _roleDefinitionId = 0;
        public RoleDefinitionIdToken(PnPContext context, string name, int roleDefinitionId)
            : base(context, $"{{roledefinitionid:{Regex.Escape(name)}}}")
        {
            _roleDefinitionId = roleDefinitionId;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _roleDefinitionId.ToString();
            }
            return Task.FromResult<string>(CacheValue);
        }
    }
}
