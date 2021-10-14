namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Value that specifies the type of the list view.
    /// </summary>
    public enum ViewType
    {
        /// <summary>
        /// The type of the list view is not specified
        /// </summary>
        None,

        /// <summary>
        /// HTML Type
        /// </summary>
        HTML,

        /// <summary>
        /// Datasheet list view type
        /// </summary>
        GRID,

        /// <summary>
        /// Calendar list view type
        /// </summary>
        CALENDAR,

        /// <summary>
        /// List view type that displays recurring events
        /// </summary>
        RECURRENCE,

        /// <summary>
        /// Chart list view type
        /// </summary>
        CHART,

        /// <summary>
        /// Gantt chart list view type
        /// </summary>
        GANTT
    }
}
