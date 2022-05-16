using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Describes the frequency by which a recurring event repeats. 
    /// This shared object is used to define the recurrence of access reviews, calendar events, and access package assignments in Azure AD.
    /// </summary>
    [ConcreteType(typeof(GraphRecurrencePattern))]
    public interface IGraphRecurrencePattern : IDataModel<IGraphRecurrencePattern>
    {
        /// <summary>
        /// The day of the month on which the event occurs. Required if type is absoluteMonthly or absoluteYearly.
        /// </summary>
        public int DayOfMonth { get; set; }

        /// <summary>
        /// A collection of the days of the week on which the event occurs. The possible values are: sunday, monday, tuesday, wednesday, thursday, friday, saturday.
        /// If type is relativeMonthly or relativeYearly, and daysOfWeek specifies more than one day, the event falls on the first day that satisfies the pattern.
        /// Required if type is weekly, relativeMonthly, or relativeYearly.
        /// </summary>
        public List<string> DaysOfWeek { get; set; }

        /// <summary>
        /// The first day of the week. The possible values are: sunday, monday, tuesday, wednesday, thursday, friday, saturday. Default is sunday. 
        /// Required if type is weekly.
        /// </summary>
        public string FirstDayOfWeek { get; set; }

        /// <summary>
        /// Specifies on which instance of the allowed days specified in daysOfWeek the event occurs, counted from the first instance in the month
        /// The possible values are: first, second, third, fourth, last. Default is first. 
        /// Optional and used if type is relativeMonthly or relativeYearly.
        /// </summary>
        public EventWeekIndex Index { get; set; }

        /// <summary>
        /// The number of units between occurrences, where units can be in days, weeks, months, or years, depending on the type. 
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The month in which the event occurs. 
        /// This is a number from 1 to 12.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// The recurrence pattern type.
        /// </summary>
        public EventRecurrenceType Type { get; set; }
    }
}
