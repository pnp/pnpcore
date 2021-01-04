# Working with lists

All data in SharePoint lives in lists: list items belong is lists and documents belong in document libraries which are based upon lists. Being able to find a list is needed to work with the list content (items, documents) and adding/updating lists is often used by apps working with SharePoint.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with lists
}
```

Since a list can be seen as a container defining a schema (fields) and having content (list items, documents, folders or pages) most of the relevant documentation lives elsewhere:

Scenario | List property/method | Documentation
---------|----------------------|------------
List schema: fields | [Fields](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Fields) | [Fields documentation](fields-intro.md)
List schema: views | [Views](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Views) | [List view documentation](lists-views.md)
List schema: content types | [ContentTypes](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_ContentTypes) | [Content types documentation](contenttypes-intro.md)
Content: items | [Items](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) | [List item documentation](listitems-intro.md)
Content: files | [Items](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) | [Files documentation](files-intro.md)
Content: pages | [Items](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) | [Pages documentation](pages-intro.md)
Content: folders | [Items](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Items) | [Folders documentation](folders-intro.md)

## Getting lists

Lists live inside an [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) and you can either get a specific list or enumerate the available lists.

### Getting a specific list

To get a specific list you can use multiple methods on the [IListCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html#PnP_Core_Model_SharePoint_IListCollection_GetById_Guid_Expression_Func_PnP_Core_Model_SharePoint_IList_System_Object_____): [GetByServerRelativeUrlAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html#collapsible-PnP_Core_Model_SharePoint_IListCollection_GetByServerRelativeUrlAsync_System_String_Expression_Func_PnP_Core_Model_SharePoint_IList_System_Object_____), [GetByTitleAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html#PnP_Core_Model_SharePoint_IListCollection_GetByTitleAsync_System_String_Expression_Func_PnP_Core_Model_SharePoint_IList_System_Object_____) or [GetByIdAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html#PnP_Core_Model_SharePoint_IListCollection_GetByIdAsync_Guid_Expression_Func_PnP_Core_Model_SharePoint_IList_System_Object_____). Alternatively you can also write a LINQ query. Mentioned approaches are shown in below sample code.

```csharp
// Get Documents list via title
var myList = await context.Web.Lists.GetByTitleAsync("Documents");

// Get Documents list via title
var myList = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/Shared Documents");

// Get Documents list via id, only load the needed properties
var myList = await context.Web.Lists.GetByIdAsync(new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5"), p => p.TemplateType, p => p.Title);

// Query on the collection
var myList = await context.Web.Lists.GetFirstOrDefaultAsync(p=>p.Title == "Documents");
```

### Enumerating lists

Loading all lists is needed when you don't upfront know which list you want to process, or maybe you want to run over all lists of a given type. Doing an enumeration can be done by requesting the [Lists property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_Lists) of an IWeb or via a LINQ query.

```csharp
// Sample 1: Load all lists in the web with their default properties
await context.Web.GetAsync(p => p.Lists);

// Sample 2: Load the web content types + all lists with their content types and the content type field links
await context.Web.GetAsync(p => p.Title,
                           p => p.ContentTypes.LoadProperties(p => p.Name),
                           p => p.Lists.LoadProperties(p => p.Id,
                                                       p => p.TemplateType,
                                                       p => p.Title,
                                                       p => p.DocumentTemplate,
                               p => p.ContentTypes.LoadProperties(p => p.Name,
                                    p => p.FieldLinks.LoadProperties(p => p.Name)))
                          );

// Process the document libraries
foreach(var list in context.Web.Lists.Where(p => TemplateType == ListTemplateType.DocumentLibrary))
{
    // Use the list
}
```

Using LINQ to load multiple lists is also possible:

```csharp
// Turning off the Graph first feature (temp fix, will be not be needed by GA)
context.GraphFirst = false;

// Option 1 to write the LINQ query
var query = (from i in context.Web.Lists
                where i.TemplateType == ListTemplateType.DocumentLibrary
                select i).Load(p => p.Title, p => p.Id);                             

// Option 2 to write the LINQ query
var query = context.Web.Lists.Where(p => p.TemplateType == ListTemplateType.DocumentLibrary)
                             .Load(p => p.Title, p => p.Id);

// Execute the LINQ query                
var lists = await query.ToListAsync();

foreach(var list in lists)
{
    // Use the list
}
```

## Adding lists

Adding lists comes down to adding a new list to the Web's [IListCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_Lists) using the [AddAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html#PnP_Core_Model_SharePoint_IListCollection_AddAsync_System_String_PnP_Core_Model_SharePoint_ListTemplateType_).

```csharp
// Add a list
var myList = await context.Web.Lists.AddAsync("MyList", ListTemplateType.GenericList);

// Add a document library
var myDocumentLibrary = await context.Web.Lists.AddAsync("myDocumentLibrary", ListTemplateType.DocumentLibrary);
```

## Updating lists

A list has a lot of properties to update and updating them comes down to setting the new property value and then calling one of the update methods like UpdateAsync.

```csharp
// Get the list to update
var myList = await context.Web.Lists.GetByTitleAsync("List to update");

// Update a list property
myList.Description = "PnP Rocks!";

// Send update to the server
await myList.UpdateAsync();
```

## Deleting lists

To delete a list you can either permanently delete the list using the DeleteAsync method or you can move the list into the site's recycle bin using the [RecycleAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_RecycleAsync).

```csharp
// Get the list to delete
var myList = await context.Web.Lists.GetByTitleAsync("List to delete");

// Delete the list
await myList.DeleteAsync();

// Recycle the list
await myList.RecycleAsync();
```
