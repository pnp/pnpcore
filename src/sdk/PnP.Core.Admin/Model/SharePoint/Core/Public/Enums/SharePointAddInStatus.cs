namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the statuses of a SharePoint AddIn
    /// </summary>
    public enum SharePointAddInStatus
    {
        /// <summary>
        /// Instaled
        /// </summary>
        Installed,

        /// <summary>
        /// Installing
        /// </summary>
        Installing,

        /// <summary>
        /// Uninstalling
        /// </summary>
        Uninstalling,

        /// <summary>
        /// Upgrading
        /// </summary>
        Upgrading,

        /// <summary>
        /// Recycling
        /// </summary>
        Recycling,

        /// <summary>
        /// Invalid status
        /// </summary>
        InvalidStatus,

        /// <summary>
        /// Canceling
        /// </summary>
        Canceling,

        /// <summary>
        /// Initialized
        /// </summary>
        Initialized,

        /// <summary>
        /// UpgradeCanceling
        /// </summary>
        UpgradeCanceling,

        /// <summary>
        /// Disabling
        /// </summary>
        Disabling,

        /// <summary>
        /// Disabled
        /// </summary>
        Disabled,

        /// <summary>
        /// SecretRolling
        /// </summary>
        SecretRolling,

        /// <summary>
        /// Restoring
        /// </summary>
        Restoring,

        /// <summary>
        /// RestoreCanceling
        /// </summary>
        RestoreCanceling
    }
}
