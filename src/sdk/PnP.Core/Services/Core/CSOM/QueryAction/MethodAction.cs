using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    internal class MethodAction : BaseAction
    {
        internal string Name { get; set; }
        internal List<Parameter> Parameters { get; set; }

        public override string ToString()
        {
            string parametersPart = "";
            if (Parameters != null)
            {
                List<string> parameters = Parameters.Select(p => p.SerializeParameter()).ToList();
                if (parameters.Count > 0)
                {
                    parametersPart = $"<Parameters>{string.Join("", parameters)}</Parameters>";
                }
            }
            return $"<Method Name=\"{Name}\" Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\">{parametersPart}</Method>";
        }
    }
}
