# Working with Teams: Channel Tabs

The Core SDK provides support for working with Microsoft Teams channel tabs allowing you to retrieve, create, update and delete tabs from the channel.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Channel Tabs

To load the channel tabs, the SharePoint site must be associated with a Team in the context, then load the Team and the Channel. From here you can load all the tabs to allow you to work with the existing tabs, see this example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

// Load the tabs in the channel
channel = await channel.GetAsync(o => o.Tabs);

// Collection of Tabs
var tabs = channel.Tabs;

```

## Creating Channel Tabs

The following code is an example of creating a channel tab that links to a document library:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);
               
// Get the Channel "General" 
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

// Load the channel tab collection
channel = await channel.GetAsync(o => o.Tabs);

var siteDocLib = $"{context.Uri.OriginalString}/Shared%20Documents";
var tabName = "Important Documents";
var newDocTab = channel.Tabs.AddDocumentLibraryTabAsync(tabName, new Uri(siteDocLib));

```

## Updating Channel Tabs

Channel Tabs can be updated using the standard Update() method, see code example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);
               
// Get the Channel "General" 
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");
channel = await channel.GetAsync(o => o.Tabs);

var tabName = "Important Documents";
var tab = channel.Tabs.FirstOrDefault(i => i.DisplayName == tabName);

// Update the display name of the tab
tab.DisplayName = "Most Important Documents";
await tab.Update();

```

## Deleting Channel Tabs

You can delete the channel tab by getting a reference to the channel tab and running the standard delete operation in the code example below:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the Channel "General"                
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

// Get the Tabs
channel = await channel.GetAsync(o => o.Tabs);
var tab = channel.FirstOfDefault(i=>i.DisplayName == "Important Documents");
if (tab != default){
    await tab.DeleteAsync();
}
```