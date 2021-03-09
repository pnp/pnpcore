using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.QueryIdentities
{
    internal class ObjectPathMethod : Property
    {
        internal MethodParameter Parameters { get; set; }

        public override string ToString()
        {
            string parameters = string.Join("", Parameters.Properties.Select(p => p.SerializeParameter()));
            return $"<Method Id=\"{Id}\" ParentId=\"{ParentId}\" Name=\"{Name}\"><Parameters>{parameters}</Parameters></Method>";
        }
    }
    internal class ConstructorPath : ObjectPathMethod
    {
        internal string TypeId { get; set; }
        public override string ToString()
        {
            string parameters = string.Join("", Parameters.Properties.Select(p => p.SerializeParameter()));
            return $"<Constructor Id=\"{Id}\" TypeId=\"{TypeId}\"><Parameters>{parameters}</Parameters></Constructor>";
        }
    }

    internal class MethodParameter
    {
        internal string TypeId { get; set; }
        internal List<Parameter> Properties { get; set; }
        internal string Value { get; set; }
    }
}
