namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Interval for which to get analytics
    /// </summary>
    public enum AnalyticsInterval
    {
        /// <summary>
        /// Get all time consolidated analytics
        /// </summary>
        AllTime = 0,

        /// <summary>
        /// Get analytics for the last seven days
        /// </summary>
        LastSevenDays = 1,

        /// <summary>
        /// Get analytics for a custom time window using a day, week or month agregation
        /// </summary>
        Custom = 2
    }
}
