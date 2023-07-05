# Site Collections

The Core SDK Admin library provides SharePoint Admin security related APIs for enumerating, creating, updating and deleting site collections.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

> [!Note]
> If your tenant is using vanity URL's then you'll need to populate the `VanityUrlOptions` class and pass it to any method that allows it.

## Enumerate the site collections in a tenant

The typical SharePoint Online tenants contains hundreds of site collections and for quite often there's a need to perform admin tasks on all site collections, so being able to enumerate sites is important. The PnP Core SDK Admin component offers two approaches.

### Getting all site collections

Using the `GetSiteCollections` methods any user/app can enumerate site collections but depending on the user/app's permissions the results will be different:

- When using **application permissions** with `Sites.Read.All` the Microsoft Graph Sites endpoint is queried and **all** site collections in the tenant are returned
- When using **delegated permissions** with `Sites.Read.All` and a user that's not a SharePoint Tenant admin then the Microsoft Graph Search endpoint is queried to return all site collections that the user can access, so a **subset** of the site collections is returned
- When using **delegated permissions** and the user is a SharePoint Tenant administrator then a hidden list in the SharePoint Tenant Admin Center site is queried and **all** site collections in the tenant are returned

Below sample shows how to use the `GetSiteCollections` methods:

```csharp
// Get a list of site collections
var siteCollections = await context.GetSiteCollectionManager().GetSiteCollectionsAsync();
```

### Getting all site collections with details

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

Whereas above approach works for any user, the amount of information returned for a given site collection is limited. If you want to get site collection list with more details about each site collection then consider using the `GetSiteCollectionsWithDetails` or `GetSiteCollectionWithDetails` methods. The collected information per site collection is:

- The various id's such as site collection id, root web id and Microsoft Graph site id
- The site collection url
- The name of the site collection
- Creation time and creator
- External sharing information
- Site owner name and email
- Storage quota information
- Template details

This method also can optionally return the site collections connected to shared and private Microsoft Teams channels by setting the `includeSharedAndPrivateTeamChannelSites` parameter.

Below sample shows how to use the `GetSiteCollectionsWithDetails` methods:

```csharp
// Get a list of site collections with details about each site collection
var siteCollections = await context.GetSiteCollectionManager().GetSiteCollectionsWithDetailsAsync();

// Get a list of site collections with details about each site collection, including the site collections connected to shared and private Microsoft Teams channels
var siteCollections = await context.GetSiteCollectionManager().GetSiteCollectionsWithDetailsAsync(includeSharedAndPrivateTeamChannelSites: true);

// Get details for one given site collection
var siteToCheckDetails = await context.GetSiteCollectionManager().GetSiteCollectionWithDetailsAsync(new Uri("https://contoso.sharepoint.com/sites/sitetocheck"));
```

## Enumerate all webs in a site collection

If you want to know all the webs that exist in a site collection then you can use the `GetSiteCollectionWebsWithDetails` methods. These methods will return a list with information about **all** webs in the site collection, webs are enumerated recursively to ensure all webs are listed. By default app webs (being created as part of installed SharePoint add-ins) are skipped but these can optionally be included.

```csharp
var webs = await context.GetSiteCollectionManager().GetSiteCollectionWebsWithDetailsAsync();
foreach(var web in webs)
{
    // do something with the web
}
```

## Getting and setting site collection properties

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

A site collection has many properties which can only be set as a SharePoint Administrator. For example you can configure a site collection to not allow Power Automate Flows. Before site collection properties can be set you first need to get the current properties via on of the `GetSiteCollectionProperties` methods, followed by changing the properties you want to change and calling an `Update` method to send the change to SharePoint:

```csharp
// Get all the properties of this site collection
var siteProperties = await context.GetSiteCollectionManager().GetSiteCollectionPropertiesAsync(new Uri("https://contoso.sharepoint.com/sites/sitetocheck"));

// Update site properties
siteProperties.Title = "New site title";
siteProperties.DisableFlows = FlowsPolicy.Disabled;

// Send the changes back to the server
await siteProperties.UpdateAsync();
```

## Creating site collections

In SharePoint site collections can be split up into three categories:
- Modern, non group connected, sites (e.g. communication site)
- Modern group connected site collections (e.g. team site)
- Classic site collections (e.g. classic team site, classic publishing portal)

It's highly recommended to use one of the "modern" site collections as these offer more features and are faster to provision. When it comes to provisioning site collections then the site collection category and used authentication approach determine how a site collection is create and how long that will take. For the PnP Core SDK user all site collections are created via one of the `CreateSiteCollection` methods, but it's good to understand the needed `Options` as outlined in below table.

Category | Delegated permissions | Application permissions
---------|-----------------------|------------------------
Modern, no group | Use `CommunicationSiteOptions` or `TeamSiteWithoutGroupOptions` | Use `CommunicationSiteOptions` or `TeamSiteWithoutGroupOptions`. The `Owner` property must be set.
Modern, with group | Use `TeamSiteOptions`.  The Owners property must be set.  | Use `TeamSiteOptions`.  The `Owners` property must be set.
Classic site | Use `ClassicSiteOptions` | Use `ClassicSiteOptions`

All provisioning flows will only return once the site collection is done, for the modern sites this is a matter of seconds, for classic sites this can take up to 10-15 minutes.

> [!Note]
> If your tenant is a multi-geo tenant then go [here](admin-m365-multigeo.md) to checkout how you can control the geo location where the site collection will be created.

### Basic site collection creation flow

> [!Important]
> **When creating classic sites** you need to be either a SharePoint Administrator or Global Administrator to use these methods.

The code structure to create a site collection is identical, regardless of which site you're creating or which type of permission you're using:

```csharp
// Create communication site
var communicationSiteToCreate = new CommunicationSiteOptions(new Uri("https://contoso.sharepoint.com/sites/sitename"), "My communication site")
{
    Description = "My site description",
    Language = Language.English,
};

using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate))
{
    // Do work on the created site collection via the newSiteContext
};

// Create group connected team site, using application permissions
var teamSiteToCreate = new TeamSiteOptions("mynewsite", "My new site")
{
    Description = "My site description",
    IsPublic = true,
    Owners = new string[] { "ann@contoso.onmicrosoft.com" }
};

SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
{
    UsingApplicationPermissions = true
};

using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(teamSiteToCreate, siteCreationOptions))
{
    // Do work on the created site collection via the newSiteContext
};
```

So depending on what `Options` object you pass into the `CreateSiteCollection` method a different type of site collection will be created. The constructors of the respective `Options` classes will ensure you're providing the minimally needed information needed to create the site collection, additional input can always be provided via the other `Options` class attributes.

### Controlling the site collection creation behavior

The `CreateSiteCollection` methods accept an optional `SiteCreationOptions` instance that you can use to control the site collection creation flow. Following properties can be set:

Option | Default | Applies to | Description
-------|---------|------------|------------
`WaitAfterCreation` | not set | Modern + Classic | Defines the wait time in seconds after the site collection creation call returns. If specified this overrides the `WaitForAsyncProvisioning` setting
`WaitAfterStatusCheck` | 10 | Modern | The modern site provisioning might seldomly be queued, if so this property determines how many seconds the code waits between queue checks.
`MaxStatusChecks` | 12 | Modern | The modern site provisioning might seldomly be queued, if so this property determines how often the queue will be checked
`WaitForAsyncProvisioning` | not set | Modern | Modern sites are provisioned very fast, but there's an async completion that needs to happen. If you want to wait for the async provisioning logic to complete then set this value to `true`
`WaitAfterAsyncProvisioningStatusCheck` | 15 | Modern | If `WaitForAsyncProvisioning` is `true` then this property determines the wait time between the async provisioning status checks
`MaxAsyncProvisioningStatusChecks` | 80 | Modern | If `WaitForAsyncProvisioning` is `true` then this property determines the maximum amount of async provisioning status checks
`UsingApplicationPermissions` | Automatic check | Modern + Classic | By default `CreateSiteCollection` methods will check if application permissions are used, if you need to create multiple site collections your code can use one of the  `AccessTokenUsesApplicationPermissions` methods store the outcome in this property. This way the check only happens once.

Below sample creates a communication site and waits for the async provisioning to complete:

```csharp
var communicationSiteToCreate = new CommunicationSiteOptions(new Uri("https://contoso.sharepoint.com/sites/sitename"), "My communication site")
{
    Description = "My site description",
    Language = Language.English,
};

SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
{
    WaitForAsyncProvisioning = true
};

using (var newSiteContext = await context.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate, siteCreationOptions))
{
    // Do work on the created site collection via the newSiteContext
}
```

## Connecting a site to a new Microsoft 365 group

By default only new team sites are connected to a Microsoft 365 group, but what if you'd wanted to connect your existing classic team sites to a Microsoft 365 group? A reason to do this would be for example the need to set up a Teams team for the site or use any other Microsoft 365 service that's connected to a Microsoft 365 group. Luckily you can take an existing site, create a new Microsoft 365 group for it, and hook it up to site. To do this you have to use one of the `ConnectSiteCollectionToGroup` methods: these methods take in an `ConnectSiteToGroupOptions` object defining the three mandatory properties: the url of the site that needs to be connected to group, the alias to use for the group and the display name to use for the group:

```csharp
ConnectSiteToGroupOptions groupConnectOptions = new ConnectSiteToGroupOptions(new Uri("https://contoso.sharepoint.com/sites/sitetogroupconnect"), "sitealias", "Site title");
await context.GetSiteCollectionManager().ConnectSiteCollectionToGroupAsync(groupConnectOptions);
```

## Checking if a site collection exists

To verify if a site collection exists you enumerate the site collections or try to get the site collection properties as described above, however these methods require you to have admin privileges. Using the `SiteExists` methods you can verify the existence of a site based upon simply trying to create a `PnPContext` for the site.

```csharp
var exists = await context.GetSiteCollectionManager().SiteExistsAsync(new Uri("https://contoso.sharepoint.com/sites/doiexist"));
if (exists)
{
    // do something
}
```

> [!Note]
> Sites living in the recycle bin are listed as "non existing" via this method.

## Recycling site collections

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

A site collection can be recycled and then later on permanently deleted or restored. Recycling site collection is done using one of the `RecycleSiteCollection` methods:

```csharp
// Recycle the site collection
await context.GetSiteCollectionManager().RecycleSiteCollectionAsync(new Uri("https://contoso.sharepoint.com/sites/sitename"));
```

> [!Note]
> When a group connected site is recycled then also the linked Microsoft 365 group is recycled

## Enumerate the recycled site collections

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

To get a list of recycled site collections you can use the `GetRecycledSiteCollections` methods:

```csharp
var recycledSites = await context.GetSiteCollectionManager().GetRecycledSiteCollectionsAsync();
foreach(var site in recycledSites)
{
    // restore the recycled site or permanently delete it
}
```

## Restoring a recycled site collection

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

> [!Note]
> When the site collection being restored has an associated Microsoft 365 group then this group is also restored. This requires the `Group.ReadWrite.All` delegated or application permission.

A recycled site collection can be restored as long it still sits in the site collection recycle bin. To do so use the `RestoreSiteCollection` methods:

```csharp
var recycledSites = await context.GetSiteCollectionManager().GetRecycledSiteCollectionsAsync();
foreach(var site in recycledSites)
{
    // restore all recycled site collections
    await context.GetSiteCollectionManager().RestoreSiteCollectionAsync(site.Url);
}
```

## Deleting a recycled site collection

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

> [!Note]
> When the site collection being deleted from the recycle bin has an associated Microsoft 365 group it's recommended to not use this approach but rather permanently delete the Microsoft 365 group which will then delete all the resources linked to the Microsoft 365 group.

A recycled site collection can be permanently deleted using the `DeleteRecycledSiteCollection` methods:

```csharp
var recycledSites = await context.GetSiteCollectionManager().GetRecycledSiteCollectionsAsync();
foreach(var site in recycledSites)
{
    // delete all non group connected recycled site collections
    if (site.GroupId == Guid.Empty)
    {
        await context.GetSiteCollectionManager().DeleteRecycledSiteCollectionAsync(site.Url);
    }
}
```

## Deleting site collections

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

A site collection can also be permanently deleted via one of the `DeleteSiteCollection` methods. This method first moves the site collection to the recycled bin and then immediately purges the site collection from the recycle bin.

> [!Note]
> A group connected site will not be permanently deleted, calling the `DeleteSiteCollection` methods will recycle the site collection and group.

```csharp
await context.GetSiteCollectionManager().DeleteSiteCollectionAsync(new Uri("https://contoso.sharepoint.com/sites/sitename"));
```
