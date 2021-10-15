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

To actually grant a permission a role needs to be associated to a user or group. By default the groups and user and their associated roles are maintained at the root web level, but it's possible to break permission inheritance and have different permissions at sub web, list or list item level.

### Assigning/removing role for a user or group at web level

todo

```csharp

```

### Configure web, list or list item to use unique permissions

todo

```csharp

```

### Assigning/removing role for a user or group at list level

todo

```csharp

```

### Assigning/removing role for a user or group at list item level

todo

```csharp

```
