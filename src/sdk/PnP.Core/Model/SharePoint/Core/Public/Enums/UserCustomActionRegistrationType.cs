namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of object associated with the custom action.
    /// </summary>
    public enum UserCustomActionRegistrationType
    {
        /// <summary>
        /// Enumeration whose values specify that the object association is not specified. The value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enumeration whose values specify that the custom action is associated with a list. The value = 1.
        /// </summary>
        List = 1,
        /// <summary>
        /// Enumeration whose values specify that the custom action is associated with a content type. The value = 2.
        /// </summary>
        ContentType = 2,
        /// <summary>
        /// Enumeration whose values specify that the custom action is associated with a ProgID. The value = 3.
        /// </summary>
        ProgId = 3,
        /// <summary>
        /// Enumeration whose values specify that the custom action is associated with a file extension. The value = 4.
        /// </summary>
        FileType = 4,
    }
}
