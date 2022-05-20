# Working with Teams: Events

Within Teams, you can have team events (meetings). This page will show you how you can Get, Create, Update and Delete events.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Events

Events is a collection part of the ITeam interface, so when you get a team, you can include the events on the request.

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Events);

// Get the Events
var events = team.Events;
```

## Creating Events

To add a new event, call the Add method, specifying atleast a subject.

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

string eventSubject = $"Test event - PnP Rocks!";

await events.AddAsync(new EventCreateOptions
{
    Subject = eventSubject
});
```

## Adding an event with multiple attendees and a start time and end time

We can create an event using the PnP Core SDK and add multiple attendees to the event. These attendees can either be set _required_, _optional_ or _resource_.

We can also choose to hide the attendees of an event. This can be done by setting the property _HideAttendees_ to true. It defaults to _false_.

Next, we will add a start and end time for the event with a timezone.

The code below will create an event which will last for one hour and will have two attendees. The attendees will be hidden.

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

// Get the users to add to the event
await context.Web.GetAsync(y => y.SiteUsers);

var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();
var secondUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).Skip(1).First();

await events.AddAsync(new EventCreateOptions
{
    Subject = "Test event - PnP Rocks!",
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
    Start = new DateTime(2022, 5, 12, 14, 0, 0),
    StartTimeZone = EventTimeZone.PacificStandardTime,
    End = new DateTime(2022, 5, 12, 15, 0, 0),
    EndTimeZone = EventTimeZone.PacificStandardTime,
});
```

## Adding an event that lasts all day

Using the PnP Core SDK, we can add an event that will last all day.

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

await events.AddAsync(new EventCreateOptions
{
    Subject = "CreateTeamEventThatLastsAllDayAsync",
    IsAllDay = true,
    Start = DateTime.Now,
    End = DateTime.Now.AddDays(1)
});
```

## Adding an event with a location

We can create an event using the PnP Core SDK and add a location to the event.

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

await events.AddAsync(new EventCreateOptions
{
    Subject = "Test event - PnP Rocks!",
    Location = new EventLocationOptions
    {
        Type = EventLocationType.Default,
        DisplayName = "PnP Test location",
        Address = new EventAddressOptions
        { 
            City = "Brussels",
            State = "Bussels",
            PostalCode = "1000",
            CountryOrRegion = "BE",
            Street = "Wetstraat"
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
});
```

## Adding a recurring event

Using the PnP Core SDK, we can create a recurring event. Below, we will create a recurring event that will occur weekly, on every monday. This event will recur for exactly one year, starting today.

```csharp

// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

await events.AddAsync(new EventCreateOptions
{
    Subject = "Test event - PnP Rocks!",
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
};)

```

For more information regarding the creation of recurring events, please have a look at the following Graph documentation page: [Recurrence Pattern documentation](https://docs.microsoft.com/en-us/graph/api/resources/patternedrecurrence?view=graph-rest-1.0). This page will list which combinations are possible and which properties are required when e.g. creating a recurrence with a recurrencerange of type 'EndDate'

You also can always check the tests that have been written starting from line 329 to 447 for more smaples: [Link to tests](https://github.com/pnp/pnpcore/blob/dev/src/sdk/PnP.Core.Test/Teams/TeamEventTests.cs#L329-L447)

## Adding an online event with some more properties being set

In the following example we will create a Teams meeting. There will also be other properties being set such as the priority, the sensitivity of the event and the status on how the presence of the user attending the meeting will be shown.

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

await events.AddAsync(new EventCreateOptions
{
    Subject = "Test event - PnP Rocks",
    AllowNewTimeProposals = true,
    IsOnlineMeeting = true,
    OnlineMeetingProvider = EventOnlineMeetingProvider.TeamsForBusiness,
    Sensitivity = EventSensitivity.Personal,
    ShowAs = EventShowAs.Busy,
    Importance = EventImportance.High
});

```

For all the possible properties that can be set on creating an event, please refer to following page: [Event Create Options](https://pnp.github.io/pnpcore/api/PnP.Core.Model.Teams.EventCreateOptions.html).

## Updating events

You can update the events by changing the properties you wish update and call the update method. The events have to be a part of the class [EventUpdateOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.Teams.EventUpdateOptions.html).

The following sample will update the attendees and the subject of an event.

```csharp

// Get the Team
var team = await context.Team.GetAsync(p => p.Events);

// Get the events
var events = team.Events;

// Create an event to update

var firstUser = context.Web.SiteUsers.Where(y => y.PrincipalType == Model.Security.PrincipalType.User).First();

var eventOptions = new EventCreateOptions
{
    Subject = "Test event - PnP Rocks",
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

// Add the event (which we will update)
var newEvent = await context.Team.Events.AddAsync(eventOptions);

// Update the event
await newEvent.UpdateAsync(new EventUpdateOptions
{
    Subject = "Test event - PnP Rocks - Updated",
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
});

```

## Deleting Events

You can delete the event with the following example:

```csharp
var team = await context.Team.GetAsync(p => p.Events);

var event = await team.Events;

string eventSubject = $"Test event - PnP Rocks";

// Get the event you wish to delete
var eventToDelete = team.Events.Where(p => p.DisplayName == eventSubject).FirstOrDefault();

if(eventToDelete != default)
{    
    // Perform the delete operation
    await eventToDelete.DeleteAsync();
}
```
