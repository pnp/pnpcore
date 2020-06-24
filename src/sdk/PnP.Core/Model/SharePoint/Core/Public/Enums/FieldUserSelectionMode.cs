namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies whether users and groups or only users can be selected.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee545809%28v%3doffice.15%29
    /// </summary>
    public enum FieldUserSelectionMode
    {
        /// <summary>
        /// Enumeration whose value specifies that only users can be selected. The value = 0.
        /// </summary>
        PeopleOnly = 0,
        /// <summary>
        /// Enumeration whose value specifies that users and groups can be selected. The value = 1.
        /// </summary>
        PeopleAndGroups = 1
    }
}
