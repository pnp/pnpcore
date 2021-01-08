namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents status to determine whether filters pane will show the field
    /// </summary>
    public enum ShowInFiltersPaneStatus
    {
        /// <summary>
        /// field will be auto determined to show/hide in filters pane based on the list data
        /// </summary>
        Auto,

        /// <summary>
        /// field will always show in Filters pane
        /// </summary>
        Pinned,

        /// <summary>
        /// field will never show in filters pane
        /// </summary>
        Removed
    }
}
