# Working with Hub Sites

In SharePoint, site collections support hub sites, that allow you to create logical groupings of multiple sites with a hub site.

 When using the PnP Core SDK, the [ISite interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) has methods to register, join, unjoin and unregister hub sites.

[!INCLUDE [Creating Context](fragments/creating-context.md)]



A `PnPContext` always has a reference to the [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) of the underlying site collection. Using the site object the following operations can be called when working with hub sites:

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

> [!Note]
> Ignore the section "creating a context", this is included in the code below.

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

        if(!assocSite.IsHubSite){

            var resultJoin = await assocSite.JoinHubSiteAsync(primarySite.HubSiteId);

        }
    }
}

```


## Unjoin a hub site

This removes the association on the site to an existing hub site, using this code to apply this to a site:

```csharp

ISite site = await context.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

if(site.IsHubSite){
    var result = await site.UnJoinHubSiteAsync();
}

```


## Unregister a hub site

This unregisters a site as the primary hub site, using this code to apply this to a site:

```csharp

ISite site = await context.Site.GetAsync(
        p => p.HubSiteId,
        p => p.IsHubSite);

if(site.IsHubSite){
    var result = await site.UnregisterHubSiteAsync();
}

```