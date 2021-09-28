# SharePoint Site Collections

The Core SDK Admin library provides SharePoint Admin security related APIs for enumerating, creating, updating and deleting site collections.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Enumerate the site collections in a tenant

The typical SharePoint Online tenants contains hundreds of site collections and for quite often there's a need to perform admin tasks on all site collections, so being able to enumerate sites is important. The PnP Core SDK Admin component offers two approaches.

### Getting all site collections

Using the `GetSiteCollections` methods any user/app can enumerate site collections but depending on the user/app's permissions the results will be different:

- When using **application permissions** with `Sites.Read.All` the Microsoft Graph Sites endpoint is queried and **all** site collections in the tenant are returned
- When using **delegated permissions** with `Sites.Read.All` and a user that's not a SharePoint Tenant admin then the Microsoft Graph Search endpoint is queries to return all site collections that the user can access, so a **subset** of the site collections is returned
- When using **delegated permissions** and the user is a SharePoint Tenant administrator then a hidden list in the SharePoint Tenant Admin Center site is queried and **all** site collections in the tenant are returned

Below sample shows how to use the `GetSiteCollections` methods:

```csharp
// Get a list of site collections
var siteCollections = await context.GetSharePointAdmin().GetSiteCollectionsAsync();
```

### Getting all site collections with details

Whereas above approach works for any user, the amount of information returned for a given site collection is limited. If you want to get site collection list with more details about each site collection then consider using the `GetSiteCollectionsWithDetails` methods. The collected information per site collection is:

- The various id's such as site collection id, root web id and Microsoft Graph site id
- The site collection url
- The name of the site collection
- Creation time and creator
- External sharing information
- Site owner name and email
- Storage quota information
- Template details

> [!Important]
> The `GetSiteCollectionsWithDetails` methods do not work when using delegated permissions while the user is **not** a SharePoint Tenant administrator.

Below sample shows how to use the `GetSiteCollectionsWithDetails` methods:

```csharp
// Get a list of site collections with details about each site collection
var siteCollections = await context.GetSharePointAdmin().GetSiteCollectionsWithDetailsAsync();
```
