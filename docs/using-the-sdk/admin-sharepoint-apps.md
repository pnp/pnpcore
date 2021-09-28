# SharePoint Apps

The Core SDK Admin library provides SharePoint Apps related APIs for configuring app prerequisites for managing apps.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Getting the url for the tenant app catalog site

When deploying SharePoint apps (SharePoint SPFx solutions, but also the classic SharePoint Add-Ins) these apps are first added into an app catalog. There's one app catalog site at tenant level hosting the app catalog with apps that are available for all sites in your tenant, alternatively you can setup an app catalog at site collection level to scope the apps to the site collection.

When you're running setup tasks you need to ensure there's an app catalog site setup, using the `GetTenantAppCatalogUri` methods you can get the current tenant app catalog site url:

```csharp
// Get the tenant app catalog url, returns null if there's none setup
var url = await context.GetSharePointAdmin().GetTenantAppCatalogUriAsync();
```

## Ensuring there's a tenant app catalog

If you want to ensure there's a tenant app catalog because you need to deploy an app to it, you can use the `EnsureTenantAppCatalog` methods. If the tenant app catalog site exists the methods return false, if there was no app catalog it will be setup using the default path of `sites/appcatalog` and the method returns true.

```csharp
// Get the tenant app catalog url, returns null if there's none setup
if (await context.GetSharePointAdmin().EnsureTenantAppCatalogAsync())
{
    // App catalog site was missing, but now added as /sites/appcatalog
}
else
{
    // The app catalog site was already available
}
```

Note that you have to use the `GetTenantAppCatalogUri` to get the actual app catalog site url, even when there was no app catalog site and it was created by calling `EnsureTenantAppCatalog` it's still recommended to get the actual url.
