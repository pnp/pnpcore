# Working with Hub Sites

In SharePoint, site collections support hub sites, that allow you to create logical groupings of multiple sites with a hub site.

 When using the PnP Core SDK, the [ISite interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) has methods to register, join, unjoin and unregister hub sites.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

A `PnPContext` always has a reference to the [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) of the underlying site collection. Using the site object the following operations can be called when working with hub sites:

## Get Hub Site Data

Firstly, you will need a connection to a site, then you can either call a method to get the current Site Hub data OR specify an Id for another HubSite ID, to get the details. This is shown in the example below:

```csharp

var hubResult = await context.Site.GetHubSiteData(site.HubSiteId);
var title = hubResult.Title;
var siteUrl = hubResult.SiteUrl;

```

## Register a hub site

This registers a site as the primary hub site, using this code to apply this to a site:

```csharp

ISite site = await context.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

if(!site.IsHubSite){
    var result = await site.RegisterHubSiteAsync();
}
    
```

## Join a hub site

This associates the site to an existing hub site, using this code to apply this to a site:

> [!Important]
> - Joining a hub site that lives in a geo location (so not the main location) require delegated permissions to work.
> - If joining the hub site requires approval then this will fail, use the `GetSharePointAdmin().ConnectToHubSiteAsync` methods from the [Admin library](admin-sharepoint-tenant.md) instead

```csharp
using (var contextPrimaryHub = await pnpContextFactory.CreateAsync("PrimaryHubSite"))
{
    // Get the primary hub site details
    ISite primarySite = await contextPrimaryHub.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

    // Associate group site to the hub
    using (var context = await pnpContextFactory.CreateAsync("AnyExistingSite"))
    {
        ISite assocSite = await context.Site.GetAsync(
            p => p.HubSiteId,
            p => p.IsHubSite);

        if(assocSite.HubSiteId == Guid.Empty){

            var resultJoin = await assocSite.JoinHubSiteAsync(primarySite.HubSiteId);

        }
    }
}

```

## Unjoin a hub site

> [!Important]
> Unjoining a hub site that lives in a geo location (so not the main location) require delegated permissions to work.

This removes the association on the site to an existing hub site, using this code to apply this to a site:

```csharp

ISite site = await context.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

if(assocSite.HubSiteId != Guid.Empty){
    var result = await site.UnJoinHubSiteAsync();
}

```

## Unregister a hub site

> [!Important]
> Unregistering a hub site does require delegated permissions to work.

This unregisters a site as the primary hub site, using this code to apply this to a site:

```csharp

ISite site = await context.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

if(site.IsHubSite){
    var result = await site.UnregisterHubSiteAsync();
}
```
