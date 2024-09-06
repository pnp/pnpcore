using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamEventTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTeamEventsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Events);
                Assert.IsNotNull(team.Events);
            }
        }

        #region Create tests

        [TestMethod]
        public async Task CreateTeamEventWithOnlySubjectAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithOnlySubjectAsyncTest"
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithMultipleAttendeesAndSubjectAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(y => y.SiteUsers);

                var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();
                var secondUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).Skip(1).First();

                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithMultipleAttendeesAndSubjectAsyncTest",
                    Attendees = new List<EventAttendeeOptions>
                    {
                        new EventAttendeeOptions
                        {
                            EmailAddress = firstUser.Mail,
                            Name = firstUser.Title,
                            Type = EventAttendeeType.Optional
                        },
                        new EventAttendeeOptions
                        {
                            EmailAddress = secondUser.Mail,
                            Name = secondUser.Title,
                            Type = EventAttendeeType.Required
                        }
                    }
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(newEvent.Attendees.Length, 2);

                Assert.AreEqual(newEvent.Attendees.FirstOrDefault().EmailAddress.Name, firstUser.Title);
                Assert.AreEqual(newEvent.Attendees.FirstOrDefault().EmailAddress.Address, firstUser.Mail);
                Assert.AreEqual(newEvent.Attendees.FirstOrDefault().Type, EventAttendeeType.Optional);

                Assert.AreEqual(newEvent.Attendees.LastOrDefault().EmailAddress.Name, secondUser.Title);
                Assert.AreEqual(newEvent.Attendees.LastOrDefault().EmailAddress.Address, secondUser.Mail);
                Assert.AreEqual(newEvent.Attendees.LastOrDefault().Type, EventAttendeeType.Required);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithLocationAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Events);

                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithLocationAsync",
                    Location = new EventLocationOptions
                    {
                        Type = EventLocationType.Default,
                        DisplayName = "PnP Test location",
                        Address = new EventAddressOptions
                        { 
                            City = "Vosselaar",
                            State = "Antwerp",
                            PostalCode = "2350",
                            CountryOrRegion = "BE",
                            Street = "Dorp"
                        },
                        Coordinates = new EventCoordinateOptions
                        {
                            Latitude = 20.00,
                            Longitude = 30.00,
                            Accuracy = 5,
                            Altitude = 20,
                            AltitudeAccuracy = 2.00
                        }
                    }
                };

                var newEvent = await team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(newEvent.Location.DisplayName, eventOptions.Location.DisplayName);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithMultipleLocationsAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Events);

                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithMultipleLocationsAsync",
                    Locations = new List<EventLocationOptions>
                    { 
                        new EventLocationOptions
                        {
                            DisplayName = "Location 1"
                        },
                        new EventLocationOptions
                        {
                            DisplayName = "Location 2"
                        }
                    }
                };

                var newEvent = await team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(newEvent.Locations.Length, 2);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithMultipleAttendeesAndHideThemAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(y => y.SiteUsers);

                var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();
                var secondUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).Skip(1).First();

                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithMultipleAttendeesAndHideThemAsyncTest",
                    Attendees = new List<EventAttendeeOptions>
                    {
                        new EventAttendeeOptions
                        {
                            EmailAddress = firstUser.Mail,
                            Name = firstUser.Title,
                            Type = EventAttendeeType.Optional
                        },
                        new EventAttendeeOptions
                        {
                            EmailAddress = secondUser.Mail,
                            Name = secondUser.Title,
                            Type = EventAttendeeType.Required
                        }
                    },
                    HideAttendees = true
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(newEvent.Attendees.Length, 2);

                Assert.AreEqual(newEvent.HideAttendees, true);
                
                // Verified that this was working

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithMultipleAttendeesAndMultipleOptionsAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync(y => y.SiteUsers);

                var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();
                var secondUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).Skip(1).First();

                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithMultipleAttendeesAndMultipleOptionsAsync",
                    Attendees = new List<EventAttendeeOptions>
                    {
                        new EventAttendeeOptions
                        {
                            EmailAddress = firstUser.Mail,
                            Name = firstUser.Title,
                            Type = EventAttendeeType.Optional
                        },
                        new EventAttendeeOptions
                        {
                            EmailAddress = secondUser.Mail,
                            Name = secondUser.Title,
                            Type = EventAttendeeType.Required
                        }
                    },
                    HideAttendees = true,
                    AllowNewTimeProposals = true,
                    IsOnlineMeeting = true,
                    OnlineMeetingProvider = EventOnlineMeetingProvider.TeamsForBusiness,
                    Sensitivity = EventSensitivity.Personal,
                    ShowAs = EventShowAs.Busy,
                    Importance = EventImportance.High
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(newEvent.Attendees.Length, 2);

                Assert.AreEqual(newEvent.HideAttendees, true);
                Assert.AreEqual(newEvent.AllowNewTimeProposals, true);
                Assert.AreEqual(newEvent.IsOnlineMeeting, true);
                Assert.AreEqual(newEvent.OnlineMeetingProvider, EventOnlineMeetingProvider.TeamsForBusiness);
                Assert.AreEqual(newEvent.Sensitivity, EventSensitivity.Personal);
                Assert.AreEqual(newEvent.Importance, EventImportance.High);
                Assert.AreEqual(newEvent.ShowAs, EventShowAs.Busy);

                Assert.IsNotNull(newEvent.OnlineMeeting.JoinUrl);
               
                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventWithStartAndEndDateAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventWithStartAndEndDateAsync",
                    Start = new DateTime(2022, 5, 12, 14, 0, 0),
                    StartTimeZone = EventTimeZone.PacificStandardTime,
                    End = new DateTime(2022, 5, 12, 15, 0, 0),
                    EndTimeZone = EventTimeZone.PacificStandardTime,
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);

                Assert.AreEqual(eventOptions.Start.ToString("yyyy-MM-ddTHH:mm:ss.0000000", CultureInfo.InvariantCulture), newEvent.Start.DateTime);
                Assert.AreEqual(eventOptions.End.ToString("yyyy-MM-ddTHH:mm:ss.0000000", CultureInfo.InvariantCulture), newEvent.End.DateTime);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventThatLastsAllDayAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {  
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventThatLastsAllDayAsync",
                    IsAllDay = true,
                    Start = DateTime.Now,
                    End = DateTime.Now.AddDays(1)
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(eventOptions.IsAllDay, true);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventRecurrencePatternTypeWeeklyAndRangeTypeEndDateAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {   
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrencePatternTypeWeeklyAndRangeTypeEndDateAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Pattern = new EventRecurrencePatternOptions
                        {
                            Type = EventRecurrenceType.Weekly,
                            Interval = 1,
                            DaysOfWeek = new List<DayOfWeek>
                            {
                                DayOfWeek.Monday
                            }
                        },
                        Range = new EventRecurrenceRangeOptions
                        {
                            Type = EventRecurrenceRangeType.EndDate,
                            EndDate = DateTime.Now.AddYears(1),
                            StartDate = DateTime.Now
                        }
                    }
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(eventOptions.IsAllDay, false);
                Assert.AreEqual(eventOptions.Recurrence.Pattern.Type, newEvent.Recurrence.Pattern.Type);
                Assert.AreEqual(eventOptions.Recurrence.Range.Type, newEvent.Recurrence.Range.Type);

                // Hard to verify everything, but have verified it in Outlook and it's working as intended

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventRecurrencePatternTypeDailyAndRangeTypeNoEndDateAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // This event will happen every two days
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrencePatternTypeDailyAndRangeTypeNoEndDateAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Pattern = new EventRecurrencePatternOptions
                        {
                            Type = EventRecurrenceType.Daily,
                            Interval = 2
                        },
                        Range = new EventRecurrenceRangeOptions
                        {
                            Type = EventRecurrenceRangeType.NoEnd,
                            StartDate = DateTime.Now
                        }
                    }
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(eventOptions.IsAllDay, false);
                Assert.AreEqual(eventOptions.Recurrence.Pattern.Type, newEvent.Recurrence.Pattern.Type);
                Assert.AreEqual(eventOptions.Recurrence.Range.Type, newEvent.Recurrence.Range.Type);

                // Hard to verify everything, but have verified it in Outlook and it's working as intended

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task CreateTeamEventRecurrencePatternTypeAbsoluteMonthlyAndRangeTypeNumberedDateAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            { 
                // This event will happen every two days
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrencePatternTypeAbsoluteMonthlyAndRangeTypeNumberedDateAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Pattern = new EventRecurrencePatternOptions
                        {
                            Type = EventRecurrenceType.AbsoluteMonthly,
                            Interval = 2,
                            DayOfMonth = 10
                        },
                        Range = new EventRecurrenceRangeOptions
                        {
                            Type = EventRecurrenceRangeType.Numbered,
                            StartDate = DateTime.Now,
                            NumberOfOccurences = 10
                        }
                    }
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);
                Assert.AreEqual(eventOptions.IsAllDay, false);
                Assert.AreEqual(eventOptions.Recurrence.Pattern.Type, newEvent.Recurrence.Pattern.Type);
                Assert.AreEqual(eventOptions.Recurrence.Range.Type, newEvent.Recurrence.Range.Type);

                // Hard to verify everything, but have verified it in Outlook and it's working as intended

                await newEvent.DeleteAsync();
            }
        }

        #endregion

        #region Update tests

        [TestMethod]
        public async Task UpdateTeamEventSubjectAndBodyAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "UpdateTeamEventSubjectAndBodyAsyncTest",
                    Body = "PnP Test body",
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                Assert.AreEqual(eventOptions.Subject, newEvent.Subject);

                var updateOptions = new EventUpdateOptions
                {
                    Subject = "PnP Rocks - updated",
                    Body = "PnP Test body - updated"
                };

                await newEvent.UpdateAsync(updateOptions);

                await context.Team.LoadAsync(o => o.Events);
                
                var updatedEvent = context.Team.Events.AsRequested().FirstOrDefault(o => o.Id == newEvent.Id);

                Assert.AreEqual(updateOptions.Subject, updatedEvent.Subject);
                Assert.IsTrue(updatedEvent.Body.Content.Contains(updateOptions.Body));

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateTeamEventAttendeesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();
                var secondUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).Skip(1).First();

                var eventOptions = new EventCreateOptions
                {
                    Subject = "UpdateTeamEventAttendeesAsyncTest",
                    Attendees = new List<EventAttendeeOptions>
                    {
                        new EventAttendeeOptions
                        {
                            EmailAddress = firstUser.Mail,
                            Name = firstUser.Title,
                            Type = EventAttendeeType.Optional
                        }
                    }
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                var updateOptions = new EventUpdateOptions
                {
                    Attendees = new List<EventAttendeeOptions>
                    {
                        new EventAttendeeOptions
                        {
                            EmailAddress = firstUser.Mail,
                            Name = firstUser.Title,
                            Type = EventAttendeeType.Optional
                        },
                        new EventAttendeeOptions
                        {
                            EmailAddress = secondUser.Mail,
                            Name = secondUser.Title,
                            Type = EventAttendeeType.Required
                        }
                    }
                };

                await newEvent.UpdateAsync(updateOptions);

                await context.Team.LoadAsync(o => o.Events);

                var updatedEvent = context.Team.Events.AsRequested().FirstOrDefault(o => o.Id == newEvent.Id);

                Assert.AreEqual(updateOptions.Attendees.Count, updatedEvent.Attendees.Length);

                await newEvent.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task UpdateTeamEventAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "UpdateTeamEventAsyncTest"
                };

                var newEvent = await context.Team.Events.AddAsync(eventOptions);

                var updateOptions = new EventUpdateOptions
                {
                    Location = new EventLocationOptions
                    {
                        DisplayName = "Location 1"
                    },
                    IsOnlineMeeting = true,
                    Start = new DateTime(2022, 05, 12),
                    End = new DateTime(2022, 05, 13),
                    IsAllDay = true,
                    HideAttendees = true,
                    AllowNewTimeProposals = true,
                    Importance = EventImportance.High,
                    Sensitivity = EventSensitivity.Confidential,
                    OnlineMeetingProvider = EventOnlineMeetingProvider.TeamsForBusiness,
                    ResponseRequested = true,
                    ShowAs = EventShowAs.Oof
                };

                await newEvent.UpdateAsync(updateOptions);

                await context.Team.LoadAsync(o => o.Events);

                var updatedEvent = context.Team.Events.AsRequested().FirstOrDefault(o => o.Id == newEvent.Id);

                Assert.AreEqual(updateOptions.Location.DisplayName, updatedEvent.Location.DisplayName);

                Assert.AreEqual(updateOptions.IsOnlineMeeting, updatedEvent.IsOnlineMeeting);
                Assert.AreEqual(updateOptions.OnlineMeetingProvider, updatedEvent.OnlineMeetingProvider);
                Assert.AreEqual(updateOptions.Sensitivity, updatedEvent.Sensitivity);
                Assert.AreEqual(updateOptions.Importance, updatedEvent.Importance);

                Assert.AreEqual(updateOptions.ShowAs, updatedEvent.ShowAs);
                Assert.AreEqual(updatedEvent.IsAllDay, true);
                Assert.AreEqual(updatedEvent.HideAttendees, true);
                Assert.AreEqual(updatedEvent.AllowNewTimeProposals, true);
                Assert.AreEqual(updatedEvent.ResponseRequested, true);

                Assert.IsNotNull(updatedEvent.OnlineMeeting.JoinUrl);

                await newEvent.DeleteAsync();
            }

        }

        #endregion

        #region Exception tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateTeamEventRecurrencePatternTypeExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrencePatternTypeExceptionTestAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Pattern = new EventRecurrencePatternOptions
                        {
                            Type = EventRecurrenceType.Daily,
                            Interval = -10,
                        }
                    }
                };
                await context.Team.Events.AddAsync(eventOptions);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateTeamEventRecurrenceRangeExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrenceRangeExceptionTestAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Range = new EventRecurrenceRangeOptions
                        {
                            Type = EventRecurrenceRangeType.EndDate,
                            StartDate = DateTime.Today
                        }
                    }
                };
                await context.Team.Events.AddAsync(eventOptions);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateTeamEventRecurrenceRangeNoStartDateExceptionTestAsync()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var eventOptions = new EventCreateOptions
                {
                    Subject = "CreateTeamEventRecurrenceRangeNoStartDateExceptionTestAsync",
                    Recurrence = new EventRecurrenceOptions
                    {
                        Range = new EventRecurrenceRangeOptions
                        {
                            Type = EventRecurrenceRangeType.NoEnd
                        }
                    }
                };
                await context.Team.Events.AddAsync(eventOptions);
            }
        }

        #endregion
    }
}
