using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Value that specifies the type of the list view.
    /// </summary>
    public enum ViewTypeKind
    {
        /// <summary>
        /// The type of the list view is not specified
        /// </summary>
        None = 0,

        /// <summary>
        /// HTML Type
        /// </summary>
        Html = 1,

        /// <summary>
        /// Datasheet list view type
        /// </summary>
        Grid = 2048,

        /// <summary>
        /// Calendar list view type
        /// </summary>
        Calendar = 524288,
        
        /// <summary>
        /// List view type that displays recurring events
        /// </summary>
        Recurrence = 8193,

        /// <summary>
        /// Chart list view type
        /// </summary>
        Chart = 131072,

        /// <summary>
        /// Gantt chart list view type
        /// </summary>
        Gantt = 67108864
    }
}
