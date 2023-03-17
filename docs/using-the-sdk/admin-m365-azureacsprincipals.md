# Legacy Azure ACS principals

The Core SDK Admin library contains APIs to enumerate the Azure ACS principals that have permissions on content inside the tenant. Azure ACS principals were commonly used to grant [tenant wide permissions](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azureacs), but also are the mechanism used by SharePoint AddIns to grant the [AddIn access](https://learn.microsoft.com/en-us/sharepoint/dev/sp-add-ins/add-in-permissions-in-sharepoint) content in the host web it was installed into.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

> [!Note]
> If your tenant is using vanity URL's then you'll need to populate the `VanityUrlOptions` class and pass it to any method that allows it.

## Enumerating the Azure ACS principals

When enumerating Azure ACS principals it's important to make a distinction between the principals that are have tenant scoped permissions (so one principal can read/write data in all sites across the tenant) versus principals that are scoped to a site collection, web or possibly even just a single list. There are different methods you can use depending on whether you want to list all ACS principals that have access to a given site, list only the site/web/list scoped ones or only list the tenant scoped ones.

### Enumerate principals scoped the site collection, web or list

To list the Azure ACS principals granted access at either site collection, web or list level use one of the `GetSiteCollectionACSPrincipals` methods. These methods returns a list of `IACSPrincipal` object instances and for these the `SiteCollectionScopedPermissions` property contains the permissions they were granted. 

```csharp
var principals = await context.GetSiteCollectionManager().GetSiteCollectionACSPrincipalsAsync();
foreach(principal in principals)
{
    // do something with the principal
}
```

By default the method will also check fot the principals used in the sub sites of the passed site, but you can prevent that via the `includeSubSites` parameter.

> [!Note]
> When an Azure ACS principal was granted site collection scoped permissions and you've queried a site collection with 5 sub sites you then see the same Azure ACS principal listed for each sub site.

### Enumerate principals scoped the tenant

To list the Azure ACS principals granted access at tenant level use one of the `GetTenantACSPrincipals` methods. These methods returns a list of `IACSPrincipal` object instances where the `TenantScopedPermissions` property contains the permissions they were granted. To use this method you first need to call the `GetLegacyServicePrincipals` method: this method will get a list of app ids by querying Azure AD, which are then used to make the call to get information about the related Azure ACS principal in SharePoint.

```csharp
// First load a list possible principal app ids from Azure AD
var legacyServicePrincipals = await context.GetSiteCollectionManager().GetLegacyServicePrincipalsAsync();

// Pass in the list of app ids to get the final list of principals
var principals = await context.GetSiteCollectionManager().GetTenantACSPrincipalsAsync(legacyServicePrincipals);

foreach(principal in principals)
{
    // do something with the principal
}
```

Given these principals are scoped to tenant the `ServerRelativeUrl` property on the `IACSPrincipal` object instances is not set: the output will be a unique list of all Azure ACS principals with tenant level permissions.

### Enumerate principals scoped the tenant, site collection, web or list

To list the Azure ACS principals granted access at either tenant, site collection, web or list level level use one of the `GetTenantAndSiteCollectionACSPrincipals` methods. These methods returns a list of `IACSPrincipal` object instances where the `SiteCollectionScopedPermissions` and `TenantScopedPermissions` properties contain the permissions they were granted. To use this method you first need to call the `GetLegacyServicePrincipals` method: this method will get a list of app ids by querying Azure AD, which are then used to make the call to get information about the related Azure ACS principal in SharePoint.

> [!Important]
> Given the `GetLegacyServicePrincipals` method can be costly it's best to make this call only once and reuse the output for each call to `GetTenantAndSiteCollectionACSPrincipals`.

```csharp
// First load a list possible principal app ids from Azure AD
var legacyServicePrincipals = await context.GetSiteCollectionManager().GetLegacyServicePrincipalsAsync();

// Pass in the list of app ids to get the final list of principals
var principals = await context.GetSiteCollectionManager().GetTenantAndSiteCollectionACSPrincipalsAsync(legacyServicePrincipals);

foreach(principal in principals)
{
    // do something with the principal
}
```

> [!Note]
>
> - When an Azure ACS principal was granted site collection scoped permissions and you've queried a site collection with 5 sub sites you then see the same Azure ACS principal listed for each sub site
> - When an Azure ACS principal was granted scoped tenant permissions and you've queried a site collection with 5 sub sites you then see the same Azure ACS principal listed for each sub site
