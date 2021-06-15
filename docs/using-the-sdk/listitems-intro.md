# Working with list items

List items are a key part of SharePoint and reading, creating, updating and deleting list items is commonly used. In this chapter we'll explain how you use the PnP Core SDK to work with list items.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with list items
}
```

## Reading list items

The PnP Core SDK supports multiple ways to read list items and what approach to use depends on your list size and your use case. For a large list you want to consider [using paging](basics-getdata-paging.md) and it's also recommended to write a query that only returns the items you really need versus loading all list items. When writing custom queries you also should consider only returning the list fields you need in your application, the lesser rows and fields to return the faster the response will come from the server.

> [!Important]
> When processing list item responses from the server the SDK will translate the server response into a easy to use field value classes in case of complex field types. This feature depends on the List field information being present, you can load your list field information once when you get load your list like (`var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));`). The minimal required field properties are `InternalName`, `FieldTypeKind`, `TypeAsString` and `Title`.

### Getting all list items

If you simply want to load all list items and your list is not containing a lot of items you load the [Items property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) of your list.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Get the item with title "Item1"
var addedItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

// Iterate over the retrieved list items
foreach (var listItem in myList.Items)
{
    // Do something with the list item
}
```

### Getting list items via a CAML query

SharePoint [CAML](https://docs.microsoft.com/en-us/sharepoint/dev/schema/query-schema) queries allow you to express a filter when loading list item data and scope down the loaded fields to the ones you need. You can use call the [LoadItemsByCamlQueryAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_LoadItemsByCamlQueryAsync_PnP_Core_Model_SharePoint_CamlQueryOptions_) on an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html) for this purpose. When using this method you can either provide the CAML query directly or use the [CamlQueryOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html) class for more fine-grained control. If you use this class you typically would use the [ViewXml property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html#collapsible-PnP_Core_Model_SharePoint_CamlQueryOptions_ViewXml), but also [FolderServerRelativeUrl](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html#collapsible-PnP_Core_Model_SharePoint_CamlQueryOptions_FolderServerRelativeUrl) is used a lot to scope the query to given folder in the list.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
// If you are using CAML query, do not use the parameter 'p => p.Items'. It causes full items to load, ignoring the CAML query. 
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title,
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

// Build a query that only returns the Title field for items where the Title field starts with "Item1"
string viewXml = @"<View>
                    <ViewFields>
                      <FieldRef Name='Title' />
                    </ViewFields>
                    <Query>
                      <Where>
                        <BeginsWith>
                          <FieldRef Name='Title'/>
                          <Value Type='text'>Item1</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                   </View>";

// Execute the query
await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
{
    ViewXml = viewXml,
    DatesInUtc = true
});

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item
}
```

#### Using paging with CAML queries

By setting a row limit in the CAML query combined with using the the PagingInfo attribute of the [CamlQueryOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html) class you can use CAML queries to load data in a paged manner. Below snippet shows the initial page load getting 20 items and the next one getting the next 20 items.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
// If you are using CAML query, do not use the parameter 'p => p.Items'. It causes full items to load, ignoring the CAML query. 
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title,
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

// Build a query that only returns the first 20 rows where the Title field starts with "Item1"
string viewXml = @"<View>
                    <ViewFields>
                      <FieldRef Name='Title' />
                    </ViewFields>
                    <Query>
                      <Where>
                        <BeginsWith>
                          <FieldRef Name='Title'/>
                          <Value Type='text'>Item1</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <RowLimit>20</RowLimit>
                   </View>";

// Execute the query loading the first 20
await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
{
    ViewXml = viewXml,
    DatesInUtc = true
});

// Execute the query loading the next 20
await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
{
    ViewXml = viewXml,
    DatesInUtc = true,
    PagingInfo = $"Paged=TRUE&p_ID={myList.Items.Last().Id}"
});

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item
}
```

> [!Note]
>
> - If you're query is ordered by one or more fields these fields also have to specified in the PagingInfo, e.g. if ordered on Title the PagingInfo would be `$"Paged=TRUE&p_ID={list2.Items.Last().Id}&p_Title=${list2.Items.Last().Title}"`. If you want to load the previous page you also need to add `&PagedPrev=TRUE`. When using the `LoadListDataAsStream` methods the paging info is automatically returned.
> - If you want to load 'system properties' like FileLeafRef, FileDirRef etc then these are not returned when doing a CAML query using the `LoadItemsByCamlQuery` methods, the `LoadListDataAsStream` methods however do support a CAML query that specifies a system property in the `ViewFields`

### Using the ListDataAsStream approach

Using the [LoadListDataAsStreamAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_LoadListDataAsStreamAsync_PnP_Core_Model_SharePoint_RenderListDataOptions_) gives you the most control over how to query the list and what data to return. Using this method is similar to the above described [LoadItemsByCamlQueryAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_LoadItemsByCamlQueryAsync_System_String_) as you typically specify a [CAML](https://docs.microsoft.com/en-us/sharepoint/dev/schema/query-schema) query when using this method. To configure the input of this method you need to use the [RenderListDataOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.RenderListDataOptions.html). Defining the CAML query to run can be done via the ViewXml property and telling what type of data to return can be done via the [RenderOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.RenderListDataOptions.html#PnP_Core_Model_SharePoint_RenderListDataOptions_RenderOptions) property.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

// Build a query that only returns the Title field for the top 5 items where the Title field starts with "Item1"
string viewXml = @"<View>
                    <ViewFields>
                      <FieldRef Name='Title' />
                      <FieldRef Name='FileLeafRef' />
                    </ViewFields>
                    <Query>
                      <Where>
                        <BeginsWith>
                          <FieldRef Name='Title'/>
                          <Value Type='text'>Item1</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <RowLimit>5</RowLimit>
                   </View>";

// Execute the query
var output = await myList.LoadListDataAsStreamAsync(new RenderListDataOptions()
{
    ViewXml = viewXml,
    RenderOptions = RenderListDataOptionsFlags.ListData
});

// If needed do something with the output, e.g. (int)result["LastRow"] tells you the last loaded row

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item
}
```

#### Using paging with ListDataAsStream

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

// Build a query that only returns the Title field for the first 20 items where the Title field starts with "Item1"
string viewXml = @"<View>
                    <ViewFields>
                      <FieldRef Name='Title' />
                      <FieldRef Name='FileLeafRef' />
                    </ViewFields>
                    <Query>
                      <Where>
                        <BeginsWith>
                          <FieldRef Name='Title'/>
                          <Value Type='text'>Item1</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <RowLimit Paged='TRUE'>20</RowLimit>
                   </View>";

// Load all the needed data using paged requests
bool paging = true;
string nextPage = null;
while (paging)
{
    var output = await pagesLibrary.LoadListDataAsStreamAsync(new RenderListDataOptions()
    {
        ViewXml = viewXml,
        RenderOptions = RenderListDataOptionsFlags.ListData,
        Paging = nextPage ?? null,
    }).ConfigureAwait(false);

    if (output.ContainsKey("NextHref"))
    {
        nextPage = output["NextHref"].ToString().Substring(1);
    }
    else
    {
        paging = false;
    }
}

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item
}
```

## Adding list items

Adding list items is done using one of the Add methods on the [ListItemCollection class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemCollection.html), e.g. the [AddAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemCollection.html#PnP_Core_Model_SharePoint_IListItemCollection_AddAsync_Dictionary_System_String_System_Object__) and requires two steps:

- You fill a `Dictionary<string, object>` with fields and their needed value
- You send the assembled data to the server via one of the Add methods

> [!Note]
> Below examples use simple fields like a Text and Number field, check out the [working with complex list item fields](listitems-fields.md) article to learn more about how to use complex fields (e.g. Taxonomy, User,...) with list items.

```csharp
// Fill a dictionary with fields and their value
Dictionary<string, object> item = new Dictionary<string, object>()
{
    { "Title", "Item1" },
    { "Field A", 25 }
};

// Persist the item
var addedItem = await myList.Items.AddAsync(item);
```

When you add list items you quite often need to add multiple items and the best way to do that is using the Batch methods (e.g. [AddBatchAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemCollection.html#PnP_Core_Model_SharePoint_IListItemCollection_AddBatchAsync_Dictionary_System_String_System_Object__)), the lesser server roundtrips the faster your code will be.

```csharp
// Add 20 items to the list
for (int i = 0; i < 20; i++)
{
    Dictionary<string, object> values = new Dictionary<string, object>
    {
        { "Title", $"Item {i}" }
    };

    // Use the AddBatch method to add the request to the current batch
    await myList.Items.AddBatchAsync(values);
}

// Execute all added batch requests as a single request to the server
await context.ExecuteAsync();
```

## Updating list items

Updating a list item comes down to updating the field values followed by calling an [IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html) Update method such as [UpdateAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelUpdate.html#PnP_Core_Model_IDataModelUpdate_UpdateAsync). Depending on how you want to do the update you do have alternative methods:

Methods | Description
--------|------------
Update, UpdateAsync | Regular update, this will result in a new version being created and the modified and editor list item fields will be updated
SystemUpdate, SystemUpdateAsync | Updates the item without creating a new version and without updating the modified and editor list item fields
UpdateOverWriteVersion, UpdateOverWriteVersionAsync | Updates the item without creating a new version and the modified and editor list item fields will be updated

> [!Note]
> Below examples use simple fields like a Text and Number field, check out the [working with complex list item fields](listitems-fields.md) article to learn more about how to use complex fields (e.g. Taxonomy, User,...) with list items.

```csharp
// Fill a dictionary with fields and their value
Dictionary<string, object> item = new Dictionary<string, object>()
{
    { "Title", "Item1" },
    { "Field A", 25 }
};

// Persist the item
var addedItem = await myList.Items.AddAsync(item);

// Update the item values
addedItem["Field A"] = 100;

// Update the item in SharePoint
await addedItem.UpdateAsync();
```

## Deleting list items

Using the Delete methods like DeleteAsync or DeleteBatchAsync you can delete one or more list items in a single server roundtrip. Batching is preferred if you need to delete multiple list items.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Delete all the items in "My List" by adding them to a batch
    await listItem.DeleteBatchAsync();
}

// Execute the batch
await context.ExecuteAsync();
```

## Getting changes for a list item

You can use the `GetChanges` methods on an `IListItem` to list all the changes. See [Enumerating changes that happened in SharePoint](changes-sharepoint.md) to learn more.
