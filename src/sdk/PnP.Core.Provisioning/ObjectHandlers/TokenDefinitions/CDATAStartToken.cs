using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    internal class CDATAStartToken : TokenDefinition
    {
        public CDATAStartToken(PnPContext context)
            : base(context, "{cdatastart}")
        {
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>("<![CDATA[");
        }
    }
}