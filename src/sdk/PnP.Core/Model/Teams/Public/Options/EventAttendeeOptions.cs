namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set for the attendees
    /// </summary>
    public class EventAttendeeOptions
    {

        /// <summary>
        /// Type
        /// </summary>
        public EventAttendeeType Type { get; set; }

        /// <summary>
        /// Mail address
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }
}
