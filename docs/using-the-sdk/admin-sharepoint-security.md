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
