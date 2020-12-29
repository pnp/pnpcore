namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Contains the values of the 3 allowed states for Sharing properties.
    /// </summary>
    public enum SharingState
    {
        /// <summary>
        /// State 1: Unspecified. Implies that the Tenant Adnin is choosing to let Site and Web Owners specify the behavior for Sharing Property State
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// State 2: On. Implies that Tenant Admin is forcing Sharing Property State to behave as if they are set to True
        /// </summary>
        On = 1,
        /// <summary>
        /// State 3: Off. Implies that Tenant Admin is forcing Sharing Property State to behave as if they are set to False
        /// </summary>
        Off = 2,
    }
}
