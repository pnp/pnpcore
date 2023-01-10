# Working with navigation

The navigation will live inside an [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html). The possible navigation options are Quick Launch and Top Navigation. These navigation objects will have navigation nodes. Those navigation nodes can then have child nodes.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with navigation
}
```

## Getting all the navigation nodes for a specific navigation type

As discussed before, the navigation nodes can be obtained for two types.

```csharp
// Get Quick Launch navigation nodes
var nodes = await context.Web.Navigation.GetAsync(n => n.QuickLaunch);

// Get Top Navigation navigation nodes
var nodes = await context.Web.Navigation.GetAsync(n => n.TopNavigationBar);
```

## Getting a specific navigation node by id

We can obtain a navigation node by his unique property `id`:

```csharp
// Get Quick Launch node by id
var node = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1);

// Get Top Navigation node by id
var node = await context.Web.Navigation.TopNavigationBar.GetByIdAsync(1);
```

## Deleting all navigation nodes

There is an option to clear all the nodes of a specific navigation type.

```csharp
// Delete all the quick launch navigation nodes
await context.Web.Navigation.QuickLaunch.DeleteAllNodesAsync();

// Delete all the top navigation navigation nodes
await context.Web.Navigation.TopNavigationBar.DeleteAllNodesAsync();
```

## Adding navigation nodes

Adding navigation nodes comes down to adding a new navigation node to the Navigations's [INavigationNodeCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.INavigation.html#PnP_Core_Model_SharePoint_INavigation_QuickLaunch) using the [AddAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.INavigationNodeCollection.html#PnP_Core_Model_SharePoint_INavigationNodeCollection_AddAsync_PnP_Core_Model_SharePoint_NavigationNodeOptions_).

You can set the following options:

- Title (Required)
- Url (Required)
- ParentNode (Optional)

```csharp
// Add a new Quick Launch Navigation Node
var newNode = await context.Web.Navigation.QuickLaunch.AddAsync(new NavigationNodeOptions
{
    Title = $"Node",
    Url = context.Uri.AbsoluteUri
});

// Add a new child node under a Quick Launch navigation node
var parentNode = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1);
var childNode = await context.Web.Navigation.QuickLaunch.AddAsync(new NavigationNodeOptions
{
    Title = $"Child node",
    Url = context.Uri.AbsoluteUri,
    ParentNode = parentNode
});
```

## Updating navigation nodes

A navigation node has three two properties who can be updated. These are the following:

- Title (string)
- Url (string)
- IsVisible (boolean)

```csharp
// Get the navigation node to update
var nodeToUpdate = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1);

// Update the properties
nodeToUpdate.Title = "New title";
nodeToUpdate.Url = "https://pnp.github.io/pnpcore/";
nodeToUpdate.IsVisible = true;

// Send update to the server
await nodeToUpdate.UpdateAsync();
```

## Deleting navigation nodes

We can delete a navigation node by first getting it by it's ID and then calling one of the `Delete` methods.

```csharp
// Get the node to delete
var nodeToDelete = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1);

// Delete the node
await nodeToDelete.DeleteAsync();
```

## Moving a navigation node after a specific node

We can move navigation nodes around. This can be done by using one of the `MoveNodeAfter` methods. We will have to pass the current node to move and the node after which the node has to be placed.

```csharp
// Get the node to move
var nodeToMove = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1);

// Get the node to move the node after
var nodeToMoveAfter = await context.Web.Navigation.QuickLaunch.GetByIdAsync(2);

// move the nodes
await context.Web.Navigation.QuickLaunch.MoveNodeAfterAsync(nodeToMove, nodeToMoveAfter);
```

## Using audience targeting

Navigation nodes can be targeted to one or more audiences making the site's navigation dynamic depending on the user visiting the site. More details can be found on the [Using audience targeting](audience-targeting-intro.md) page.
