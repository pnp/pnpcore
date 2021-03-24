# Working with content types

Each SharePoint site uses [content types](https://support.microsoft.com/en-us/office/documents-and-libraries-in-sharepoint-8284da52-9092-4b45-90e1-1b7de6311c38?ui=en-US&rs=en-US&ad=US#id0eaabaaa=content_types&ID0EAACAAA=Content_types), a site comes pre-populated with a set of content types and a set of lists and libraries that use these content types. You can also create your own content types, either being a site content type or list content type. A site content type can be reused across multiple lists in the site collection and this is the preferred model, adding a list content type however is possible as well. Creating site and list content types is from a developer point of view quite similar so both options are discussed together in this article.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with content types
}
```

## Getting content types

To get the existing content types you need to load the [site's ContentTypes collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_ContentTypes) or the [list's content type collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_ContentTypes).

```csharp
// Get site content types, also load the content types field links in one go
await context.Web.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                                p => p.FieldLinks.QueryProperties(p => p.Name)));
var contentTypes = context.Web.ContentTypes;                                                

// Get list content types
var contentTypes = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.ContentTypes)).ContentTypes;

foreach (var contentType in contentTypes.AsRequested())
{
    // do something
}
```

## Adding content types

Adding a content type is done by using one of the Add methods on the [site's ContentTypes collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_ContentTypes) or the [list's content type collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_ContentTypes). When adding a content type you need to specify a content type id as explained in the [Content Type IDs documentation](https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/aa543822(v=office.14)).

>[!Note]
> Before you can add content types to a list the list needs to be enabled for content types by setting the [ContentTypesEnabled property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_ContentTypesEnabled) to true.

```csharp
// Add a site content type
var contentType = await context.Web.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "MyContentType");

// Add a list content type, start with getting a reference to the list
var list = await context.Web.Lists.GetByTitleAsync("Documents", p => p.ContentTypes);

// Ensure content type are enabled for the list
list.ContentTypesEnabled = true;
await list.UpdateAsync();

// Add the content type
await list.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "MyContentType");
```

### Adding fields to the content type

Once the content type is added you typically also want to add fields to it, this is done by adding the needed fields in the [content type's IFieldLinkCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IContentType.html#collapsible-PnP_Core_Model_SharePoint_IContentType_FieldLinks) via the [AddAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldLinkCollection.html#collapsible-PnP_Core_Model_SharePoint_IFieldLinkCollection_Add_System_String_System_String_System_Boolean_System_Boolean_System_Boolean_System_Boolean_).

```csharp
// Get site content types, also load the content types field links in one go
await context.Web.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                    p => p.FieldLinks.QueryProperties(p => p.Name)));
var contentTypes = context.Web.ContentTypes;

// Get the content type to update
var contentType = contentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Add existing field with internal name "MyField" to the content type's field link collection
await contentType.FieldLinks.AddAsync("MyField");
```

## Updating content types

Updating a content type comes down to getting a reference to the content type to update, update the needed properties and call the UpdateAsync method.

```csharp
await context.Web.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                    p => p.FieldLinks.QueryProperties(p => p.Name)));
var contentTypes = context.Web.ContentTypes;

// Get the content type to update
var contentType = contentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Update the content type
contentType.Description = "PnP Rocks!";
await contentType.UpdateAsync();

// Adding a new field to the existing content type, mark it as required
await contentType.FieldLinks.AddAsync("MyRequiredField", required: true);
```

## Deleting content types

To delete a content type you need to get a reference to the content type to delete followed by calling one of the Delete methods.

```csharp
await context.Web.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                    p => p.FieldLinks.QueryProperties(p => p.Name)));
var contentTypes = context.Web.ContentTypes;

// Get the content type to update
var contentType = contentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Delete the content type
await contentType.DeleteAsync();
```
