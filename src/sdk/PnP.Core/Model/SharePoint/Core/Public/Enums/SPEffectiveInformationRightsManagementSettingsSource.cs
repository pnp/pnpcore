namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// The source of settings for the effective IRM of a file.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/mt684131(v=office.15)
    /// </summary>
    public enum SPEffectiveInformationRightsManagementSettingsSource
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// File
        /// </summary>
        File,
        /// <summary>
        /// List
        /// </summary>
        List,
        /// <summary>
        /// Rule
        /// </summary>
        Rule
    }
}
