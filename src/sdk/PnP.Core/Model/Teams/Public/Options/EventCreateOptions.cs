using System;
using System.Collections.Generic;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Options that can be set when creating a meeting request
    /// </summary>
    public class EventCreateOptions
    {

        /// <summary>
        /// Subject property
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body message
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Body content type
        /// </summary>
        public EventBodyType BodyContentType { get; } = EventBodyType.Html;

        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Start time zone
        /// </summary>
        public EventTimeZone StartTimeZone { get; set; } = EventTimeZone.GMTStandardTime;

        /// <summary>
        /// End time of the event
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// End time zone
        /// </summary>
        public EventTimeZone EndTimeZone { get; set; } = EventTimeZone.GMTStandardTime;

        /// <summary>
        /// Location of the event
        /// </summary>
        public EventLocationOptions Location { get; set; }

        /// <summary>
        /// Locations of the event. 
        /// An event can have multiple locations
        /// </summary>
        public List<EventLocationOptions> Locations { get; set; }

        /// <summary>
        /// List of attendees to be invited to the meeting
        /// </summary>
        public List<EventAttendeeOptions> Attendees { get; set; }

        /// <summary>
        /// Allow new time proposals
        /// </summary>
        public bool AllowNewTimeProposals { get; set; }

        /// <summary>
        /// When set to true, each attendee only sees themselves in the meeting request and meeting Tracking list. 
        /// Default is false.
        /// </summary>
        public bool HideAttendees { get; set; }

        /// <summary>
        /// Set to true if the event lasts all day.
        /// </summary>
        public bool IsAllDay { get; set; }

        /// <summary>
        /// Set to true if the sender would like a response when the event is accepted or declined.
        /// </summary>
        public bool ResponseRequested { get; set; }

        /// <summary>
        /// True if this event has online meeting information, false otherwise. 
        /// Default is false. 
        /// Optional.
        /// </summary>
        public bool IsOnlineMeeting { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// The importance of the event. 
        /// The possible values are: low, normal, high.
        /// </summary>
        public EventImportance Importance { get; set; }

        /// <summary>
        /// The sensitivity of the event. 
        /// </summary>
        public EventSensitivity Sensitivity { get; set; }

        /// <summary>
        /// The status to show
        /// </summary>
        public EventShowAs ShowAs { get; set; }

        /// <summary>
        /// Represents the online meeting service provider.
        /// </summary>
        public EventOnlineMeetingProvider OnlineMeetingProvider { get; set; }

        /// <summary>
        /// The recurrence pattern for the event.
        /// </summary>
        public EventRecurrenceOptions Recurrence { get; set; }
    }
}
