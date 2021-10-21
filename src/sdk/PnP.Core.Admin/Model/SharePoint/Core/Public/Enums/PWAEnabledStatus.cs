namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents the status of EnableProjectWebInstance on a site collection.
    /// </summary>
    public enum PWAEnabledStatus
    {
        /// <summary>
        /// Default value, Project Web App status was never set
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The project web app is disabled for this site collection
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// The project web app is enabled for this site collection
        /// </summary>
        Enabled = 2
    }
}
