using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
     Token = "{guid}",
     Description = "Returns a newly generated GUID",
     Example = "{guid}",
     Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class GuidToken : TokenDefinition
    {
        public GuidToken(PnPContext context)
            : base(context, "{guid}")
        {
            // Every use must produce a different value, so this can never be cached.
            IsCacheable = false;
        }

        // Overriding the synchronous path as well keeps the non-cacheable parse path free of
        // any async machinery - this token needs no I/O at all.
        public override string GetReplaceValue()
        {
            return Guid.NewGuid().ToString();
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult(GetReplaceValue());
        }
    }
}