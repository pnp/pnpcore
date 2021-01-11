# Working with Teams: Introduction

The PnP Core SDK provides support for working with Microsoft Teams, in this set of articles, you will learn to use the SDK to interact with the service to work with Teams, Channels, Tabs, Chat Messages and more.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

When you want to get a Team, the SDK uses the associated Office 356 group ID attached to the current SharePoint Site to retrieve the Team from the Graph, you must connect to an existing site first that is associated to a Microsoft 365 group and has been 'Teamified'.

## Loading existing Team

Getting a Team can be achieved with the following:

```csharp
 var team = await context.Team.GetAsync();
```

If a Team is not set for the site referenced in the context, you will get an exception.

## Getting Team Settings

Each Team has settings to control which features are enabled. The following code shows examples of accessing those:

```csharp
var team = await context.Team.GetAsync(x => x.FunSettings);

var giphyRating = team.FunSettings.GiphyContentRating;
var allowStickersAndMemes =  team.FunSettings.AllowStickersAndMemes;
var allowCustomMemes = team.FunSettings.AllowCustomMemes;
var allowGiphy = team.FunSettings.AllowGiphy;
```

To retrieve the other properties, include the property within the expression in the 'GetAsync' call to add more properties to include.

> [!NOTE]
> The **Classification** property will always return null, the Microsoft Graph does not currently return a value, however, you can retrieve this by finding the property of the underlying Microsoft 365 group.


## Updating Team Settings

You can update the settings of a Team, the following code example shows updating the Fun Settings for a Team, this principle can be used for the other properties.

```csharp
 var team = await context.Team.GetAsync(x => x.FunSettings);

team.FunSettings.GiphyContentRating = TeamGiphyContentRating.Moderate;
team.FunSettings.AllowStickersAndMemes = true;
team.FunSettings.AllowCustomMemes = true;
team.FunSettings.AllowGiphy = true;

await team.UpdateAsync();

```

> [!NOTE]
> The **DiscoverySettings** property cannot be updated. The Microsoft Graph does not currently support this operation.
