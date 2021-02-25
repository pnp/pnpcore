using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.QueryAction
{
    class MethodAction : BaseAction
    {
        internal string Name { get; set; }
        internal List<Parameter> Parameters { get; set; }
        public override string ToString()
        {
            List<string> parameters = Parameters.Select(p => $"<Parameter Type=\"{p.Type}\">{p.Value}</Parameter>").ToList();
            string parametersPart = "";
            if (parameters.Count > 0)
            {
                parametersPart = $"<Parameters>{String.Join("", parameters)}</Parameters>";
            }
            return $"<Method Name=\"{Name}\" Id=\"{Id}\" ObjectPathId=\"{ObjectPathId}\">{parametersPart}</Method>";
        }
    }
}
