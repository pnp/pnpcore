# Working with Document Sets

A [Document Set](https://support.microsoft.com/en-us/office/introduction-to-document-sets-3dbcd93e-0bed-46b7-b1ba-b31de2bcd234) is a group of related documents that you can manage as a single entity. Looking at it's implementation a Document Set is based upon content types, it's a special content type with some specific Document Set settings.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with content types
}
```

## Loading a content type as Document Set

All Document Sets in need to inherit from the base Document Set content type, which has as content type id `0x0120D520`. To access the Document Set functionality for an existing content type you first need to load it as a Document Set via one of the `AsDocumentSet` methods.

```csharp
// Load the base Document Set content type
IContentType contentType = await (from ct in context.Web.ContentTypes
                                  where ct.StringId == "0x0120D520"
                                  select ct)
            .QueryProperties(ct => ct.StringId, ct => ct.Id)
            .FirstOrDefaultAsync();

var documentSet = await contentType.AsDocumentSetAsync();

// check the settings
foreach(var sharedColumn in documentSet.SharedColumns)
{    
}
```

## Adding a Document Set

To add a new Document Set you can use one of the `AddDocumentSet` methods on the `IContentTypeCollection` you're working with. While doing so you can provide the specific Document Set configuration via an `DocumentSetOptions` instance as shown in below sample.

```csharp
// Load the needed fields that will serve as Shared Field or Welcome Field in the Document Set
var categoriesField = await context.Web.Fields.FirstAsync(y => y.InternalName == "Categories").ConfigureAwait(false);
var managersField = await context.Web.Fields.FirstAsync(y => y.InternalName == "ManagersName").ConfigureAwait(false);

// Load the content types that are allowed to be added to the Document Set
var documentCt = await context.Web.ContentTypes.FirstAsync(y => y.Name == "Document").ConfigureAwait(false);
var formCt = await context.Web.ContentTypes.FirstAsync(y => y.Name == "Form").ConfigureAwait(false);

// Load the template files that will be used as default content for the Document Set
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/hrsite/cv/template.docx").ConfigureAwait(false);

// Prepare the Document Set options
var documentSetOptions = new DocumentSetOptions
{
    AllowedContentTypes = new List<IContentType>
    {
        documentCt,
        formCt
    },
    ShouldPrefixNameToFile = true,
    PropagateWelcomePageChanges = true,
    SharedColumns = new List<IField>
    {
        managersField,
        categoriesField
    },
    WelcomePageColumns = new List<IField>
    {
        managersField,
        categoriesField
    },
    DefaultContents = new List<DocumentSetContentOptions>
    {
        new DocumentSetContentOptions
        {
            FileName = "template.docx",
            FolderName = "Templates",
            File = file,
            ContentTypeId = documentCt.StringId
        }
    }
};

// Create the Document Set
IDocumentSet newDocumentSet = await context.Web.ContentTypes.AddDocumentSetAsync(docSetId, "My New Document Set", "", "Custom Document Sets", documentSetOptions);
```

## Updating a Document Set

Updating a Document Set is fairly similar to adding one, first you ensure you've an `IDocumentSet` instance for the Document Set you want to update, followed by passing in the specific Document Set configuration via an `DocumentSetOptions` instance which is applied via a call to one of the `Update` methods.

```csharp
IContentType contentType = await (from ct in context.Web.ContentTypes
                                  where ct.Name == "My New Document Set"
                                  select ct)
            .QueryProperties(ct => ct.StringId, ct => ct.Id)
            .FirstOrDefaultAsync();

var documentSet = await contentType.AsDocumentSetAsync();

// Load the needed fields that will serve as Shared Field or Welcome Field in the Document Set
var managersField = await context.Web.Fields.FirstAsync(y => y.InternalName == "ManagersName").ConfigureAwait(false);
// Load the template files that will be used as default content for the Document Set
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/hrsite/cv/template2.docx").ConfigureAwait(false);

// Prepare the Document Set options
var documentSetOptionsUpdate = new DocumentSetOptions
{
    SharedColumns = new List<IField>
    {
        managersField
    },
    WelcomePageColumns = new List<IField>
    {
        managersField
    },
    DefaultContents = new List<DocumentSetContentOptions>
    {
        new DocumentSetContentOptions
        {
            FileName = "template2.docx",
            FolderName = "Templates",
            File = file,
            ContentTypeId = documentCt.StringId
        }
    }
};

// Update the Document Set
documentSet = await documentSet.UpdateAsync(documentSetOptionsUpdate);
```

## Deleting a Document Set

Given a Document Set is a content type, deleting a Document Set is identical to deleting a content type. If you have an `IDocumentSet` reference you obtain the connected content type via the `Parent` property and once you have that you can delete the Document Set content type.

```csharp
// Option A: directly delete the Document Set content type
IContentType contentType = await (from ct in context.Web.ContentTypes
                                  where ct.Name == "My New Document Set"
                                  select ct)
            .QueryProperties(ct => ct.StringId, ct => ct.Id)
            .FirstOrDefaultAsync();

await contentType.DeleteAsync();

// Option B: start from an IDocumentSet
// Assume IDocumentSet newDocumentSet is set

var contentType = newDocumentSet.Parent as IContentType
await contentType.DeleteAsync();
```

> [!Note]
> If a content type is in use you cannot delete it.
