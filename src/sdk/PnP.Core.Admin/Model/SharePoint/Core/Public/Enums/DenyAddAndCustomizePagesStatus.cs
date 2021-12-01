namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Represents the status of DenyAddAndCustomizePages on a site collection.
    /// </summary>
    public enum DenyAddAndCustomizePagesStatus
    {
        /// <summary>
        /// Value is unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// DenyAddAndCustomizePages is disabled
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// DenyAddAndCustomizePages is enabled
        /// </summary>
        Enabled = 2
    }
}
