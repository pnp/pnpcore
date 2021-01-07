# Adding, updating or deleting data from Microsoft 365

Adding, updating or deleting data (e.g. a SharePoint list item or a Teams channel message) is usually needed when you write applications using the PnP Core SDK and in this article you'll get a brief overview on how to add, update or delete data.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for adding, updating and deleting data
}
```

## Adding data

Adding of data is usually done via an Add method on the collection to which you want to add data, so if you want to add an [IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html) you would use one of the Add methods (e.g. [AddAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemCollection.html#collapsible-PnP_Core_Model_SharePoint_IListItemCollection_AddAsync_Dictionary_System_String_System_Object__)) on the [IListItemCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemCollection.html) collection. Given that each model (List Item, File, TeamChannel,..) requires different input for doing an add you'll see different add method signatures.

```csharp
Dictionary<string, object> item = new Dictionary<string, object>()
{
    { "Title", "Item1" },
    { "Field A", 25 }
};

// Add the item
var addedItem = await myList.Items.AddAsync(item);
```

Another add example is adding (= uploading) a file to a document library:

```csharp
// Get a reference to a folder
IFolder siteAssetsFolder = await context.Web.Folders.GetFirstOrDefaultAsync(f => f.Name == "SiteAssets");

// Upload a file by adding it to the folder's files collection
IFile addedFile = await siteAssetsFolder.Files.AddAsync("test.docx", 
                  System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestFilesFolder{Path.DirectorySeparatorChar}test.docx"));
```

## Updating data

To update data you first need to get the data to update, see [Get data](basics-getdata.md) to learn more on getting data. Once you've data for example (you did get a list) you can update the list by changing one of it's properties and then call one of the update methods (e.g. UpdateAsync). You can perform multiple updates to the model and only when you're done with updating the properties you need to call one of the update methods, the PnP Core SDK will keep track of the changes and will only use the actually changed properties in the update request.

```csharp
// Get the list to update
var myList = await context.Web.Lists.GetByTitleAsync("List to update");

// Update a list property
myList.Description = "PnP Rocks!";

// Update another list property
myList.EnableVersioning = true;

// Send update to the server
await myList.UpdateAsync();
```

## Deleting data

Like with updating you first need to have the model instance to delete available, see [Get data](basics-getdata.md) to learn more on getting data. Once you have the model to delete available (e.g. a list item) you can use one of the delete methods to perform the delete. Below example uses the DeleteBatchAsync method to delete all items of a list with a single network request.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = await context.Web.Lists.GetByTitleAsync("My List", p => p.Title, p => p.Items, 
                                                                p => p.Fields.LoadProperties(p => p.InternalName, p => p.FieldTypeKind, 
                                                                                             p => p.TypeAsString, p => p.Title));
// Iterate over the retrieved list items
foreach (var listItem in myList.Items)
{
    // Delete all the items in "My List" by adding them to a batch
    await listItem.DeleteBatchAsync();
}

// Execute the batch
await context.ExecuteAsync();
```
