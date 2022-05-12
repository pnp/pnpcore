using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Model.Teams
{
    internal sealed class GraphEventCollection : QueryableDataModelCollection<IGraphEvent>, IGraphEventCollection
    {
        public GraphEventCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add Methods

        public async Task<IGraphEvent> AddAsync(EventCreateOptions options)
        {
            GraphEventHelper.CheckCreateOptions(options);
            
            var newAndAdd = CreateNewAndAdd() as GraphEvent;

            //strings
            newAndAdd.Subject = options.Subject;
            newAndAdd.TransactionId = options.TransactionId;

            //bools
            newAndAdd.HideAttendees = options.HideAttendees;
            newAndAdd.IsOnlineMeeting = options.IsOnlineMeeting;
            newAndAdd.IsAllDay = options.IsAllDay;
            newAndAdd.AllowNewTimeProposals = options.AllowNewTimeProposals;
            newAndAdd.ResponseRequested = options.ResponseRequested;

            //enums
            newAndAdd.Importance = options.Importance;
            newAndAdd.Sensitivity = options.Sensitivity;
            newAndAdd.ShowAs = options.ShowAs;
            newAndAdd.OnlineMeetingProvider = options.OnlineMeetingProvider;
            

            if (!string.IsNullOrEmpty(options.Body))
            {
                newAndAdd.Body = new GraphItemBody
                {
                    Content = options.Body,
                    ContentType = options.BodyContentType
                };
            }

            if (options.Start != DateTime.MinValue)
            {
                if (options.IsAllDay)
                {
                    newAndAdd.Start = new GraphDateTimeTimeZone
                    {
                        DateTime = GraphEventHelper.ConvertDateTimeToAllDayString(options.Start),
                        TimeZone = GraphEventHelper.GetEnumMemberValue(options.StartTimeZone)
                    };
                }
                else
                {
                    newAndAdd.Start = new GraphDateTimeTimeZone
                    {
                        DateTime = GraphEventHelper.ConvertDateTimeToRegularString(options.Start),
                        TimeZone = GraphEventHelper.GetEnumMemberValue(options.StartTimeZone)
                    };
                }
                
            }

            if (options.End != DateTime.MinValue)
            {
                if (options.IsAllDay)
                {
                    newAndAdd.End = new GraphDateTimeTimeZone
                    {
                        DateTime = GraphEventHelper.ConvertDateTimeToAllDayString(options.End),
                        TimeZone = GraphEventHelper.GetEnumMemberValue(options.EndTimeZone)
                    };
                }
                else 
                { 
                    newAndAdd.End = new GraphDateTimeTimeZone
                    {
                        DateTime = GraphEventHelper.ConvertDateTimeToRegularString(options.End),
                        TimeZone = GraphEventHelper.GetEnumMemberValue(options.EndTimeZone)
                    };
                }
            }

            if (options.Location != null)
            {
                newAndAdd.Location = GetGraphLocation(options.Location);
            }

            if (options.Locations != null && options.Locations.Count > 0)
            {
                var locations = new GraphLocationCollection();
                foreach (var location in options.Locations)
                {
                    locations.Add(GetGraphLocation(location));
                }
                newAndAdd.Locations = locations;
            }

            if (options.Attendees != null && options.Attendees.Count > 0)
            {
                var attendees = new GraphEventAttendeeCollection();
                foreach (var attendee in options.Attendees)
                {
                    var graphAttendee = new GraphEventAttendee
                    {
                        EmailAddress = new GraphEmailAddress
                        {
                            Address = attendee.EmailAddress,
                            Name = attendee.Name
                        },
                        Type = attendee.Type
                    };
                    attendees.Add(graphAttendee);
                }
                newAndAdd.Attendees = attendees;
            }

            if (options.Recurrence != null)
            {
                var recurrence = new GraphPatternedRecurrence();

                if (options.Recurrence.Pattern != null)
                {
                    recurrence.Pattern = new GraphRecurrencePattern
                    {
                        DayOfMonth = options.Recurrence.Pattern.DayOfMonth,
                        FirstDayOfWeek = options.Recurrence.Pattern.FirstDayOfWeek.ToString(),
                        DaysOfWeek = options.Recurrence.Pattern.DaysOfWeek != null ? options.Recurrence.Pattern.DaysOfWeek.Select(y => y.ToString()).ToList() : new List<string>(),
                        Index = options.Recurrence.Pattern.Index,
                        Month = options.Recurrence.Pattern.Month,
                        Interval = options.Recurrence.Pattern.Interval,
                        Type = options.Recurrence.Pattern.Type
                    };
                }

                if (options.Recurrence.Range != null)
                {
                    recurrence.Range = new GraphRecurrenceRange
                    {
                        StartDate = options.Recurrence.Range.StartDate,
                        EndDate = options.Recurrence.Range.EndDate,
                        NumberOfOccurrences = options.Recurrence.Range.NumberOfOccurences,
                        RecurrenceTimeZone = options.Recurrence.Range.RecurrenceTimeZone,
                        Type = options.Recurrence.Range.Type
                    };
                }

                newAndAdd.Recurrence = recurrence;
            }

            return await newAndAdd.AddAsync().ConfigureAwait(false) as GraphEvent;
        }

        private GraphLocation GetGraphLocation(EventLocationOptions location)
        {
            var graphLoc = new GraphLocation
            {
                LocationType = location.Type,
                DisplayName = location.DisplayName,
                LocationEmailAddress = location.LocationEmailAddress,
                LocationUri = location.LocationUri
            };
            if (location.Coordinates != null)
            {
                graphLoc.Coordinates = new GraphOutlookGeoCoordinates
                {
                    Latitude = location.Coordinates.Latitude,
                    Longitude = location.Coordinates.Longitude
                };
            }
            if (location.Address != null)
            {
                graphLoc.Address = new GraphPhysicalAddress
                {
                    Street = location.Address.Street,
                    State = location.Address.State,
                    PostalCode = location.Address.PostalCode,
                    CountryOrRegion = location.Address.CountryOrRegion,
                    City = location.Address.City
                };
            }

            return graphLoc;
        }

        public IGraphEvent Add(EventCreateOptions options)
        {
            return AddAsync(options).GetAwaiter().GetResult();
        }

        #endregion
    }
}
