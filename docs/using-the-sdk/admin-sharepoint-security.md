# Security

The Core SDK Admin library provides SharePoint Admin security related APIs like listing the SharePoint Tenant admins and more.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Is the current user a SharePoint tenant administrator?

If the code you want to run requires SharePoint tenant administrator privileges then you can use the `IsCurrentUserSharePointAdmin` methods to verify.

```csharp
// Checks if the current user is a SharePoint tenant admin
if (await context.GetSharePointAdmin().IsCurrentUserTenantAdminAsync())
{
    // Do the admin operations
}
else
{
    // Handle non admin scenario
}
```

## Getting the SharePoint tenant administrators

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

If you need to list all the SharePoint admins use the `GetTenantAdmins` methods which will return a `ISharePointUser` instance for each admin user.

```csharp
// Get the tenant admins
var admins = await context.GetSharePointAdmin().GetTenantAdminsAsync();

foreach(var admin in admins)
{
    // Do something with the admin user
}
```

## Getting the administrators of a site collection

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

To get the administrators of a given site collection use the `GetSiteCollectionAdmins` methods. You do not have to have access to the site collection to enumerate it's administrators as these methods depend on tenant APIs that only work for SharePoint administrators. When getting the administrators you'll see a difference between Microsoft 365 group connected site collections and the other site collections:

- For Microsoft 365 group connected site collections the Microsoft 365 group owners are included, they have the `IsMicrosoft365GroupOwner` property set to `true`. These users don't have the `LoginName` property set, instead the `Id` property is set
- For the other site collections the returned users all have the `LoginName` property set, the `Id` property is not set. Also one of the admins is marked as `IsSecondaryAdmin == false`, that one administrator is the primary site collection administrator

```csharp
// Get the site collection admins
var admins = await context.GetSiteCollectionManager().GetSiteCollectionAdminsAsync(new Uri("https://contoso.sharepoint.com/sites/somesite"));

foreach(var admin in admins)
{
    // Do something with the site collection admin user
}
```

## Setting the administrators of a site collection

[!INCLUDE [SharePoint Admin required](fragments/sharepoint-admin-required.md)]

To set the administrators of a given site collection use the `SetSiteCollectionAdmins` methods. You do not have to have access to the site collection to set it's administrators as these methods depend on tenant APIs that only work for SharePoint administrators. When setting the administrators you'll see a difference between Microsoft 365 group connected site collections and the other site collections:

- For the other site collections you provide a list of login names (e.g. `i:0#.f|membership|anna@contoso.onmicrosoft.com` or `c:0-.f|rolemanager|spo-grid-all-users/6492ece7-7f5d-4499-8130-50e761e25bd9`). The first one if the list will be set as the primary site collection administrator, the others will be set as secondary site collection administrators
- For Microsoft 365 group connected site collections you do have the same option as for the other site collections with the difference that the primary site collection administrator of group connected sites is never updated. Next to that you can also specify the Azure AD user id's of users you want to grant site collection admin permissions by adding them to the Microsoft 365 group's owners. To stay in sync with with SharePoint Tenant admin center does, when adding a Microsoft 365 group owner the user is also added as a Microsoft 365 group member.

> [!Note]
> The `SetSiteCollectionAdmins` methods will not remove existing site collection admins, only add new site collection admins.

```csharp
// Set the site collection admins for a regular site
List<string> newAdmins = new List<string>();
newAdmins.Add("i:0#.f|membership|anna@contoso.onmicrosoft.com");
newAdmins.Add("c:0-.f|rolemanager|spo-grid-all-users/6492ece7-7f5d-4499-8130-50e761e25bd9");
context.GetSiteCollectionManager().SetSiteCollectionAdmins(new Uri("https://contoso.sharepoint.com/sites/somesite"), newAdmins);

// Set the site collection admins for a Microsoft 365 group connected site
List<Guid> newGroupOwners = new List<Guid>();
newGroupOwners.Add(Guid.Parse("3d25e9c4-b20f-443b-ab4d-8ab0668f72ee"));

context.GetSiteCollectionManager().SetSiteCollectionAdmins(new Uri("https://contoso.sharepoint.com/sites/somesite"), newGroupOwners);
```
