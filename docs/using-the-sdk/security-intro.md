# Configuring site security

In SharePoint the definition of what a user or group can do on a securable object (Web, List, ListItem) is defined as a role (a.k.a. permission level). For example the role "Full Control" has many permissions like "Manage Lists", "View Items" and more. The default **owners** group of a site has the "Full Control" role granting all users and groups part of the **owners** group the related permissions like "Manage Lists", "View Items" and more. Roles are always managed at the root web level, meaning there's only one set of roles defined per site collection.

By default securable objects inherit permissions: the permissions set on the site are inherited by each list and the are again inherited by each list item. If a user is then added to the site's contributors group he immediately has the "Full Control" role on all of the content in the site. However, sometimes it's need to provide other permissions for a sub site, for a list or for a list item which is implemented by breaking the permission inheritance for the securable object followed by setting the correct permissions on the securable object.

## Configuring roles

### Listing the existing roles

To query the roles defined on the `IWeb` instance load the `RoleDefinitions` property on the root web of the site collection.

```csharp
var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
foreach(role in roleDefinitions)
{
    // do something with the role
    // Typical roles are "Full Control", "Edit", "Contribute", ...
}
```

### Understanding if a role has a permission

To verify if a role has a given permission level you can use

```csharp
// Context is a root web context
var roleDefinition = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "Full Control");
if (roleDefinition.BasePermissions.Has(PermissionKind.ViewPages))
{
    // The "Full Control" role has the "View Pages" permission
}
```

### Adding a custom role

If the default roles and their associated permissions do not fit your needs you can create a new role.

> [!Note]
> It's not recommended to change the out of the box provided roles.

```csharp
// Load the roles, context is a root web context
var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
// Add a custom role
var customRole = await roleDefinitions.AddAsync("My custom role", 
                                                RoleType.None, 
                                                new PermissionKind[] 
                                                { 
                                                    PermissionKind.ViewPages, 
                                                    PermissionKind.ViewListItems 
                                                }, 
                                                "Limited viewing only");
```

### Updating a role

A role can also be updated, simply call one of the `Update` methods after having changed the role. The change the permissions a role has use the `Clear` and `Set` methods on the `BasePermissions` property:

```csharp
customRole = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "My custom role");

customRole.BasePermissions.Clear(PermissionKind.ViewPages);
customRole.BasePermissions.Set(PermissionKind.AddListItems);
customRole.Description = "Custom role";
// Send update to server
await customRole.UpdateAsync();
```

### Deleting a role

Deleting a role can be done via the `Delete` methods:

```csharp
customRole = await context.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == "My custom role");
// Delete the role
await customRole.DeleteAsync();
```

## Assigning roles

To actually grant a permission a role needs to be assigned to a user or group. By default the groups and user and their assigned roles are maintained at the root web level, but it's possible to break permission inheritance and have different permissions at sub web, list or list item level. In PnP Core SDK the three objects that can be secured (`IWeb`, `IList` and `IListItem`) all implement the `ISecurableObject` interface providing the needed properties and methods to configure security.

> [!Note]
> The below chapters use an `IList` as sample but they apply to either `IWeb`, `IList` and `IListItem`.

### Breaking permission inheritance

By default permissions are inherited, so each securable object inherits from it's securable object parent. If you want to configure different permissions for let's say an `IList` then you need to first break permission inheritance using one of the `BreakRoleInheritance` methods. Using the first method parameter `copyRoleAssignments` can opt to copy over the current permissions so that you can edit them later on. If set to `false` then no permissions are copied over. The second parameter `clearSubscopes` determines what will happen with the existing custom permissions: imagine the scenario where that a list in a sub has custom permissions set and then the custom permissions are defined on the web which is the list's `ISecurableObject` parent. When `clearSubscopes` is set to `true` then existing custom permissions on securable child objects are dropped in favor of the current one, if set to `false` the existing custom permissions are left as is.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");
// Break permission inheritance
await myList.BreakRoleInheritanceAsync(false, true);
```

### Restoring permission inheritance

If a securable object again needs to inherit the permissions from it's securable parent object then use one of the `ResetRoleInheritance` methods.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");
// Reset permission inheritance
await myList.ResetRoleInheritanceAsync();
```

### Listing the current roles a user or group has on a securable object

To get a list of the currently assigned roles for a securable object use the `GetRoleDefinitions` methods. Simply pass in the id of principal (= user or group) you want to get the roles for.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");
// Load the role assignments on this list, also load the role definitions
await myList.LoadAsync(w => w.RoleAssignments.QueryProperties(p => p.RoleDefinitions));

foreach (var roleAssignment in myList.RoleAssignments.AsRequested())
{
    // do something with the role assignment
}
```

### Checking if custom permissions are defined

Checking if a securable object has custom role assignments is done via the `HasUniqueRoleAssignments`.

> [!Note]
> When loading data you can apply a filter (via the `where()` LINQ operator) and you can filter on `HasUniqueRoleAssignments == true` when querying webs and lists, however this is not possible when querying list items.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");
// Load the role assignments on this list, also load the role definitions
await myList.LoadAsync(w => w.HasUniqueRoleAssignments);

if (myList.HasUniqueRoleAssignments)
{
    // List has custom permissions set, let's reset them
    await myList.ResetRoleInheritanceAsync();
}
```

### Assigning roles to a user or group

Once you've broken permissions inheritance you will often assign roles for one or more users and groups to the securable object. To do so there are approaches: doing a bulk role assignment via the `AddRoleDefinitions` methods or assigning one role via the `AddRoleDefinition` methods. Both are shown in below example.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");

// Get current user
var currentUser = await context.Web.GetCurrentUserAsync();

// Approach A: adding single role to the current user
var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

await myList.AddRoleDefinitionAsync(currentUser.Id, editRole);

// Approach B: adding multiple roles at once
await myList.AddRoleDefinitionsAsync(currentUser.Id, new string[] { "Full Control", "Edit" });
```

### Removing roles from a user or group

Just like adding roles there's also options for removing roles from a user or group for a securable object: doing a bulk role removal via the `RemoveRoleDefinitions` methods or removing one role via the `RemoveRoleDefinition` methods. Both are shown in below example.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("mylist");

// Get current user
var currentUser = await context.Web.GetCurrentUserAsync();

// Approach A: remove single role to the current user
var roleDefinitions = (await context.Web.GetAsync(p => p.RoleDefinitions)).RoleDefinitions;
var editRole = roleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Edit");

await myList.RemoveRoleDefinitionAsync(currentUser.Id, editRole);

// Approach B: Removing multiple roles at once
await myList.RemoveRoleDefinitionsAsync(currentUser.Id, new string[] { "Full Control", "Edit" });
```
