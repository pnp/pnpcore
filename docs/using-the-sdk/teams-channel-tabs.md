# Working with Teams: Channel Tabs

The Core SDK provides support for working with Microsoft Teams channel tabs allowing you to retrieve, create, update and delete tabs from the channel.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Channel Tabs

To load the channel tabs, the SharePoint site must be associated with a Team in the context, then load the Team and the Channel. From here you can load all the tabs to allow you to work with the existing tabs, see this example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.Where(i => i.DisplayName == "General").FirstOrDefault();

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
var channel = await team.Channels.Where(i => i.DisplayName == "General").FirstOrDefaultAsync();

// Load the channel tab collection
channel = await channel.GetAsync(o => o.Tabs);

var siteDocLib = $"{context.Uri.OriginalString}/Shared%20Documents";
var tabName = "Important Documents";

// Perform the add tab operation 
var newDocTab = channel.Tabs.AddDocumentLibraryTabAsync(tabName, new Uri(siteDocLib));
```

## Updating Channel Tabs

Channel Tabs can be updated using the standard Update() method, see code example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);
               
// Get the Channel "General" 
var channel = await team.Channels.Where(i => i.DisplayName == "General").FirstOrDefaultAsync();
channel = await channel.GetAsync(o => o.Tabs);

var tabName = "Important Documents";
var tab = channel.Tabs.AsRequested().FirstOrDefault(i => i.DisplayName == tabName);

// Update the display name of the tab
tab.DisplayName = "Most Important Documents";

// Perform the update operation
await tab.Update();
```

## Deleting Channel Tabs

You can delete the channel tab by getting a reference to the channel tab and running the standard delete operation in the code example below:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the Channel "General"                
var channel = await team.Channels.Where(i => i.DisplayName == "General").FirstOrDefaultAsync();

// Get the Tabs
channel = await channel.GetAsync(o => o.Tabs);
var tab = channel.Tabs.AsRequested().FirstOfDefault(i => i.DisplayName == "Important Documents");
if (tab != default)
{
    // Perform the delete operation
    await tab.DeleteAsync();
}
```
