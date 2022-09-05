# Working with the recycle bin

Deleted lists, libraries, documents or list items are not immediately deleted in SharePoint Online, they first end up in a recycle bin. After they've been deleted from the recycle bin they move to the a site collection recycle bin, deleting from there will permanently delete the item. See [Restore items in the recycle bin that were deleted from SharePoint or Teams](https://support.microsoft.com/en-us/office/restore-items-in-the-recycle-bin-that-were-deleted-from-sharepoint-or-teams-6df466b6-55f2-4898-8d6e-c0dff851a0be) and [Restore deleted items from the site collection recycle bin](https://support.microsoft.com/en-us/office/restore-deleted-items-from-the-site-collection-recycle-bin-5fa924ee-16d7-487b-9a0a-021b9062d14b) to learn more.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with files
}
```

## Deleting an item so that it ends up in the recycle bin

When using the delete methods in the SharePoint APIs and such also in PnP Core SDK the actual deleted items is always permanently deleted. For most applications this is the desired behavior, if you want to soft delete an item and move it to the recycle bin you need to use one of the `Recycle` methods which are available on `IList`, `IListItem`, `IFile` and `IAttachment`.

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();
```


## Listing the deleted items from the recycle bin

There are multiple approaches to enumerate the recycle bin: first is by loading the `IWeb` `RecycleBin` property, this gives you the items in the web's recycle bin. Using `RecycleBin` property on `ISite` allows you to query the items in the site collection recycle bin (also known as the second stage recycle bin). An alternative approach via using one of the `IWeb` `GetRecycleBinItemsByQuery` methods offers some more control and also allows you to enumerate items from both recycle bins, allows to set order and do paging and more.

```csharp
// Option A: load the web recycle bin using default options
await context.Web.LoadAsync(w => w.RecycleBin);

foreach(var deletedItem in context.Web.RecycleBin.AsRequested())
{
    // Do something with the deleted item
}

// Option A: load the web recycle bin using default options
await context.Site.LoadAsync(w => w.RecycleBin);

foreach(var deletedItem in context.Site.RecycleBin.AsRequested())
{
    // Do something with the deleted item
}

// Option C: query the web recycle bin
var recyleBinItems = await context.Web.GetRecycleBinItemsByQueryAsync(
    new RecycleBinQueryOptions 
    { 
        ItemState = RecycleBinItemState.FirstStageRecycleBin, 
        IsAscending = true,
        OrderBy = RecycleBinOrderBy.DeletedDate
    });

foreach(var deletedItem in recyleBinItems.AsRequested())
{
    // Do something with the deleted item
}
```

Whenever you delete an item, list or file using a `Recycle` method you get back a GUID id of the deleted item, using that id you can retrieve a specific deleted item:

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();

// Get the recycled document from the recycle bin
IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);
```

## Restore a deleted item

Restoring deleted items is possible using one of the `Restore` methods for a specific item or by using the `RestoreAll` method to restore all deleted items

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();

// Get the recycled document from the recycle bin
IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);

// Option A: Restore the document again
await recycleBinItem.RestoreAsync();

// Option B: Restore all deleted items
await context.Web.RecycleBin.RestoreAllAsync();
```

## Move a deleted item from the recycle bin to the site collection recycle bin

To move a deleted item to the site collection recycle bin (only site collection admins can restore from there) you can use the `MoveToSecondStage` methods or `MoveAllToSecondStage` methods to move all deleted items to the site collection recycle bin.

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();

// Get the recycled document from the recycle bin
IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);

// Option A: Move to the site collection recycle bin
await recycleBinItem.MoveToSecondStageAsync();

// Option B: Move all to the site collection recycle bin
context.Web.RecycleBin.MoveAllToSecondStageAsync();
```

## Delete items from the recycle bin

To delete items from the recycle bin you can either delete items one by one via the `Delete` methods on `IRecycleBinItem` or alternatively delete all items via the `DeleteAll` methods on `IRecycleBinItemCollection`.

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();

// Get the recycled document from the recycle bin
IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);

// Option A: Delete the item from the recycle bin
await recycleBinItem.DeleteAsync();

// Option B: Delete all items from the recycle bin
await context.Web.RecycleBin.DeleteAllAsync();
```

## Delete items from the site collection recycle bin

To delete items from the site collection recycle bin you can either delete items one by one via the `Delete` methods on `IRecycleBinItem` or alternatively delete all items via the `DeleteAll` methods on `IRecycleBinItemCollection`.

```csharp
var doc = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Recycle a document
Guid recycleBinItemId = await doc.RecycleAsync();

// Get the recycled document from the recycle bin
IRecycleBinItem recycleBinItem = await context.Web.RecycleBin.GetByIdAsync(recycleBinItemId, r => r.LeafName);

// Option A: Delete the item from the site collection recycle bin
await recycleBinItem.DeleteAsync();

// Option B: Delete all items from the site collection recycle bin
await context.Web.RecycleBin.DeleteAllSecondStageItemsAsync();
```
