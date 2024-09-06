# Using audience targeting

Audience targeting helps the most relevant content get to the right audiences. By enabling audience targeting, specific content will be prioritized to specific audiences through SharePoint web parts, page libraries, and navigational links. See [here](https://support.microsoft.com/en-us/office/target-content-to-a-specific-audience-on-a-sharepoint-site-68113d1b-be99-4d4c-a61c-73b087f48a81) to learn more. In SharePoint Online audiences are equivalent to Groups in Azure AD, the "classic" audience system is not supported by PnP Core SDK. The guids shown on this page are id's of Azure AD groups.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with navigation
}
```

## In navigation

Navigation nodes can have up to 10 audiences to have them show up for the right audiences. Before adding audiences to navigation nodes you first need to ensure that the site's navigation is configured for using audiences by setting the `NavAudienceTargetingEnabled` property to `true`.

```csharp
await context.Web.EnsurePropertiesAsync(w => w.NavAudienceTargetingEnabled);
context.Web.NavAudienceTargetingEnabled = true;
await context.Web.UpdateAsync();
```

Once the site has been configured you can add new navigation nodes with one or more audiences. Below sample adds a new node to the site's left (`QuickLaunch`) navigation, if the site uses a top (`TopNavigationBar`) navigation then the same approach applies.

```csharp
var node = await context.Web.Navigation.QuickLaunch.AddAsync(
                new NavigationNodeOptions
                {
                    Title = "My node",
                    Url = "/sites/demo/mylibrary",
                    AudienceIds = new List<Guid> { Guid.Parse("7bf72917-4c72-4a83-91d6-1362fcf7222a"),  Guid.Parse("0402aa20-e67a-47e3-bad4-03801247be9e") }
                });
```

You can also update the set audiences:

```csharp
// Load the node to update
var node = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1204);

// Update the node by adding an audience
node.AudienceIds = new System.Collections.Generic.List<Guid> { Guid.Parse("7bf72917-4c72-4a83-91d6-1362fcf7222a") };
await node.UpdateAsync();
```

Or remove the set audiences:

```csharp
// Load the node to update
var node = await context.Web.Navigation.QuickLaunch.GetByIdAsync(1204);

// Remove the audiences again
node.AudienceIds.Clear();
await node.UpdateAsync();
```

## For files and list items in libraries and lists

Some web parts (e.g. news web part, high lighted content web part, ...) do support audience targeting. This works by defining audiences on the content (pages, documents) shown by these web parts and to so you first need to enable audience targeting for the respective pages and document libraries by using one of the `EnableAudienceTargeting` methods. Once that's done you need to use the `EnsureUser` method to resolve an Azure AD Group id into a user and then update the `_ModernAudienceTargetUserField` list item field of your document or page.

```csharp
// Load library to update
var myList = await context.Web.Lists.GetByTitleAsync("MyLibrary", 
                                                    p => p.Title, 
                                                    p => p.RootFolder,
                                                    p => p.Fields.QueryProperties(p => p.InternalName,
                                                                                  p => p.FieldTypeKind,
                                                                                  p => p.TypeAsString,
                                                                                  p => p.Title));
// Enable audience targeting
await myList.EnableAudienceTargetingAsync();

// Upload file to library
IFile addedFile = await myList.RootFolder.Files.AddAsync("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}test.docx"));

// Resolve Azure AD Groups for use as audiences using a single roundtrip
var batch = context.NewBatch();
var myUser1 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|06ed1f73-c58d-45e8-ad07-66f4d1eed723");
var myUser2 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|7bf72917-4c72-4a83-91d6-1362fcf7222a");
var myUser3 = await context.Web.EnsureUserBatchAsync(batch, "c:0o.c|federateddirectoryclaimprovider|0402aa20-e67a-47e3-bad4-03801247be9e");
await context.ExecuteAsync(batch);

// Load the file metadata first
await addedFile.LoadAsync(p => p.ListItemAllFields.QueryProperties(li => li.All));

// Set audiences for uploaded file
var userCollection = new FieldValueCollection();
userCollection.Values.Add(new FieldUserValue(myUser1));
userCollection.Values.Add(new FieldUserValue(myUser2));
userCollection.Values.Add(new FieldUserValue(myUser3));

addedFile.ListItemAllFields.Values.Add("_ModernAudienceTargetUserField", userCollection);
await addedFile.ListItemAllFields.UpdateAsync();

```
