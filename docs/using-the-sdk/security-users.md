# Working with users

Using SharePoint groups is the common model to used to grant users or other groups access to a SharePoint site, but you can also directly [grant access to a user](./security-intro.md#assigning-roles). When a user or group is assigned a role then that user has to exist in the web's user table. In this article you'll learn how query and add users to the web's user table.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with lists
}
```

## SharePoint users

SharePoint Online uses Azure Active Directory as the source of regular users to work with, but before you can grant a user a role the user needs to exist in the root web's user table which is implemented as a hidden list named `User Information List`. Besides the regular users there are also special user accounts, below are the common ones you can work with. There are other system users (e.g. `i:0#.w|nt service\spsearch`) that should be left untouched.

User account | Login name | Description
-------------|------------|------------
Everyone | `c:0(.s|true` | Use this user to represent all users in your organization
Everyone except external users | `c:0-.f|rolemanager|spo-grid-all-users/<guid>` | Use this user to represent all users except the internal users in your organization
Azure AD security group | `c:0t.c|tenant|<guid>` | You can add an Azure AD security group to a SharePoint Group or directly grant it a role by using this login name. The `<guid>` is the Azure AD object id of the security group
Company Administrator | `c:0t.c|tenant|<guid>` | Company Administrator translated to a group containing all the tenant level SharePoint administrators. The `<guid>` is the Azure AD object id of the security group
Microsoft 365 group | `c:0o.c|federateddirectoryclaimprovider|<guid>` | You can add a Microsoft 365 group to a SharePoint Group or directly grant it a role by using this login name. The `<guid>` is the Azure AD object id of the Microsoft 365 group. Note that adding this Microsoft 365 group will grant it's members access

As you can't simply update the user table list to add users, use the approaches as outlined below.

### Getting users

To get the user's defined for the current site load the web's `SiteUsers` property.

```csharp
await context.Web.LoadAsync(p => p.SiteUsers);

foreach(var group in context.Web.SiteUsers.AsRequested())
{
    // do something with the user
}
```

To get the current user use one of the `GetCurrentUser` methods:

```csharp
var currentUser = await context.Web.GetCurrentUserAsync();
```

To verify if a specific user exists you can use the `GetUserById` methods:

```csharp
var foundUser = await context.Web.GetUserByIdAsync(userId);
```

## Adding/ensuring a user

Before a user can be used the user needs to exist and the best way to ensure a user exists is by using the `EnsureUser` and `EnsureEveryoneExceptExternalUsers` methods. The latter method is needed for the "Everyone except external users" user as that string is different for sites created in other languages. Other "special" users are language neutral.

```csharp
// Regular user
var user = await context.Web.EnsureUserAsync("joe@contoso.onmicrosoft.com");

// Special users: for the "Everyone except external users" user use the EnsureEveryoneExceptExternalUsersAsync method
var everyoneExceptEnternalUsers = await context.Web.EnsureEveryoneExceptExternalUsersAsync();

// Other special users can be done using the string
var companyAdministators = await context.Web.EnsureUserAsync("Company Administrator");

```

> [!Important]
> Using `EnsureUser` will add the user to the site's users list whenever the user was not there, during that operation the passed in user needs to be a valid user. However, if the user already was added to the site, then deleted from Azure AD, followed by calling `EnsureUser` this method will not return an error. Calling one of the `ValidateAndEnsureUsers` method is the best option here, alternatively calling `AsGraphUser` method will perform an actual validation and throw an error is the user does not exist in Azure AD.

### Validating users

When you've a list of users and you want to validate if these users still exist in Azure AD then use the `ValidateUsers` methods, if you also want to ensure (see previous chapter) that exist in Azure AD use the `ValidateAndEnsureUsers` methods.

```csharp
var userList = new List<string> 
{ 
    "ValidUser1@contoso.onmicrosoft.com", 
    "ValidUser2@contoso.onmicrosoft.com", 
    "NonExistingUser@contoso.onmicrosoft.com" 
};

// Check users, returns a list containing the NonExistingUser@contoso.onmicrosoft.com user
var nonExistingUsers = await context.Web.ValidateUsersAsync(userList);

// Checks users and ensures the ones that exist in Azure AD. 
// Returns a list with 2 ISharePointUser instances for ValidUser1@contoso.onmicrosoft.com
// and ValidUser2@contoso.onmicrosoft.com
var existingUserList = await context.Web.ValidateAndEnsureUsersAsync(userList);
```

## Granting permissions for a user at web level

Once a user is added you can directly permission the user by granting it one or more role definitions via one of the `AddRoleDefinitions` methods. You can also enumerate the roles a user has via the `GetRoleDefinitions` methods and remove granted roles via the `RemoveRoleDefinitions` methods. These methods are equivalent to using the methods provided via the `ISecurableObject` interface on `IWeb`.

```csharp
var user = await context.Web.EnsureUserAsync("joe@contoso.onmicrosoft.com");

// Grant the user the Full Control role at web level
await user.AddRoleDefinitionsAsync("Full Control");

// Check the roles the user has been granted
var currentRoleDefinitions = await user.GetRoleDefinitionsAsync();

if (currentRoleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control"))
{
    // User was granted Full Control
}

// Remove the Full Control role from the user at web level
user.RemoveRoleDefinitions(new string[] { "Full Control" });
```

## Verifying the permissions of a user at `IWeb`, `IList` or `IListItem` level

If you want to get the permissions of a user granted at `IWeb`, `IList` or `IListItem` level or verify if a user has a certain `PermissionKind` then you can use the `GetUserEffectivePermissions` and `CheckIfUserHasPermissions` methods:

```csharp
// Get the permissions of a user
var basePermissions = await context.Web.GetUserEffectivePermissionsAsync("joe@contoso.sharepoint.com");

// Verify if a user has a permission
if (await context.Web.CheckIfUserHasPermissionsAsync("joe@contoso.sharepoint.com", PermissionKind.AddListItems))
{
    // Do something
}
```

## Microsoft 365 users

When working with SharePoint sites you use a SharePoint user, but when you for example want to add a user to a Microsoft 365 group's owners you need to use a Microsoft Graph user object. When you either have a SharePoint user or Microsoft Graph user you can translate that user via `AsGraphUser` and `AsSharePointUser` methods. Below example shows how a user granted access to a Microsoft Teams team can be translated into a SharePoint user.

```csharp
// Load the Team owners, returns a collection of `IGraphUser` objects
var team = await context.Team.GetAsync(p => p.Owners);

// Get the first owner
var graphUser = team.Owners.AsRequested().FirstOrDefault();

// Get sharepoint user for graph user
var sharePointUser = await graphUser.AsSharePointUserAsync();
```

The opposite flow is also possible:

```csharp
// Get the users defined for this web
var web = await context.Web.GetAsync(p => p.SiteUsers);

// Get the first "regular" user
var testUser = web.SiteUsers.AsRequested().FirstOrDefault(p => p.PrincipalType == PrincipalType.User);

// Get that user as a Graph user
var graphUser = await testUser.AsGraphUserAsync();
```
