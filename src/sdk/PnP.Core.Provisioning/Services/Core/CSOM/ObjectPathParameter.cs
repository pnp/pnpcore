using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Globalization;

namespace PnP.Core.Provisioning.Services.Core.CSOM
{
    /// <summary>
    /// A parameter that refers to another object path in the same request, rather than carrying a
    /// literal value.
    /// </summary>
    internal sealed class ObjectPathParameter : Parameter
    {
        /// <summary>
        /// The id of the object path this parameter refers to.
        /// </summary>
        internal int ReferencedObjectPathId { get; set; }

        internal override string SerializeParameter()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "<{0} ObjectPathId=\"{1}\" />", ParameterTagName, ReferencedObjectPathId);
        }
    }
}
