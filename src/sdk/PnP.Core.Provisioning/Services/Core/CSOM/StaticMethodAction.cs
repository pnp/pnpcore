using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Services.Core.CSOM
{
    /// <summary>
    /// Invokes a void-returning static CSOM method as an action.
    /// </summary>
    internal sealed class StaticMethodAction : BaseAction
    {
        /// <summary>
        /// The method name.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The declaring type's CSOM server type id - see <see cref="CsomTypeIds"/>.
        /// </summary>
        internal string TypeId { get; set; }

        /// <summary>
        /// The method's parameters, excluding the implicit <c>ClientRuntimeContext</c>.
        /// </summary>
        internal List<Parameter> Parameters { get; set; }

        public override string ToString()
        {
            string parametersPart = string.Empty;

            if (Parameters != null && Parameters.Count > 0)
            {
                parametersPart = $"<Parameters>{string.Join("", Parameters.Select(p => p.SerializeParameter()))}</Parameters>";
            }

            return $"<StaticMethod Id=\"{Id}\" Name=\"{Name}\" TypeId=\"{TypeId}\">{parametersPart}</StaticMethod>";
        }
    }
}
