namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set for the recurrence
    /// </summary>
    public class EventRecurrenceOptions
    {

        /// <summary>
        /// Pattern options
        /// </summary>
        public EventRecurrencePatternOptions Pattern { get; set; }

        /// <summary>
        /// Range options
        /// </summary>
        public EventRecurrenceRangeOptions Range { get; set; }
    }
}
