# Discovery

The Core SDK Admin library provides support for discovering relevant information of the SharePoint Online tenant that's being used.

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

## Building a context to read data from SharePoint Tenant Admin

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

The SharePoint Tenant Admin center site is not a site one typically would connect to, but in case you need it your code can use the `GetTenantAdminCenterContextAsync` methods:

```csharp
using (var tenantContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync())
{
    // do work with the SharePoint Tenant Admin Center site
}
```
