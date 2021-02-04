# Working with list items

List items are a key part of SharePoint and reading, creating, updating and deleting list items is commonly used. In this chapter we'll explain how you use the PnP Core SDK to work with list items.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

## Reading list items

The PnP Core SDK supports multiple ways to read list items and what approach to use depends on your list size and your use case. For a large list you want to consider [using paging](using%20paging.md) and it's also recommended to write a query that only returns the items you really need versus loading all list items. When writing custom queries you also should consider only returning the list fields you need in your application, the lesser rows and fields to return the faster the response will come from the server.

> [!Important]
> When processing list item responses from the server the SDK will translate the server response into a easy to use field value classes in case of complex field types. This feature depends on the List field information being present, you can load your list field information once when you get load your list like (`var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, p => p.Fields.LoadProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));`). The minimal required field properties are `InternalName`, `FieldTypeKind`, `TypeAsString` and `Title`.

### Getting all list items

If you simply want to load all list items and your list is not containing a lot of items you load the [Items property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) of your list.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.LoadProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));
// Get the item with title "Item1"
var addedItem = myList.Items.FirstOrDefault(p => p.Title == "Item1");

// Iterate over the retrieved list items
foreach (var listItem in myList.Items)
{
    // Do something with the list item
}
```

### Getting list items using a CAML query

### Getting list items via 

## Adding list items

## Updating list items

## Deleting list items

## Other list item operations