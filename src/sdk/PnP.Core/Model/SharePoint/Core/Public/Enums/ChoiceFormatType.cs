namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enumeration that specifies how options are displayed for the selections in a choice field.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee536424%28v%3doffice.15%29
    /// </summary>
    public enum ChoiceFormatType
    {
        /// <summary>
        /// A drop-down list box.
        /// </summary>
        Dropdown = 0,
        /// <summary>
        /// Option buttons (also known as radio buttons).
        /// </summary>
        RadioButtons = 1
    }
}
