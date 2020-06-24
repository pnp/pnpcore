namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies a friendly format to use in displaying date and time fields.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/jj168261%28v%3doffice.15%29
    /// </summary>
    public enum DateTimeFieldFriendlyFormatType
    {
        /// <summary>
        /// Undefined. The default rendering will be used. Value = 0.
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// The standard absolute representation will be used. Value = 1.
        /// </summary>
        Disabled = 1,
        /// <summary>
        /// The standard friendly relative representation will be used (for example, "today at 3:00 PM"). Value = 2.
        /// </summary>
        Relative = 2
    }
}
