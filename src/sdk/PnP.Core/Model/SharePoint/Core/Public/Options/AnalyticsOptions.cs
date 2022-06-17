using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines what analytics must be retrieved
    /// </summary>
    public class AnalyticsOptions
    {
        /// <summary>
        /// Defines the analytics interval, defaults to all time
        /// </summary>
        public AnalyticsInterval Interval { get; set; } = AnalyticsInterval.AllTime;

        /// <summary>
        /// Start date (UTC) for analytics retrieval using <see cref="AnalyticsInterval.Custom"/> as interval
        /// </summary>
        public DateTime CustomStartDate { get; set; }

        /// <summary>
        /// End date (UTC) for analytics retrieval using <see cref="AnalyticsInterval.Custom"/> as interval
        /// </summary>
        public DateTime CustomEndDate { get; set; }

        /// <summary>
        /// Aggregation interval for analytics retrieval using <see cref="AnalyticsInterval.Custom"/> as interval. Defaults to Day
        /// </summary>
        public AnalyticsAggregationInterval CustomAggregationInterval { get; set; } = AnalyticsAggregationInterval.Day;
    }
}
