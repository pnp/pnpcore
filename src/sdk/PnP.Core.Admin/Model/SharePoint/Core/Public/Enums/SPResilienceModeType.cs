namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the Resilience Mode type values.
    /// </summary>
    public enum SPResilienceModeType
    {
        /// <summary>
        /// Resilence Mode has same value as AAD.
        /// </summary>
        DefaultAAD = 0,

        /// <summary>
        /// When enabled, Resilence features are enabled during AAD outage.
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// When disabled, Resilence features are disabled during AAD outage.
        /// </summary>
        Disabled = 2,
    }
}
