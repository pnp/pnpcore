using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
       Token = "{everyone}",
       Description = "Returns the claim for everyone in this tenant",
       Example = "{everyone}",
       Returns = "c:0(.s|true")]
    internal class EveryoneToken : TokenDefinition
    {

        public EveryoneToken(PnPContext context)
            : base(context, $"{{everyone}}")
        {

        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>("c:0(.s|true");
        }
    }
}
