# Tenant

The Core SDK Admin library provides support for getting and setting relevant information of the SharePoint Online tenant that's being used.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Getting the key tenant urls

A SharePoint Online tenant does have a portal host, a host for the OneDrive for Business sites, a tenant admin center site and more. Using the `GetTenant...UriAsync` methods your code can easily get these urls:

```csharp
// Returns the SharePoint tenant admin center url (e.g. https://contoso-admin.sharepoint.com)
var url = await context.GetSharePointAdmin().GetTenantAdminCenterUriAsync();

// Returns the SharePoint tenant portal url (e.g. https://contoso.sharepoint.com)
var url = await context.GetSharePointAdmin().GetTenantPortalUriAsync();

// Returns the SharePoint tenant my site host url (e.g. https://contoso-my.sharepoint.com)
var url = await context.GetSharePointAdmin().GetTenantMySiteHostUriAsync();
```

> [!Note]
> Support for discovering vanity urls is planned for later. When you're tenant is a multi-geo tenant then checkout the [Microsoft 365 multi geo admin article](admin-m365-multigeo.md).

## Getting the tenant properties

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

There are more than hundred properties that control how a tenant operates and using the `GetTenantProperties` methods you can request these properties:

```csharp
var tenantProperties = await context.GetSharePointAdmin().GetTenantPropertiesAsync();

if (tenantProperties.BlockMacSync)
{
    // syncing from Mac devices is blocked
}
```

## Setting tenant properties

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

There are more than hundred properties that control how a tenant operates and using the `Update` methods on the `ITenantProperties` object you can update these properties:

```csharp
// First get the properties
var tenantProperties = await context.GetSharePointAdmin().GetTenantPropertiesAsync();

if (tenantProperties.BlockMacSync)
{
    // syncing from Mac devices is blocked, let's unblock this
    tenantProperties.BlockMacSync = false;

    // Send the updated properties to the server
    await tenantProperties.UpdateAsync();
}
```

## Building a context to read data from SharePoint Tenant Admin

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

The SharePoint Tenant Admin center site is not a site one typically would connect to, but in case you need it your code can use the `GetTenantAdminCenterContextAsync` methods:

```csharp
using (var tenantContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync())
{
    // do work with the SharePoint Tenant Admin Center site
}
```

## Exporting the tenant search schema

The search schema can be exported and imported via an XML file, exporting is done using one of the `GetSearchConfigurationXml` methods:

```csharp
// Get the tenant search configuration XML
var searchConfigXml = await context.GetSharePointAdmin().GetTenantSearchConfigurationXmlAsync();
```

If you're only interested in understanding which managed properties were added for the tenant then you can also use one of the `GetSearchConfigurationManagedProperties` methods. These methods get the needed search configuration XML first and then parses it to return a collection of managed properties:

```csharp
// Get tenant search configuration managed properties
var managedProperties = await context.GetSharePointAdmin().GetTenantSearchConfigurationManagedPropertiesAsync();
foreach(var mp in managedProperties)
{
    // Do something
}
```

## Importing the tenant search schema

Just like the search schema can be exported it's also possible to import it again via calling the `SetSearchConfigurationXml` methods:

```csharp
// Set tenant search configuration XML
await context.GetSharePointAdmin().SetSearchConfigurationXmlAsync(searchConfigXml);
```

## Joining a site to a hub that requires approval

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

Joining and un-joining a hub site can be done using methods (`JoinHubSite`, `UnJoinHubSite`) on `ISite` and it's recommended to use those. There's however the case where joining a hub site requires approval from the hub and in that case the existing methods (`JoinHubSite`) return an error ("This Hub Site requires approval to join. Please resubmit this request with an Approval Token from the Hub Site admin."). To workaround this limitation you can use the tenant `ConnectToHubSite` methods:

```csharp
await context.GetSharePointAdmin().ConnectToHubSiteAsync(
    new Uri("https://contoso.sharepoint.com/sites/sitetojointohub"), 
    new Uri("https://contoso.sharepoint.com/sites/hubsite"));
```
