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

The PnP Core SDK supports multiple ways to read list items and what approach to use depends on your list size and your use case. For a large list you need to use a paged approach and it's also recommended to write a query that only returns the items you really need versus loading all list items. When writing custom queries you also should consider only returning the list fields you need in your application, the lesser rows and fields to return the faster the response will come from the server.

> [!Important]
> When processing list item responses from the server the SDK will translate the server response into a easy to use field value classes in case of complex field types. This feature depends on the List field information being present, you can load your list field information once when you get load your list like (`var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title));`). The minimal required field properties are `InternalName`, `FieldTypeKind`, `TypeAsString` and `Title`.

Use below information to help pick the best option for reading list items.

### No need to read 'system properties' like FileLeafRef, FileDirRef and you've no need to filter list items

Requirements | Recommended approach
-------------|---------------------
List item count <= 100 | [Option A](#a-getting-list-items-max-100-items): expand the items via a `Get` or `Load` method
List item count > 100 | [Option B](#b-getting-list-items-via-paging-no-item-limit): iterate over the list items using implicit paging

### You need to read 'system properties' like FileLeafRef, FileDirRef or you need to filter list items or you want to define the returned fields or you want to get lookup column properties

Requirements | Recommended approach
-------------|---------------------
You want to also 'expand' list item collections like `RoleAssignments` | [Option C](#c-getting-list-items-via-the-loaditemsbycamlquery-approach): use a CAML query via the `LoadItemsByCamlQuery` methods
You want to have more details on the list item properties (e.g. author name instead of only the author id, lookup column values) | [Option D](#d-using-the-listdataasstream-approach): use a CAML query via the `ListDataAsStream` methods

### A. Getting list items (max 100 items)

If you simply want to load all list items in a small list (< 100 items) you load the [Items property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) of your list.

```csharp
// Assume the fields where not yet loaded, so loading them with the list.
// Also expand the items when loading the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Get the item with title "Item1"
var addedItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item
    if (listItem["MyStatus"].ToString() == "Pending")
    {
      // take action
    }
}
```

If you'd like to load list item properties then this is possible via `QueryProperties`:

```csharp
// Assume the fields where not yet loaded, so loading them with the list
// Also expand the items when loading the list, including specific item properties
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, 
            p => p.Items.QueryProperties(p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, 
                                                                                p => p.RoleDefinitions)), 
            p => p.Fields.QueryProperties(p => p.InternalName, 
                                          p => p.FieldTypeKind, 
                                          p => p.TypeAsString, 
                                          p => p.Title));
```

> [!Note]
>
> - When list items are loaded in this manner SharePoint Online will only return 100 items, to get more you'll need to use a paged approach
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When referencing a field ensure to use the correct field name casing: `version_x0020_tag` is not the same as `Version_x0020_Tag`.
> - Filtering on the `HasUniqueRoleAssignments` and `FileSystemObjectType` fields is not allowed by SharePoint.

### B. Getting list items via paging (no item limit)

If your list contains more than 100 items and you don't have a need to specify a query to limit the returned list items then iterating over the list item collection is the recommended model as that will automatically page the list items using a configurable page size:

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = await context.Web.Lists.GetByTitleAsync("My List", p => p.Title,  
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Do a paged retrieval of the list items
await foreach(var listItem in myList.Items)
{
    // Do something with the list item
    if (listItem["MyStatus"].ToString() == "Pending")
    {
      // take action
    }
}
```

If you'd like to load list item properties then this is possible via `QueryProperties`:

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Do a paged retrieval (non async sample) of the list items with additional collection loads
foreach(var listItem in myList.Items.QueryProperties(p => p.All,
                                     p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, 
                                                                           p => p.RoleDefinitions))
{
    // Do something with the list item
    if (listItem["MyStatus"].ToString() == "Pending")
    {
      // Do something with the list item and the per 
      // item loaded RoleAssignments and RoleDefinitions
    }
}
```

> [!Note]
>
> - Ensure you add `p => p.All` to ensure your custom fields are loaded in case you need those
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When referencing a field ensure to use the correct field name casing: `version_x0020_tag` is not the same as `Version_x0020_Tag`.
> - Filtering on the `HasUniqueRoleAssignments` and `FileSystemObjectType` fields is not allowed by SharePoint.

### C. Getting list items via the LoadItemsByCamlQuery approach

SharePoint [CAML](https://docs.microsoft.com/en-us/sharepoint/dev/schema/query-schema) queries allow you to express a filter when loading list item data and scope down the loaded fields to the ones you need. You can use call the [LoadItemsByCamlQueryAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_LoadItemsByCamlQueryAsync_PnP_Core_Model_SharePoint_CamlQueryOptions_) on an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html) for this purpose. When using this method you can either provide the CAML query directly or use the [CamlQueryOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html) class for more fine-grained control. If you use this class you typically would use the [ViewXml property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html#collapsible-PnP_Core_Model_SharePoint_CamlQueryOptions_ViewXml), but also [FolderServerRelativeUrl](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html#collapsible-PnP_Core_Model_SharePoint_CamlQueryOptions_FolderServerRelativeUrl) is used a lot to scope the query to given folder in the list.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title,
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

// Build a query that only returns the Title field for items where the Title field starts with "Item1"
string viewXml = @"<View>
                    <ViewFields>
                      <FieldRef Name='Title' />
                      <FieldRef Name='FileRef' />
                    </ViewFields>
                    <Query>
                      <Where>
                        <BeginsWith>
                          <FieldRef Name='Title'/>
                          <Value Type='text'><![CDATA[Item1]]</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                   </View>";

// Execute the query
await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
{
    ViewXml = viewXml,
    DatesInUtc = true
}, p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, p => p.RoleDefinitions));

// Iterate over the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item and the per 
    // item loaded RoleAssignments and RoleDefinitions
}
```

> [!Note]
>
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When you want to reuse a `PnPContext` which you've previously used to load items you first need to clear the loaded items via `myList.Items.Clear()`
> - When referencing a field ensure to use the correct field name casing: `version_x0020_tag` is not the same as `Version_x0020_Tag`.
> - Filtering on the `HasUniqueRoleAssignments` field is not allowed by SharePoint.
> - When using `text` fields in a CAML query is recommended to escape the text field value to ensure the query does not break. Escaping should be done using `<![CDATA[{MyVariable}]]`
> - When using the `CamlQueryOptions.FolderServerRelativeUrl` property then this will not work if the referred folder has a # or & it it's name. A workaround then is scoping the CAML query to the folder via the `FileDirRef` element in combination with setting the `Scope='RecursiveAll'` property in the `View` element. See [here](https://github.com/pnp/pnpcore/issues/839) for more context.

#### Using paging with LoadItemsByCamlQuery

By setting a row limit in the CAML query combined with using the the PagingInfo attribute of the [CamlQueryOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CamlQueryOptions.html) class you can use CAML queries to load data in a paged manner. Below snippet loads all pages in memory and then iterates over the loaded items:

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title,
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));

int pageSize = 500;
// Filter on files and load 'system' properties like FileRef
string viewXml = @$"<View>
    <ViewFields>
      <FieldRef Name='Title' />
      <FieldRef Name='FileRef' />
      <FieldRef Name='FileLeafRef' />
      <FieldRef Name='FSObjType'/>
      <FieldRef Name='File_x0020_Size' />
    </ViewFields>
    <Query>
        <Where>
        <Eq>
            <FieldRef Name='FSObjType'/>
            <Value Type='Integer'>0</Value>
        </Eq>
        </Where>
    </Query>
    <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
    <RowLimit>{pageSize}</RowLimit>
    </View>";

// Load all the needed data using paged requests
bool paging = true;
string nextPage = null;
int pages = 0;
while (paging)
{
    // Execute the query
    await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
    {
        ViewXml = viewXml,
        DatesInUtc = true,
        PagingInfo = nextPage
    },
    // Load list item collections (e.g. RoleAssignments)
    p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, 
          p => p.RoleDefinitions.QueryProperties(rd => rd.Id, rd => rd.Name)),
    // Load FieldValuesAsText to get access to 'system' properties
    p => p.FieldValuesAsText,
    // Load the HasUniqueRoleAssignments property
    p => p.HasUniqueRoleAssignments);

    pages++;

    if (myList.Items.Length == pages * pageSize)
    {
        nextPage = $"Paged=TRUE&p_ID={myList.Items.AsRequested().Last().Id}";
    }
    else
    {
        paging = false;
    }
}

// Iterate over ALL the retrieved list items
foreach (var listItem in myList.Items.AsRequested())
{
    // Do something with the list item and the per 
    // item loaded RoleAssignments and RoleDefinitions
    var fileSize = listItem.FieldValuesAsText["File_x005f_x0020_x005f_Size"];
}
```

> [!Note]
>
> - If you're query is ordered by one or more fields these fields also have to specified in the PagingInfo, e.g. if ordered on Title the PagingInfo would be `$"Paged=TRUE&p_ID={list2.Items.AsRequested().Last().Id}&p_Title=${list2.Items.AsRequested().Last().Title}"`. If you want to load the previous page you also need to add `&PagedPrev=TRUE`. When using the `LoadListDataAsStream` methods the paging info is automatically returned.

Sometimes loading all pages in memory is not what you need (e.g. due to memory/performance constraints) and you'd rather want to read a page, process it and then read the next page. This can be done by clearing the loaded items collection while paging as shown in this sample:

```csharp
bool paging = true;
string nextPage = null;
int pages = 0;
int totalItemsLoaded = 0;
while (paging)
{
    // Clear the previous page (if any)
    myList.Items.Clear();

    // Execute the query, this populates a page of list items 
    await myList.LoadItemsByCamlQueryAsync(new CamlQueryOptions()
    {
        ViewXml = viewXml,
        DatesInUtc = true,
        PagingInfo = nextPage
    },
    // Load list item collections (e.g. RoleAssignments)
    p => p.RoleAssignments.QueryProperties(p => p.PrincipalId, 
          p => p.RoleDefinitions.QueryProperties(rd => rd.Id, rd => rd.Name)),
    // Load FieldValuesAsText to get access to 'system' properties
    p => p.FieldValuesAsText,
    // Load the HasUniqueRoleAssignments property
    p => p.HasUniqueRoleAssignments);

    pages++;
    totalItemsLoaded = totalItemsLoaded + myList.Items.Length;

    if (totalItemsLoaded == pages * pageSize)
    {
        nextPage = $"Paged=TRUE&p_ID={myList.Items.AsRequested().Last().Id}";
    }
    else
    {
        paging = false;
    }

    // Iterate over the retrieved page of list items
    foreach (var listItem in myList.Items.AsRequested())
    {        
    }
}
```

### D. Using the ListDataAsStream approach

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
                          <Value Type='text'>![CDATA[Item1]]</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
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

> [!Note]
>
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When you want to reuse a `PnPContext` which you've previously used to load items you first need to clear the loaded items via `myList.Items.Clear()`
> - When referencing a field ensure to use the correct field name casing: `version_x0020_tag` is not the same as `Version_x0020_Tag`.
> - Filtering on the `HasUniqueRoleAssignments` field is not allowed by SharePoint.
> - When using `text` fields in a CAML query is recommended to escape the text field value to ensure the query does not break. Escaping should be done using `<![CDATA[{MyVariable}]]`
> - When using the `CamlQueryOptions.FolderServerRelativeUrl` property then this will not work if the referred folder has a # or & it it's name. A workaround then is scoping the CAML query to the folder via the `FileDirRef` element in combination with setting the `Scope='RecursiveAll'` property in the `View` element. See [here](https://github.com/pnp/pnpcore/issues/839) for more context.
> - When you need to fetch additional fields populated via a lookup column just specify the field in the `ViewFields` node of the CAML query, e.g. `<FieldRef Name='LookupSingle_x003a_Created' />` loads the `Created` field brought via the lookup column named `LookupSingle`

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
                          <Value Type='text'>![CDATA[Item1]]</Value>
                        </BeginsWith>
                      </Where>
                    </Query>
                    <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                    <RowLimit Paged='TRUE'>20</RowLimit>
                   </View>";

// Load all the needed data using paged requests
bool paging = true;
string nextPage = null;
while (paging)
{
    var output = await myList.LoadListDataAsStreamAsync(new RenderListDataOptions()
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

Sometimes loading all pages in memory is not what you need (e.g. due to memory/performance constraints) and you'd rather want to read a page, process it and then read the next page. This can be done by clearing the loaded items collection while paging as shown in this sample:

```csharp
// Load all the needed data using paged requests
bool paging = true;
string nextPage = null;
while (paging)
{
  // Clear the previous page (if any)
    myList.Items.Clear();

    // Execute the query, this populates a page of list items 
    var output = await myList.LoadListDataAsStreamAsync(new RenderListDataOptions()
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

    // Iterate over the retrieved page of list items
    foreach (var listItem in myList.Items.AsRequested())
    {
    }
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

> [!Note]
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When referencing a field ensure to use the correct field name casing: `version` is not the same as `Version`.

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

> [!Note]
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.
> - When referencing a field ensure to use the correct field name casing: `version` is not the same as `Version`.

### Updating the list item Author, Editor, Created and Modified system properties

A common request is to change the list item `Author`, `Editor`, `Created` and `Modified` system properties, which is allowed via the `UpdateOverWriteVersion` methods.

> [!Note]
> The Azure AD application you're using must have the `Sites.FullControl.All` permission to make updating the `Author`, `Editor`, `Created` and `Modified` system properties work.

```csharp
// Load the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, 
                                                     p => p.Items,
                                                     p => p.Fields.QueryProperties(
                                                       p => p.InternalName, 
                                                       p => p.FieldTypeKind, 
                                                       p => p.TypeAsString, 
                                                       p => p.Title));

// Grab first item
var firstItem = myList.Items.AsRequested().FirstOrDefault();
if (firstItem != null)
{
    // Load the Author and Editor fields
    var author = myList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Author");
    var editor = myList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Editor");

    // Load the user to set as Author/Editor
    var currentUser = await context.Web.GetCurrentUserAsync();
    // Define the new date for Created/Modified
    var newDate = new DateTime(2020, 10, 20);

    // Update the properties
    firstItem.Values["Author"] = author.NewFieldUserValue(currentUser);
    firstItem.Values["Editor"] = editor.NewFieldUserValue(currentUser);
    firstItem.Values["Created"] = newDate;
    firstItem.Values["Modified"] = newDate;

    // Persist the changes
    await firstItem.UpdateOverwriteVersionAsync();
}
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

## Adding a list folder

To add a folder to a list the list first must be configured to allow content types (`ContentTypesEnabled`) and allow folders (`EnableFolderCreation`). Once that's done use one of the `AddListFolder` methods to add a folder.

``` csharp
list.ContentTypesEnabled = true;
list.EnableFolderCreation = true;
await list.UpdateAsync();

// Option A: Add folder Test
await list.AddListFolderAsync("Test");


// Option B: Create path 'folderA/subfolderA'
string path = new[] {"folderA", "subfolderA" }.Aggregate(
    "",
    (aggregate, element) =>
    {
        IListItem addedFolder = list.AddListFolder(element, aggregate);
        return $"{aggregate}/{element}";
    }
);
```

## Moving a list item

You can move a list item to another folder inside it's list using one of the `MoveTo` methods:

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("My List");

// Load list item with id 1
var first = await myList.Items.GetByIdAsync(1, li => li.All, li => li.Versions);

// Move to folder folderA/subfolderA inside this list
await first.MoveToAsync("folderA/subfolderA");
```

## Sharing a list item

A list can be shared with your organization, with specific users or with everyone (anonymous), obviously all depending on how the sharing configuration of your tenant and site collection. Check out the [PnP Core SDK Sharing APIs](./sharing-listitems.md) to learn more on how you can share a list item.

## Getting changes for a list item

You can use the `GetChanges` methods on an `IListItem` to list all the changes. See [Enumerating changes that happened in SharePoint](changes-sharepoint.md) to learn more.

## Getting list item versions

Depending on the list versioning settings an `IListItem` can have multiple versions. If you want to enumerate these versions you need to first load the `Versions` property:

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("My List");

// Load list item with id 1
var first = await myList.Items.GetByIdAsync(1, li => li.All, li => li.Versions);

// Iterate over the retrieved list items
foreach (var version in first.Versions.AsRequested())
{
  // do something with the file version
}
```

## Getting the file of a list item

A document in a document library is an `IListItem` holding the file metadata with an `IFile` holding the actual file. If you have an `IListItem` you can load the connected file via `File` property:

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("My List");

// Load list item with id 1 with it's file
var first = await myList.Items.GetByIdAsync(1, li => li.All, li => li.File);

// Download the content of the actual file
byte[] downloadedContentBytes = await first.File.GetContentBytesAsync();
```

## Getting the file version of a list item version

If there's a file for the list item then there's also a version of that file for each list item version. To access that file version you need to load the `FileVersion` property on the `IListItemVersion` instance.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("My List");

// Load list item with id 1, also load the FileVersion
var first = await myList.Items.GetByIdAsync(1, li => li.All, li => li.Versions.QueryProperties(p => p.FileVersion));

// Iterate over the retrieved list items
foreach (var version in first.Versions.AsRequested())
{
  // do something with the file version, e.g. download it
  Stream downloadedContentStream = await version.FileVersion.GetContentAsync();
  downloadedContentStream.Seek(0, SeekOrigin.Begin);

  // Get string from the content stream
  string downloadedContent = await new StreamReader(downloadedContentStream).ReadToEndAsync();
}
```
