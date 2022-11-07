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
            if (Parameters != null && Parameters.Properties != null && Parameters.Properties.Count > 0)
            {
                string parameters = string.Join("", Parameters.Properties.Select(p => p.SerializeParameter()));
                return $"<Method Id=\"{Id}\" ParentId=\"{ParentId}\" Name=\"{Name}\"><Parameters>{parameters}</Parameters></Method>";
            }
            else
            {
                return $"<Method Id=\"{Id}\" ParentId=\"{ParentId}\" Name=\"{Name}\" />";
            }
        }
    }

    internal class StaticMethodPath : ObjectPathMethod
    {
        internal string TypeId { get; set; }

        public override string ToString()
        {
            string parameters = string.Join("", Parameters.Properties.Select(p => p.SerializeParameter()));
            return $"<StaticMethod Id=\"{Id}\" TypeId=\"{TypeId}\" Name=\"{Name}\"><Parameters>{parameters}</Parameters></StaticMethod>";
        }
    }
    
    internal sealed class ConstructorPath : ObjectPathMethod
    {
        internal string TypeId { get; set; }

        public override string ToString()
        {
            if (Parameters != null && Parameters.Properties != null && Parameters.Properties.Count > 0)
            {
                string parameters = string.Join("", Parameters.Properties.Select(p => p.SerializeParameter()));
                return $"<Constructor Id=\"{Id}\" TypeId=\"{TypeId}\"><Parameters>{parameters}</Parameters></Constructor>";
            }
            else
            {
                return $"<Constructor Id=\"{Id}\" TypeId=\"{TypeId}\" />";
            }
        }
    }

    internal sealed class MethodParameter
    {
        internal string TypeId { get; set; }

        internal List<Parameter> Properties { get; set; }

        internal string Value { get; set; }
    }
}
