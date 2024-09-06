# Modernize your sites

The Core SDK Admin library provides SharePoint Modernization APIs that allow you to "upgrade" classic sites to become modern Team or Communication sites. To learn more about modernizing your SharePoint sites checkout https://aka.ms/sharepoint/modernization.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Connecting a site to a new Microsoft 365 group

By default only new team sites are connected to a Microsoft 365 group, but what if you'd wanted to connect your existing classic team sites to a Microsoft 365 group? A reason to do this would be for example the need to set up a Teams team for the site or use any other Microsoft 365 service that's connected to a Microsoft 365 group. Luckily you can take an existing site, create a new Microsoft 365 group for it, and hook it up to site. To do this you have to use one of the `ConnectSiteCollectionToGroup` methods: these methods take in an `ConnectSiteToGroupOptions` object defining the three mandatory properties: the url of the site that needs to be connected to group, the alias to use for the group and the display name to use for the group:

```csharp
ConnectSiteToGroupOptions groupConnectOptions = new ConnectSiteToGroupOptions(
    new Uri("https://contoso.sharepoint.com/sites/sitetogroupconnect"), 
    "sitealias", 
    "Site title");
await context.GetSiteCollectionManager().ConnectSiteCollectionToGroupAsync(groupConnectOptions);
```

> [!Important]
>
> - If in **SharePoint Tenant Admin** -> **Settings** -> **Site creation** you've not checked both boxes then connecting a site to a new Microsoft 365 Group can only work when the user is a SharePoint Administrator or Global Administrator.
> - Connecting a site to a new group does not work using application permissions.

## Control the "Add Teams" prompt

Microsoft 365 group connected sites by default show a prompt in their left navigation to enable the creation of Team. Sometimes you might not want your users to create a Team or you do want to create the Team programmatically. For those cases you can use the `IsAddTeamsPromptHidden` and `HideAddTeamsPrompt` methods:

```csharp
// Check if the Add Teams prompt is hidden
var isAddTeamsPromptHidden = await context.GetSiteCollectionManager().IsAddTeamsPromptHiddenAsync(
    new Uri("https://contoso.sharepoint.com/sites/sitetogroupconnect"));

if (!isAddTeamsPromptHidden)
{
    // Hide the Add Teams prompt
    await context.GetSiteCollectionManager().HideAddTeamsPromptAsync(
        new Uri("https://contoso.sharepoint.com/sites/sitetogroupconnect"));
}
```

## Create a Team for a Microsoft 365 Group connected site

Once a site collection is connected to a Microsoft 365 group it's eligible to be "teamified", so creating a Team linked to the site's Microsoft 365 group. To do so, the PnP Core SDK Teams admin API features are needed, more specifically the `CreateTeam` methods.

```csharp
Guid groupId = Guid.Parse("");
using (var contextWithTeam = await context.GetTeamManager().CreateTeamAsync(
    new TeamForGroupOptions(groupId)))
{
    // Post a message in the Teams general channel
    await context.Team.LoadAsync(p => p.PrimaryChannel);
    await context.Team.PrimaryChannel.LoadAsync(p => p.Messages);
    await context.Team.PrimaryChannel.Messages.AddAsync("Hi Teams!");     
}
```

## Enabling communication site features

Connecting to a Microsoft 365 group and possible also adding a Team is one way to modernize your classic team sites, but you can also turn a classic team site into a communication site by enabling the communication site features. Communication site features can be enabled on a site collection assuming that:

- The site collection is not connected to a Microsoft 365 group
- The site collection's template is STS#0 or EHS#1

If above requirements are met you can use the `EnableCommunicationSiteFeatures` methods to transform your classic site into a communication site.

```csharp
// Enable the communication site features on this classic site, uses Topic design package (default)
await context.GetSiteCollectionManager().EnableCommunicationSiteFeaturesAsync(context.Uri);

// Enable the communication site features on this classic site, uses Showcase design package
await context.GetSiteCollectionManager().EnableCommunicationSiteFeaturesAsync(context.Uri, 
    Guid.Parse("6142d2a0-63a5-4ba0-aede-d9fefca2c767"));
```
