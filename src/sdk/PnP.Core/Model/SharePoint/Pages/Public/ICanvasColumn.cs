using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A column in a <see cref="ICanvasSection"/>
    /// </summary>
    public interface ICanvasColumn
    {
        /// <summary>
        /// Column order
        /// </summary>
        int Order { get; }

        /// <summary>
        /// <see cref="ICanvasSection"/> this section belongs to
        /// </summary>
        ICanvasSection Section { get; }

        /// <summary>
        /// Column size factor. Max value is 12 (= one column), other options are 8,6,4 or 0
        /// </summary>
        int ColumnFactor { get; }

        /// <summary>
        /// Returns the layout index. Defaults to 1, except for the vertical section column this is 2
        /// </summary>
        int LayoutIndex { get; }

        /// <summary>
        /// List of <see cref="ICanvasControl"/> instances that are hosted in this section
        /// </summary>
        List<ICanvasControl> Controls { get; }

        /// <summary>
        /// Is this a vertical section column?
        /// </summary>
        bool IsVerticalSectionColumn { get; }

        /// <summary>
        /// Color emphasis of the column (used for the vertical section column) 
        /// </summary>
        int? VerticalSectionEmphasis { get; }

        /// <summary>
        /// The Zone Identifier
        /// </summary>
        string ZoneId { get; }

        /// <summary>
        /// Resets the column, used in scenarios where a section is changed from type (e.g. from 3 column to 2 column)
        /// </summary>
        /// <param name="order">Column order to set</param>
        /// <param name="columnFactor">Column factor to set</param>
        void ResetColumn(int order, int columnFactor);
    }
}
