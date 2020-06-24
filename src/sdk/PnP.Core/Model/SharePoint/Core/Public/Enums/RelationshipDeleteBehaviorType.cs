namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the optional relationship behavior of a relationship lookup field.
    /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/ee537368%28v%3doffice.15%29
    /// </summary>
    public enum RelationshipDeleteBehaviorType
    {
        /// <summary>
        /// Enumeration whose value specifies that no relationship behavior is applied. . Value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enumeration whose value specifies the cascade behavior. Value = 1.
        /// </summary>
        Cascade = 1,
        /// <summary>
        /// Enumeration whose value specifies the restrict behavior. Value = 2.
        /// </summary>
        Restrict = 2
    }
}
