# Working with the content type hub

Each SharePoint site uses [content types](https://support.microsoft.com/en-us/office/documents-and-libraries-in-sharepoint-8284da52-9092-4b45-90e1-1b7de6311c38?ui=en-US&rs=en-US&ad=US#id0eaabaaa=content_types&ID0EAACAAA=Content_types), a site comes pre-populated with a set of content types and a set of lists and libraries that use these content types. You can also create your own content types, either being a site content type or list content type, but you can also consume content types from a central location: the [content type hub](https://support.microsoft.com/en-us/office/what-s-changed-in-content-type-publishing-609399c7-5c42-4e25-aff0-b59d4aa1867f).

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with content types
}
```

> [!Note]
> The code snippets in this article show a regular content type but the exact same applies to [Document Sets](./contenttypes-documentsets.md) as Document Sets actually are content types as well.

## Your tenant's content type hub

When a tenant is created it already contains a site collection that will act as the content type hub. The server relative URL for this site collection is `/sites/contenttypehub`. When working with PnP Core SDK you don't have to specify this, you can use the `ContentTypeHub` property on `PnPContext` to perform operations on your content type hub.

```csharp
await context.ContentTypeHub.LoadAsync(y => y.ContentTypes);

foreach(var contentType in context.ContentTypeHub.ContentTypes.AsRequested())
{
    // Do something
}
```

## Adding, updating and deleting content types in the content type hub

You can create and maintain your content types in the content type hub just like you'd do in any other site collection, the difference here is that you use the `PnPContext` `ContentTypeHub` property to access the hub. Content types in the content type hub can be published and made available for all site collections in your tenant.

Adding a content type to the hub:

```csharp
var contentType = await context.ContentTypeHub.ContentTypes.AddAsync("0x0100302EF0D1F1DB4C4EBF58251BCCF5968F", "MyContentType");
```

Updating an existing content type in the hub:

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                       p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type to update
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Update the content type
contentType.Description = "PnP Rocks!";
await contentType.UpdateAsync();

// Adding a new field to the existing content type, mark it as required
await contentType.FieldLinks.AddAsync("MyRequiredField", required: true);
```

Deleting a content type from the hub:

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type to update
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Delete the content type
await contentType.DeleteAsync();
```

## Publishing a content type from the content type hub

Once a content type has been added to the hub is can be published, after publishing it's available for all site collections in your tenant. If you then later on update a published content type in the hub you all the sites consuming it will see the updates after you've published the updated version again. To publish a content type you need to use one of the `Publish` methods on `IContentType`.

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type to publish
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Publish the content type
await contentType.PublishAsync();
```

## Unpublishing a content type from the content type hub

Once a content type has been published to the hub it can be used by all site collections. If you want to prevent using a published content type you'll need to unpublish it. To unpublish a content type you need to use one of the `Unpublish` methods on `IContentType`.

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type to unpublish
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Publish the content type
await contentType.UnpublishAsync();
```

## Checking if a content type is published

If you want to verify if a given content type hub content type is published you can call one of the `IsPublished` methods on `IContentType`.

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type from the hub
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Is the content type already published or not?
bool isPublished = await contentType.IsPublishedAsync();

if (!isPublished)
{
    // Publish the content type
    await contentType.PublishAsync();
}
```

## Making a content type from the hub available in a site or list

Once a content type has been published in the content type hub you can consume it any site collection. This works via a pull mechanism, the first time you use the content type from the UI it's copied over to the site or list content types. If you programmatically want to use a content type hub content type you first need to pull the content type from the hub via calling one of the `AddAvailableContentTypeFromHub` methods.

```csharp
await context.ContentTypeHub.LoadAsync(p => p.ContentTypes.QueryProperties(p => p.Name, p => p.Description,
                                            p => p.FieldLinks.QueryProperties(p => p.Name)));

// Get the content type you want to add
var contentType = context.ContentTypeHub.ContentTypes.AsRequested().FirstOrDefault(p => p.Name == "MyContentType");

// Add content type from hub to site
await context.Web.ContentTypes.AddAvailableContentTypeFromHubAsync(contentType.StringId, new AddContentTypeFromHubOptions { WaitForCompletion = true });

// Add content type from hub to list
var myList = await context.Web.Lists.GetByTitleAsync("Demo");

// Ensure the list is content types enabled
myList.ContentTypesEnabled = true;
await myList.UpdateAsync();

await myList.ContentTypes.AddAvailableContentTypeFromHubAsync(contentType.StringId);
```
