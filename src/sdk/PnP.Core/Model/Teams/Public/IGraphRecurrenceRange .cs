using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Describes a date range over which a recurring event. This shared object is used to define the recurrence of access reviews, calendar events, and access package assignments in Azure AD.
    /// </summary>
    [ConcreteType(typeof(GraphRecurrenceRange))]
    public interface IGraphRecurrenceRange : IDataModel<IGraphRecurrenceRange>
    {
        /// <summary>
        /// The date to start applying the recurrence pattern. 
        /// The first occurrence of the meeting may be this date or later, depending on the recurrence pattern of the event. 
        /// Must be the same value as the start property of the recurring event. 
        /// Required.
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// The date to stop applying the recurrence pattern. 
        /// Depending on the recurrence pattern of the event, the last occurrence of the meeting may not be this date. 
        /// Required if type is endDate.
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// The number of times to repeat the event. 
        /// Required and must be positive if type is numbered.
        /// </summary>
        public int NumberOfOccurrences { get; }

        /// <summary>
        /// Time zone for the startDate and endDate properties. 
        /// Optional. 
        /// If not specified, the time zone of the event is used.
        /// </summary>
        public string RecurrenceTimeZone { get; }

        /// <summary>
        /// The recurrence range.
        /// </summary>
        public EventRecurrenceRangeType Type { get; }

    }
}
