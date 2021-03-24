# Working with Teams: Channels

Within Teams, you have one or more channels. This page will show you how you can Get, Create, Update and Delete channels.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Channels

Channels is a collection part of the ITeam interface, so when you get a team, you can include the channels on the request. 

```csharp
// Get the Team
 var team = await context.Team.GetAsync(o => o.Channels);

// Get the Channels
 var channels = team.Channels;
```

## Getting Channels with specific properties

When you want to load the Team and Channels with specific properties populated, you can specify which properties to include by:

```csharp
// Get the Team
 var team = await context.Team.GetAsync(
    p => p.Channels.QueryProperties(p => p.DisplayName));
```

### Getting a specific Channel

If you would like to get a specific channel you can use the extension method GetByDisplayNameAsync to get the channel by name.

```csharp
// Get the Team
 var team = await context.Team.GetAsync(o => o.Channels);

// Get the Channel 
var generalChannel = await team.Channels.GetByDisplayNameAsync("General");
```

### Getting the default channel

Alternatively, if you are looking for the default channel within a Team, you can retrieve this with the following:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.PrimaryChannel);
 
// Get the primary channel
var primaryChannel = await team.PrimaryChannel;
```

## Creating Channels

To add a new channel, call the Add method, specifying a name and optionally a description.

```csharp
var team = await context.Team.GetAsync(p => p.Channels);

// Get the Team
string channelName = $"My Cool New Channel";

// Check if the channel exists
var channelFound = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();
if (channelFound == null)
{
    // Add a new channel
    channelFound = await team.Channels.AddAsync(channelName, "This is my cool new Channel, check this out!");
}
```

> [!TIP]
> When working with channels these are some tips to help:
> * Channel Names must be unique
> * Check if the channel already exists, to avoid trying to add a Channel with an existing name


## Updating Channels

You can update the channel by changing the properties you wish update and call the update method:

```csharp

// Get the Team
var team = await context.Team.GetAsync(p => p.Channels);

string channelName = $"My Cool New Channel";

// Get the channel you wish to update
var channelToUpdate = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();

if(channelToUpdate != default){

    string newChannelDescription = $"This cool channel is being updated!";
    channelToUpdate.Description = newChannelDescription;
    
    // Perform the update to the channel    
    await channelToUpdate.UpdateAsync();
}
```

## Deleting Channels

You can delete the channel with the following example:

```csharp
var team = await context.Team.GetAsync(p => p.Channels);

string channelName = $"My Cool New Channel";

// Get the channel you wish to delete
var channelToDelete = team.Channels.Where(p => p.DisplayName == channelName).FirstOrDefault();

if(channelToDelete != default){
    
    // Perform the delete operation
    await channelToUpdate.DeleteAsync();
}
```

> [!Note]
> You cannot delete the General Channel, this operation is not supported by the service.
Additionally, channels are soft-deleted for 30 days, before which counts towards the total limit of channels per Team, for further details on the limits, please refer to [Limits and specifications for Microsoft Teams](https://docs.microsoft.com/en-us/microsoftteams/limits-specifications-teams)