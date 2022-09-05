# Multi-Geo

The Core SDK Admin library provides Microsoft 365 Admin APIs for understanding the multi-geo setup of a tenant.

[!INCLUDE [Microsoft 365 Admin setup](fragments/setup-admin-m365.md)]

## Discover if a tenant is a multi-geo tenant

Most tenants are single geo, meaning their data lives in one region. If the tenant you're working with is multi-geo it's data is spread over two or more geo locations, for example the tenant can have SharePoint and Teams data hosted in Europe and in the US. Each geo location will have it's own Tenant Admin Center site, it's own personal and portal site hosts and more. If you want to find out if the tenant you're working with supports multiple geo's you can use the `IsMultiGeoTenant` methods:

```csharp
if (await context.GetMicrosoft365Admin().IsMultiGeoTenantAsync())
{
    // Enumerate geo locations and perform admin task per geo
}
else
{
    // Perform admin task for the tenant
}
```

## Listing the geo locations of a tenant

If you want to understand the multi-geo setup of a tenant then using the `GetMultiGeoLocations` methods will give you the needed details. These methods will return the key information per geo location in the tenant. The returned data includes:

- Data location code: a [code identifying a geo location](https://docs.microsoft.com/en-us/microsoft-365/enterprise/plan-for-multi-geo?view=o365-worldwide)
- SharePoint Tenant Admin Center url
- SharePoint Portal url
- SharePoint my site host url

Below sample shows how to use the `GetMultiGeoLocations` methods:

```csharp
foreach(var location in await context.GetMicrosoft365Admin().GetMultiGeoLocationsAsync())
{
    // Do admin work per geo location
}
```

## Creating new site collections in the right geo location

How to use PnP Core SDK to create site collections is described in the [Site Collections](admin-sharepoint-sites.md#creating-site-collections) page. When working in a multi-geo tenant it's important to control where a site collection will be created and therefore some additional steps have to be taken in account. These steps depend per category of sites and used permissions:

Category | Delegated permissions | Application permissions
---------|-----------------------|------------------------
Modern, no group | Ensure the `context` you use is for a site in the target geo location + specify URL valid for the target geo location | Ensure the `context` you use is for a site in the target geo location + specify URL valid for the target geo location
Modern, with group | Ensure the `context` you use is for a site in the target geo location + set the `PreferredDataLocation` value in the `TeamSiteOptions` | Set the `PreferredDataLocation` value in the `TeamSiteOptions`
Classic site | Ensure the `context` you use is for a site in the target geo location + specify URL valid for the target geo location | Ensure the `context` you use is for a site in the target geo location + specify URL valid for the target geo location

So for modern sites without group and classic sites the story is simple: ensure the context and new site url you specify are valid for the target geo location. For group connected sites the story is different as the group's location determines where the associated SharePoint site will be created. To correctly do that you need to specify the `PreferredDataLocation`:

```csharp
teamSiteToCreate = new TeamSiteOptions(alias, "PnP Core SDK Test")
{
    Description = "This is a test site collection",
    Language = Language.English,
    IsPublic = true,
    PreferredDataLocation = GeoLocation.NAM
};

// Ensure the used context is for a site in NAM geo location
using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(teamSiteToCreate))
{
    // Do work on the created site collection via the newSiteContext
}
```

If you don't specify the `PreferredDataLocation` during the creation then the preferred data location from the account creating the team site (if any) is taken, if not the site will be created in the default geo location.