using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    internal class CDATAEndToken : TokenDefinition
    {
        public CDATAEndToken(PnPContext context)
            : base(context, "{cdataend}")
        {
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>("]]>");
        }
    }
}