namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Agregation interval for when analytics are retrieved in a custom timewindow
    /// </summary>
    public enum AnalyticsAggregationInterval
    {
        /// <summary>
        /// Aggregate by day
        /// </summary>
        Day = 0,

        /// <summary>
        /// Aggregate by week
        /// </summary>
        Week = 1,

        /// <summary>
        /// Aggregate by month
        /// </summary>
        Month = 2
    }
}
