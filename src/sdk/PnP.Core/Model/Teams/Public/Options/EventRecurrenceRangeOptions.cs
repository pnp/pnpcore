using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set for the recurrence range
    /// </summary>
    public class EventRecurrenceRangeOptions
    {
        /// <summary>
        /// Start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Amount of occurences
        /// </summary>
        public int NumberOfOccurences { get; set; }

        /// <summary>
        /// Time zone
        /// </summary>
        public string RecurrenceTimeZone { get; set; }

        /// <summary>
        /// Range type
        /// </summary>
        public EventRecurrenceRangeType Type { get; set; }
    }
}
