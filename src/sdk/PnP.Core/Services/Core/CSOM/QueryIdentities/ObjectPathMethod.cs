using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    internal class ObjectPathMethod : Property
    {
        internal List<MethodParameter> Parameters { get; set; }
    }

    internal class MethodParameter
    {
        internal string TypeId { get; set; }
        internal List<Parameter> Properties { get; set; }
        internal string Value { get; set; }
    }

    internal class StaticMethod : Identity
    {
        internal string TypeId { get; set; }
    }
}
