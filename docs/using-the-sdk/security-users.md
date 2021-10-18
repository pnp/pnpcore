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

Before a user can be used the user needs to exist and the best way to ensure a user exists is by using the `EnsureUser` methods.

```csharp
// Regular user
var user = await context.Web.EnsureUserAsync("joe@contoso.onmicrosoft.com");

// Special user
var specialUser = await context.Web.EnsureUserAsync("Everyone except external users");
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
