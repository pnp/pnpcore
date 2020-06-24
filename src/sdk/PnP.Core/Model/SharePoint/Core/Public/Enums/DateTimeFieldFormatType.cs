namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the display format for date and time fields.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee537367%28v%3doffice.15%29
    /// </summary>
    public enum DateTimeFieldFormatType
    {
        /// <summary>
        /// Displays only the date.
        /// </summary>
        DateOnly = 0,
        /// <summary>
        /// Displays the date and time.
        /// </summary>
        DateTime = 1
    }
}
