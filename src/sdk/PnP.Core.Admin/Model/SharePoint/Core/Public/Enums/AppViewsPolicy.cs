namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates whether app views are disabled in all the webs of this site
    /// </summary>
    public enum AppViewsPolicy
    {
        /// <summary>
        /// Internal use only
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Indicates that app views are disabled in all the webs of this site
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// Indicates that app views may be enabled in some of the webs of this site
        /// </summary>
        NotDisabled = 2
    }
}
