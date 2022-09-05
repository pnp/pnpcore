using System;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// The response status of an attendee or organizer for a meeting request.
    /// </summary>
    [ConcreteType(typeof(GraphEventResponseStatus))]
    public interface IGraphEventResponseStatus : IDataModel<IGraphEventResponseStatus>
    {
        /// <summary>
        /// The type of the attendee
        /// </summary>
        public EventResponse Response { get; }

        /// <summary>
        /// The date and time that the response was returned. It uses ISO 8601 format and is always in UTC time. For example, midnight UTC on Jan 1, 2014 is 2014-01-01T00:00:00Z
        /// </summary>
        public DateTimeOffset Time { get; }

    }
}
