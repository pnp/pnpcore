namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the possible values for the default sharing state
    /// </summary>
    public enum SharingState
    {
        /// <summary>
        /// Implies that the Tenant Admin is choosing to let Site and Web Owners specify the behavior for Sharing Property State
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Implies that Tenant Admin is forcing Sharing Property State to behave as if they are set to True
        /// </summary>
        On = 1,

        /// <summary>
        /// Implies that Tenant Admin is forcing Sharing Property State to behave as if they are set to False
        /// </summary>
        Off = 2,
    }
}
