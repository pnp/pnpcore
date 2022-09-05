using System.Collections.Generic;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Defines an abstraction for a Column in a Section of a target modern page
    /// </summary>
    public class Column
    {
        /// <summary>
        /// The list of Canvas Controls controls in the column
        /// </summary>
        public List<CanvasControl> Controls { get; } = new List<CanvasControl>();
    }
}
