using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines an event in a Team
    /// </summary>
    [ConcreteType(typeof(GraphEvent))]
    public interface IGraphEvent : IDataModel<IGraphEvent>, IDataModelGet<IGraphEvent>, IDataModelLoad<IGraphEvent>, IDataModelDelete, IQueryableDataModel
    {

        #region Properties

        /// <summary>
        /// Identifier that uniquely identifies a specific instance of an event. Read only.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// True if the meeting organizer allows invitees to propose a new time when responding; otherwise, false.
        /// </summary>
        public bool AllowNewTimeProposals { get; }

        /// <summary>
        /// The preview of the message associated with the event. It is in text format.
        /// </summary>
        public string BodyPreview { get; }

        /// <summary>
        /// The categories associated with the event. Each category corresponds to the displayName property of an outlookCategory defined for the user.
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Identifies the version of the event object. Every time the event is changed, ChangeKey changes as well. This allows Exchange to apply changes to the correct version of the object.
        /// </summary>
        public string ChangeKey { get; }

        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; }

        /// <summary>
        /// Timestamp when the event was last modified
        /// </summary>
        public DateTimeOffset LastModifiedDateTime { get; }

        /// <summary>
        /// Set to true if the event has attachments.
        /// </summary>
        public bool HasAttachments { get; }

        /// <summary>
        /// When set to true, each attendee only sees themselves in the meeting request and meeting Tracking list. Default is false.
        /// </summary>
        public bool HideAttendees { get; set; }

        /// <summary>
        /// A unique identifier for an event across calendars. This ID is different for each occurrence in a recurring series.
        /// </summary>
        public string ICalUId { get; }

        /// <summary>
        /// The importance of the event.
        /// </summary>
        public EventImportance Importance { get; set; }

        /// <summary>
        /// Set to true if the event lasts all day.
        /// </summary>
        public bool IsAllDay { get; set; }

        /// <summary>
        /// Set to true if the event has been canceled.
        /// </summary>
        public bool IsCancelled { get; }

        /// <summary>
        /// Set to true if the user has updated the meeting in Outlook but has not sent the updates to attendees. Set to false if all changes have been sent, or if the event is an appointment without any attendees.
        /// </summary>
        public bool IsDraft { get; }

        /// <summary>
        /// Defines if the event is an online meeting (e.g. Teams meeting)
        /// </summary>
        public bool IsOnlineMeeting { get; set; }

        /// <summary>
        /// Defines if the event was created by the requestor
        /// </summary>
        public bool IsOrganizer { get; }

        /// <summary>
        /// Set to true if an alert is set to remind the user of the event. (n/a for Team events)
        /// </summary>
        public bool IsReminderOn { get; }

        /// <summary>
        /// Represents the online meeting service provider. 
        /// After you set onlineMeetingProvider, Microsoft Graph initializes onlineMeeting. Subsequently you cannot change onlineMeetingProvider again, and the meeting remains available online.
        /// </summary>
        public EventOnlineMeetingProvider OnlineMeetingProvider { get; set; }

        /// <summary>
        /// A URL for an online meeting. The property is set only when an organizer specifies in Outlook that an event is an online meeting such as Skype.
        /// To access the URL to join an online meeting, use joinUrl which is exposed via the onlineMeeting property of the event. The onlineMeetingUrl property will be deprecated in the future.
        /// </summary>
        public string OnlineMeetingUrl { get; }

        /// <summary>
        /// The end time zone that was set when the event was created.
        /// </summary>
        public string OriginalEndTimeZone { get; }

        /// <summary>
        /// Represents the start time of an event when it is initially created as an occurrence or exception in a recurring series. 
        /// This property is not returned for events that are single instances. 
        /// Its date and time information is expressed in ISO 8601 format and is always in UTC. For example, midnight UTC on Jan 1, 2014 is 2014-01-01T00:00:00Z
        /// </summary>
        public DateTimeOffset OriginalStart { get; }

        /// <summary>
        /// The start time zone that was set when the event was created.
        /// </summary>
        public string OriginalStartTimeZone { get; }

        /// <summary>
        /// The number of minutes before the event start time that the reminder alert occurs.
        /// </summary>
        public int ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Defines if the organizer would like an invitee to send a response to the event.
        /// </summary>
        public bool ResponseRequested { get; set; }

        /// <summary>
        /// Sensitivity of the event
        /// </summary>
        public EventSensitivity Sensitivity { get; set; }

        /// <summary>
        /// The ID for the recurring series master item, if this event is part of a recurring series.
        /// </summary>
        public string SeriesMasterId { get; }

        /// <summary>
        /// The status to show.
        /// </summary>
        public EventShowAs ShowAs { get; set; }

        /// <summary>
        /// The text of the event's subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// A custom identifier specified by a client app for the server to avoid redundant POST operations in case of client retries to create the same event. 
        /// This is useful when low network connectivity causes the client to time out before receiving a response from the server for the client's prior create-event request. 
        /// After you set transactionId when creating an event, you cannot change transactionId in a subsequent update. 
        /// This property is only returned in a response payload if an app has set it. 
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// The event type. 
        /// </summary>
        public EventType Type { get; }

        /// <summary>
        /// The URL to open the event in Outlook on the web.
        /// </summary>
        public string WebLink { get; }

        /// <summary>
        /// The body of the message associated with the event. It can be in HTML or text format.
        /// </summary>
        public IGraphItemBody Body { get; }

        /// <summary>
        /// The body of the message associated with the event. It can be in HTML or text format.
        /// </summary>
        public IGraphEventAttendeeCollection Attendees { get; set; }

        /// <summary>
        /// The date, time, and time zone that the event starts. By default, the start time is in UTC.
        /// </summary>
        public IGraphDateTimeTimeZone Start { get; set; }

        /// <summary>
        /// The date, time, and time zone that the event starts. By default, the start time is in UTC.
        /// </summary>
        public IGraphDateTimeTimeZone End { get; set; }

        /// <summary>
        /// Indicates the type of response sent in response to an event message.
        /// </summary>
        public IGraphEventResponseStatus ResponseStatus { get; }

        /// <summary>
        /// The location of the event.
        /// </summary>
        public IGraphLocation Location { get; set; }

        /// <summary>
        /// The locations where the event is held or attended from. 
        /// The location and locations properties always correspond with each other. 
        /// If you update the location property, any prior locations in the locations collection would be removed and replaced by the new location value.
        /// </summary>
        public IGraphLocationCollection Locations { get; set; }

        /// <summary>
        /// Details for an attendee to join the meeting online. 
        /// Default is null. 
        /// Read-only. 
        /// </summary>
        public IGraphOnlineMeetingInfo OnlineMeeting { get; }

        /// <summary>
        /// The organizer of the event.
        /// </summary>
        public IGraphRecipient Organizer { get; }

        /// <summary>
        /// The recurrence pattern for the event.
        /// </summary>
        public IGraphPatternedRecurrence Recurrence { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method to update the event
        /// </summary>
        /// <param name="options">Options on what to update</param>
        /// <returns></returns>
        Task UpdateAsync(EventUpdateOptions options);

        /// <summary>
        /// Method to update the event
        /// </summary>
        /// <param name="options">Options on what to update</param>
        void Update(EventUpdateOptions options);

        #endregion
    }
}
