using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    [GraphType(Uri = eventUri, LinqGet = baseUri)]
    internal sealed class GraphEvent : BaseDataModel<IGraphEvent>, IGraphEvent
    {
        private const string baseUri = "groups/{Site.GroupId}/events";
        private const string eventUri = baseUri + "/{GraphId}";

        public GraphEvent()
        {
            // Handler to construct the Add request for this event
            AddApiCallHandler = async (keyValuePairs) =>
            {
                dynamic body = new ExpandoObject();

                body.subject = Subject;
                body.transactionId = TransactionId;

                //bools
                body.hideAttendees = HideAttendees;
                body.isOnlineMeeting = IsOnlineMeeting;
                body.isAllDay = IsAllDay;
                body.allowNewTimeProposals = AllowNewTimeProposals;
                body.responseRequested = ResponseRequested;

                //enums
                body.importance = Importance.ToString();
                body.sensitivity = Sensitivity.ToString();
                body.showAs = ShowAs.ToString();
                body.onlineMeetingProvider = OnlineMeetingProvider.ToString();

                if (Body != null && Body.HasValue("Content"))
                {
                    dynamic bodyContentMessage = new ExpandoObject();

                    bodyContentMessage.content = Body.Content;
                    bodyContentMessage.contentType = Body.ContentType.ToString();

                    body.body = bodyContentMessage;
                }

                if (Attendees != null && Attendees.Length > 0)
                {
                    dynamic attendeeList = new List<dynamic>();

                    foreach (var attendee in Attendees)
                    {
                        dynamic attendeeObj = new ExpandoObject();
                        dynamic emailAddressObj = new ExpandoObject();
                        emailAddressObj.address = attendee.EmailAddress.Address;
                        emailAddressObj.name = attendee.EmailAddress.Name;
                        attendeeObj.emailAddress = emailAddressObj;
                        attendeeObj.Type = attendee.Type.ToString();
                        attendeeList.Add(attendeeObj);
                    }

                    body.attendees = attendeeList;
                }

                if (Location != null && Location.HasValue("DisplayName"))
                {
                    body.location = GetLocation(Location);
                }

                if (Locations != null && Locations.Length > 0)
                {
                    dynamic locationList = new List<dynamic>();

                    foreach (var location in Locations)
                    {
                        locationList.Add(GetLocation(location));
                    }

                    body.locations = locationList;
                }

                if (Start != null && Start.HasValue("DateTime") && !string.IsNullOrEmpty(Start.DateTime))
                {
                    dynamic start = new ExpandoObject();

                    start.dateTime = Start.DateTime;
                    start.timeZone = Start.TimeZone;

                    body.start = start;
                }

                if (End != null && End.HasValue("DateTime") && !string.IsNullOrEmpty(End.DateTime))
                {
                    dynamic end = new ExpandoObject();

                    end.dateTime = End.DateTime;
                    end.timeZone = End.TimeZone;

                    body.end = end;
                }

                if (Recurrence != null && (Recurrence.HasValue("Pattern") || Recurrence.HasValue("Range")))
                {
                    body.recurrence = GetRecurrence();
                }

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(this, baseUri).ConfigureAwait(false);

                return new ApiCall(parsedApiCall, ApiType.Graph, bodyContent);
            };
        }

        #region Properties

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public bool AllowNewTimeProposals { get => GetValue<bool>(); set => SetValue(value); }

        public string BodyPreview { get => GetValue<string>(); set => SetValue(value); }

        public List<string> Categories { get => GetValue<List<string>>(); set => SetValue(value); }

        public string ChangeKey { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset CreatedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public DateTimeOffset LastModifiedDateTime { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public bool HasAttachments { get => GetValue<bool>(); set => SetValue(value); }

        public bool HideAttendees { get => GetValue<bool>(); set => SetValue(value); }

        public string ICalUId { get => GetValue<string>(); set => SetValue(value); }

        public EventImportance Importance { get => GetValue<EventImportance>(); set => SetValue(value); }

        public bool IsAllDay { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsCancelled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDraft { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsOnlineMeeting { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsOrganizer { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsReminderOn { get => GetValue<bool>(); set => SetValue(value); }

        public EventOnlineMeetingProvider OnlineMeetingProvider { get => GetValue<EventOnlineMeetingProvider>(); set => SetValue(value); }

        public string OnlineMeetingUrl { get => GetValue<string>(); set => SetValue(value); }

        public string OriginalEndTimeZone { get => GetValue<string>(); set => SetValue(value); }

        public DateTimeOffset OriginalStart { get => GetValue<DateTimeOffset>(); set => SetValue(value); }

        public string OriginalStartTimeZone { get => GetValue<string>(); set => SetValue(value); }

        public int ReminderMinutesBeforeStart { get => GetValue<int>(); set => SetValue(value); }

        public bool ResponseRequested { get => GetValue<bool>(); set => SetValue(value); }

        public EventSensitivity Sensitivity { get => GetValue<EventSensitivity>(); set => SetValue(value); }

        public string SeriesMasterId { get => GetValue<string>(); set => SetValue(value); }

        public EventShowAs ShowAs { get => GetValue<EventShowAs>(); set => SetValue(value); }

        public string Subject { get => GetValue<string>(); set => SetValue(value); }

        public string TransactionId { get => GetValue<string>(); set => SetValue(value); }

        public EventType Type { get => GetValue<EventType>(); set => SetValue(value); }

        public string WebLink { get => GetValue<string>(); set => SetValue(value); }

        public IGraphItemBody Body { get => GetModelValue<IGraphItemBody>(); set => SetModelValue(value); }

        public IGraphEventAttendeeCollection Attendees { get => GetModelCollectionValue<IGraphEventAttendeeCollection>(); set => SetModelValue(value); }

        public IGraphDateTimeTimeZone Start { get => GetModelValue<IGraphDateTimeTimeZone>(); set => SetModelValue(value); }

        public IGraphDateTimeTimeZone End { get => GetModelValue<IGraphDateTimeTimeZone>(); set => SetModelValue(value); }

        public IGraphEventResponseStatus ResponseStatus { get => GetModelValue<IGraphEventResponseStatus>(); set => SetModelValue(value); }

        public IGraphLocation Location { get => GetModelValue<IGraphLocation>(); set => SetModelValue(value); }

        public IGraphLocationCollection Locations { get => GetModelCollectionValue<IGraphLocationCollection>(); set => SetModelValue(value); }

        public IGraphOnlineMeetingInfo OnlineMeeting { get => GetModelValue<IGraphOnlineMeetingInfo>(); set => SetModelValue(value); }

        public IGraphRecipient Organizer { get => GetModelValue<IGraphRecipient>(); set => SetModelValue(value); }

        public IGraphPatternedRecurrence Recurrence { get => GetModelValue<IGraphPatternedRecurrence>(); set => SetModelValue(value); }
        
        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }
        #endregion

        #region Methods

        public async Task UpdateAsync(EventUpdateOptions options)
        {
            dynamic body = GetUpdateObject(options);

            var apiCall = new ApiCall($"groups/{PnPContext.Site.GroupId}/events/{Id}", ApiType.Graph, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));

            await RawRequestAsync(apiCall, new HttpMethod("PATCH")).ConfigureAwait(false);
        }

        

        public void Update(EventUpdateOptions options)
        {
            UpdateAsync(options).GetAwaiter().GetResult();
        }

        #endregion

        #region Helper methods

        private dynamic GetAddress(IGraphLocation location)
        {
            dynamic address = new ExpandoObject();

            if (location.Address.HasValue("Street"))
            {
                address.street = location.Address.Street;
            }

            if (location.Address.HasValue("City"))
            {
                address.city = location.Address.City;
            }

            if (location.Address.HasValue("CountryOrRegion"))
            {
                address.countryOrRegion = location.Address.CountryOrRegion;
            }

            if (location.Address.HasValue("CountryOrRegion"))
            {
                address.countryOrRegion = location.Address.CountryOrRegion;
            }

            if (location.Address.HasValue("State"))
            {
                address.state = location.Address.State;
            }

            return address;
        }

        private dynamic GetCoordinates(IGraphLocation location)
        {
            dynamic coordinates = new ExpandoObject();

            if (location.Coordinates.HasValue("Longitude"))
            {
                coordinates.longitude = location.Coordinates.Longitude;
            }

            if (location.Coordinates.HasValue("Accuracy"))
            {
                coordinates.accuracy = location.Coordinates.Accuracy;
            }

            if (location.Coordinates.HasValue("Altitude"))
            {
                coordinates.altitude = location.Coordinates.Altitude;
            }

            if (location.Coordinates.HasValue("AltitudeAccuracy"))
            {
                coordinates.altitudeAccuracy = location.Coordinates.AltitudeAccuracy;
            }

            if (location.Coordinates.HasValue("Latitude"))
            {
                coordinates.latitude = location.Coordinates.Latitude;
            }

            return coordinates;
        }

        private dynamic GetLocation(IGraphLocation location)
        {
            dynamic locationObj = new ExpandoObject();
            
            if (location.HasValue("DisplayName"))
            { 
                locationObj.displayName = location.DisplayName;
            }

            if (location.HasValue("LocationEmailAddress"))
            {
                locationObj.locationEmailAddress = location.LocationEmailAddress;
            }

            if (location.HasValue("LocationType"))
            { 
                locationObj.locationType = location.LocationType.ToString();
            }
            if (location.HasValue("LocationUri"))
            { 
                locationObj.locationUri = location.LocationUri;
            }
          
            locationObj.address = GetAddress(location);
            locationObj.coordinates = GetCoordinates(location);

            return locationObj;
        }

        private dynamic GetRecurrence()
        {
            dynamic recurrence = new ExpandoObject();

            if (Recurrence.HasValue("Pattern"))
            {
                recurrence.pattern = GetRecurrencePattern(Recurrence.Pattern);
            }

            if (Recurrence.HasValue("Range"))
            {
                recurrence.range = GetRecurrenceRange(Recurrence.Range);
            }

            return recurrence;
        }

        private dynamic GetRecurrenceRange(IGraphRecurrenceRange recurrenceRange)
        {
            dynamic range = new ExpandoObject();

            if (recurrenceRange.HasValue("EndDate") && recurrenceRange.EndDate != DateTime.MinValue)
            {
                range.endDate = recurrenceRange.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (recurrenceRange.HasValue("StartDate") && recurrenceRange.StartDate != DateTime.MinValue)
            {
                range.startDate = recurrenceRange.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (recurrenceRange.HasValue("NumberOfOccurrences") && recurrenceRange.NumberOfOccurrences > 0)
            {
                range.numberOfOccurrences = recurrenceRange.NumberOfOccurrences;
            }

            if (recurrenceRange.HasValue("RecurrenceTimeZone") && !string.IsNullOrEmpty(recurrenceRange.RecurrenceTimeZone))
            {
                range.recurrenceTimeZone = recurrenceRange.RecurrenceTimeZone;
            }

            if (Recurrence.Range.HasValue("Type"))
            {
                range.type = recurrenceRange.Type.ToString();
            }

            return range;
        }

        private dynamic GetRecurrencePattern(IGraphRecurrencePattern recurencePattern)
        {
            dynamic pattern = new ExpandoObject();

            if (recurencePattern.HasValue("Type"))
            {
                pattern.type = recurencePattern.Type.ToString();
            }

            if (recurencePattern.HasValue("Month") && recurencePattern.Month > 0)
            {
                pattern.month = recurencePattern.Month;
            }

            if (recurencePattern.HasValue("Interval") && recurencePattern.Interval > 0)
            {
                pattern.interval = recurencePattern.Interval;
            }

            if (recurencePattern.HasValue("Index") && (recurencePattern.Type == EventRecurrenceType.RelativeMonthly || recurencePattern.Type == EventRecurrenceType.RelativeYearly))
            {
                pattern.index = recurencePattern.Index.ToString();
            }

            if (recurencePattern.HasValue("FirstDayOfWeek") && !string.IsNullOrEmpty(recurencePattern.FirstDayOfWeek))
            {
                pattern.firstDayOfWeek = recurencePattern.FirstDayOfWeek.ToString();
            }

            if (recurencePattern.HasValue("DaysOfWeek") && recurencePattern.DaysOfWeek.Count > 0)
            {
                dynamic dayOfWeekList = new List<dynamic>();

                foreach (var day in recurencePattern.DaysOfWeek)
                {
                    dayOfWeekList.Add(day.ToString());
                }

                pattern.daysOfWeek = dayOfWeekList;
            }

            if (recurencePattern.HasValue("DayOfMonth") && recurencePattern.DayOfMonth > 0)
            {
                pattern.dayOfMonth = recurencePattern.DayOfMonth;
            }

            return pattern;
        }

        private dynamic GetUpdateObject(EventUpdateOptions options)
        {
            dynamic body = new ExpandoObject();

            if (!string.IsNullOrEmpty(options.Subject))
            {
                body.subject = options.Subject;
            }

            if (!string.IsNullOrEmpty(options.Body))
            {
                dynamic graphBody = new ExpandoObject();
                graphBody.content = options.Body;
                graphBody.contentType = options.BodyContentType.ToString();
                body.body = graphBody;
            }

            if (options.Attendees != null)
            {
                dynamic attendeeList = new List<dynamic>();

                foreach (var attendee in options.Attendees)
                {
                    dynamic attendeeObj = new ExpandoObject();
                    dynamic emailAddressObj = new ExpandoObject();
                    emailAddressObj.address = attendee.EmailAddress;
                    emailAddressObj.name = attendee.Name;
                    attendeeObj.emailAddress = emailAddressObj;
                    attendeeObj.Type = attendee.Type.ToString();
                    attendeeList.Add(attendeeObj);
                }

                body.attendees = attendeeList;
            }

            if (options.Location != null)
            {
                body.location = GetLocationFromOptions(options.Location);
            }

            if (options.Locations != null && options.Locations.Count > 0)
            {
                dynamic locationList = new List<dynamic>();

                foreach (var location in options.Locations)
                {
                    locationList.Add(GetLocationFromOptions(location));
                }

                body.locations = locationList;
            }

            if (options.IsOnlineMeeting && (IsOnlineMeeting != options.IsOnlineMeeting))
            {
                body.isOnlineMeeting = options.IsOnlineMeeting;
            }

            if (options.HideAttendees && (HideAttendees != options.HideAttendees))
            {
                body.hideAttendees = options.HideAttendees;
            }

            if (options.ShowAs != ShowAs)
            {
                body.showAs = options.ShowAs.ToString();
            }

            if (options.Sensitivity != Sensitivity)
            {
                body.sensitivity = options.Sensitivity.ToString();
            }

            if (options.Importance != Importance)
            { 
                body.importance = options.Importance.ToString();
            }

            if (options.ResponseRequested != ResponseRequested)
            {
                body.responseRequested = options.ResponseRequested;
            }

            if (options.AllowNewTimeProposals != AllowNewTimeProposals)
            {
                body.allowNewTimeProposals = options.AllowNewTimeProposals;
            }

            if (options.IsAllDay != IsAllDay)
            {
                body.isAllDay = options.IsAllDay;
            }

            if (options.Start != DateTime.MinValue)
            {
                dynamic start = new ExpandoObject();

                if (options.IsAllDay)
                {
                    start.datetime = GraphEventHelper.ConvertDateTimeToAllDayString(options.Start);
                }
                else
                {
                    start.datetime = GraphEventHelper.ConvertDateTimeToRegularString(options.Start);
                }
                if (GraphEventHelper.GetEnumMemberValue(options.StartTimeZone) != Start.TimeZone.ToString())
                {
                    start.timeZone = GraphEventHelper.GetEnumMemberValue(options.StartTimeZone);
                }

                body.start = start;
            }

            if (options.End != DateTime.MinValue)
            {
                dynamic end = new ExpandoObject();

                if (options.IsAllDay)
                {
                    end.datetime = GraphEventHelper.ConvertDateTimeToAllDayString(options.End);
                }
                else
                {
                    end.datetime = GraphEventHelper.ConvertDateTimeToRegularString(options.End);
                }
                if (GraphEventHelper.GetEnumMemberValue(options.StartTimeZone) != Start.TimeZone.ToString())
                {
                    end.timeZone = GraphEventHelper.GetEnumMemberValue(options.StartTimeZone);
                }

                body.end = end;
            }

            if (options.OnlineMeetingProvider != OnlineMeetingProvider)
            {
                body.onlineMeetingProvider = options.OnlineMeetingProvider.ToString();
            }

            return body;
        }

        private dynamic GetLocationFromOptions(EventLocationOptions location)
        {
            dynamic locationObj = new ExpandoObject();

            locationObj.displayName = location.DisplayName;
            locationObj.locationUri = location.LocationUri;
            locationObj.locationEmailAddress = location.LocationEmailAddress;

            if (location.Address != null)
            {
                dynamic address = new ExpandoObject();

                address.street = location.Address.Street;
                address.state = location.Address.State;
                address.city = location.Address.City;
                address.countryOrRegion = location.Address.CountryOrRegion;
                address.postalCode = location.Address.PostalCode;

                locationObj.address = address;
            }

            if (location.Coordinates != null)
            {
                dynamic coordinates = new ExpandoObject();

                coordinates.accuracy = location.Coordinates.Accuracy;
                coordinates.altitude = location.Coordinates.Altitude;
                coordinates.altitudeAccuracy = location.Coordinates.AltitudeAccuracy;
                coordinates.longitude = location.Coordinates.Longitude;
                coordinates.latitude = location.Coordinates.Latitude;

                locationObj.coordinates = coordinates;
            }

            return locationObj;
        }

        #endregion
    }
}
