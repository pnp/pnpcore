using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Provisioning.Services.Core.CSOM
{
    /// <summary>
    /// An array parameter whose elements are references to other object paths in the same request.
    /// </summary>
    internal sealed class ObjectArrayParameter : Parameter
    {
        /// <summary>
        /// The object path ids of the array's elements, in order.
        /// </summary>
        internal List<int> ObjectPathIds { get; set; } = new List<int>();

        internal override string SerializeParameter()
        {
            string elements = string.Join("", ObjectPathIds.Select(id => $"<Object ObjectPathId=\"{id}\" />"));
            return $"<Parameter Type=\"Array\">{elements}</Parameter>";
        }
    }
}
