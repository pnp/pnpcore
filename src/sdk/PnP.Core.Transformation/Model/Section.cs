using PnP.Core.Model.SharePoint;
using System.Collections.Generic;

namespace PnP.Core.Transformation.Model
{
    /// <summary>
    /// Defines an abstraction for a target section of a modern page
    /// </summary>
    public class Section
    {
        /// <summary>
        /// The canvas template for the sections to render in the target modern page
        /// </summary>
        public CanvasSectionTemplate CanvasTemplate { get; set; }

        /// <summary>
        /// Defines the order of the section in the target page
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The columns in the current section
        /// </summary>
        public List<Column> Columns { get; set; } = new List<Column>();

        /// <summary>
        /// The emphasis for the zone
        /// </summary>
        public int ZoneEmphasis { get; set; }

        /// <summary>
        /// The emphasis for the vertical zone
        /// </summary>
        public int VerticalSectionZoneEmphasis { get; set; }
    }
}
