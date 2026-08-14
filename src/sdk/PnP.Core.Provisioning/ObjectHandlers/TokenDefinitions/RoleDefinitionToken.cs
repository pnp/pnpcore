using PnP.Core.Model.Security;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{roledefinition:[roletype]}",
      Description = "Returns the name of a role definition given its role type",
      Example = "{roledefinition:Editor}",
      Returns = "Edit")]
    internal class RoleDefinitionToken : TokenDefinition
    {
        private readonly string _name;

        public RoleDefinitionToken(PnPContext context, IRoleDefinition definition)
            : base(context, $"{{roledefinition:{definition.RoleTypeKind}}}")
        {
            _name = definition.Name;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                CacheValue = _name;
            }
            return Task.FromResult(CacheValue);
        }
    }
}