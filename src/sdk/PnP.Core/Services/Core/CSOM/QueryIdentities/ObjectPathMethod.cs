using PnP.Core.Services.Core.CSOM.QueryAction;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    class ObjectPathMethod : Property
    {
        internal List<MethodParameter> Parameters { get; set; }
    }
    class MethodParameter
    {
        internal string TypeId { get; set; }
        internal List<Parameter> Properties { get; set; }
        internal string Value { get; set; }
    }
    class StaticMethod : Identity
    {
        internal string TypeId { get; set; }
    }
}
