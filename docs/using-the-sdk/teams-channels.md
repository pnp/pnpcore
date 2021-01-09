# Working with Teams: Channels

Within Teams, you have one or more channels. This page will show you how you can Get, Create, Update and Delete channels.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Channels

Channels is a collection part of the ITeam interface, so when you get a team, you can include the channels on the request. 

```csharp
 var team = await context.Team.GetAsync(o => o.Channels);
 var channels = team.Channels;
```

If you would like to get a specific channel you can use the extension method GetByDisplayNameAsync to get the channel by name.

```csharp
 var team = await context.Team.GetAsync(o => o.Channels);
 var channel = await team.Channels.GetByDisplayNameAsync("General");
```

## Getting Channels with specific properties

When you want to load the Team and Channels with specific properties populated, you can specify which properties to include by:

```csharp
 var team = await context.Team.GetAsync(
    p => p.Channels.LoadProperties(p => p.DisplayName));

```

## Creating Channels


## Updating Channels


## Deleting Channels

[!NOTE]
You cannot delete the General Channel. 