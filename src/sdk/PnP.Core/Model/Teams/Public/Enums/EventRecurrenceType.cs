namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Recurrence type
    /// </summary>
    public enum EventRecurrenceType
    {
        /// <summary>
        /// Event repeats based on the number of days specified by interval between occurrences.
        /// </summary>
        Daily,

        /// <summary>
        /// Event repeats on the same day or days of the week, based on the number of weeks between each set of occurrences.
        /// </summary>
        Weekly,

        /// <summary>
        /// Event repeats on the specified day of the month (e.g. the 15th), based on the number of months between occurrences.
        /// </summary>
        AbsoluteMonthly,

        /// <summary>
        /// Event repeats on the specified day or days of the week, in the same relative position in the month, based on the number of months between occurrences.
        /// </summary>
        RelativeMonthly,

        /// <summary>
        /// Event repeats on the specified day and month, based on the number of years between occurrences.
        /// </summary>
        AbsoluteYearly,

        /// <summary>
        /// Event repeats on the specified day or days of the week, in the same relative position in a specific month of the year, based on the number of years between occurrences.
        /// </summary>
        RelativeYearly
    }
}
