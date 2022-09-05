namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Represents properties of the attendee of an event
    /// </summary>
    [ConcreteType(typeof(GraphEventAttendee))]
    public interface IGraphEventAttendee : IDataModel<IGraphEventAttendee>
    {
        /// <summary>
        /// Includes the name and SMTP address of the attendee.
        /// </summary>
        public IGraphEmailAddress EmailAddress { get; set; }

        /// <summary>
        /// The type of the attendee
        /// </summary>
        public EventAttendeeType Type { get; set; }

        /// <summary>
        /// The attendee's response (none, accepted, declined, etc.) for the event and date-time that the response was sent.
        /// </summary>
        public IGraphEventResponseStatus ResponseStatus { get; }

        /// <summary>
        /// An alternate date/time proposed by the attendee for a meeting request to start and end. 
        /// If the attendee hasn't proposed another time, then this property is not included in a response of a GET event.
        /// </summary>
        public IGraphTimeSlot TimeSlot { get; }

    }
}
