namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of check-in for a file.
    /// (see https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee542953(v=office.15) )
    /// </summary>
    public enum CheckinType
    {
        /// <summary>
        /// Enumeration whose values are incremented as minor version. The value = 0.
        /// </summary>
        MinorCheckIn = 0,
        /// <summary>
        ///  Enumeration whose values are incremented as a major version. The value = 1.
        /// </summary>
        MajorCheckIn = 1,
        /// <summary>
        /// Enumeration whose values overwrite the file. The value = 2.
        /// </summary>
        OverwriteCheckIn = 2
    }
}
