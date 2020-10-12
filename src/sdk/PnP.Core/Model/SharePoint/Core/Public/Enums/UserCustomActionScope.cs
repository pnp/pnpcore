namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the scope of the custom action.
    /// </summary>
    public enum UserCustomActionScope
    {
        /// <summary>
        /// Enumeration whose values specify that the scope of the custom action is not specified. The value = 0.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Enumeration whose values specify that the scope of the custom action is limited to a site collection. The value = 2.
        /// </summary>
        Site = 2,
        /// <summary>
        /// Enumeration whose values specify that the scope of the custom action is limited to a site. The value = 3.
        /// </summary>
        Web = 3,
        /// <summary>
        /// Enumeration whose values specify that the scope of the custom action is limited to a list. The value = 4.
        /// </summary>
        List = 4
    }
}
