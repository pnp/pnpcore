using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set for the recurrence pattern
    /// </summary>
    public class EventRecurrencePatternOptions
    {

        /// <summary>
        /// Type of recurrence
        /// </summary>
        public EventRecurrenceType Type { get; set; }

        /// <summary>
        /// Interval
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Month
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Day of the month
        /// </summary>
        public int DayOfMonth { get; set; } 

        /// <summary>
        /// Days of week of the recurrence event
        /// </summary>
        public List<DayOfWeek> DaysOfWeek { get; set; }

        /// <summary>
        /// First day of week
        /// </summary>
        public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

        /// <summary>
        /// Index
        /// </summary>
        public EventWeekIndex Index { get; set; }
    }
}
