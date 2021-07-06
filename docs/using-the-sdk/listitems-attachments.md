# Working with list item attachments

List items can have one or more attachments. This chapter explains how to load the attachments, how to add them and how to delete or recycle them.

> [!Note]
> Some of the samples assume you've loaded a list into the variable `mylist`, the code that shows how to do so is listed in the first examples.

## Loading the list item attachments

List item attachments are handled via the `AttachmentFiles` property of an `IListItem`. Simply requesting this property will get you the needed information about all the attachments on the `IListItem`. Once you've requested the attachments you can also download them using one of the `GetContent` methods.

```csharp
// Load the list item with attachments
var item = await mylist.Items.GetByIdAsync(1, p => p.AttachmentFiles);

// Enumerate over the attachments and download them
foreach (var attachment in item.AttachmentFiles.AsRequested())
{
    // Download the attachment as stream
    Stream downloadedContentStream = await attachment.GetContentAsync();

    // Download the attachment as an array of bytes
    byte[] downloadedContentBytes = await attachment.GetContentBytesAsync();
}
```

## Adding a list item attachment

Adding attachments to an `IListItem` can be done using the `Add` methods on the attachment collection.

```csharp
// Load the list item with attachments
var item = await mylist.Items.GetByIdAsync(1, p => p.AttachmentFiles);

// Add an attachment by adding it the attachment collection
var fileName = TestCommon.GetPnPSdkTestAssetName("test_added.docx");
var addedAttachment = await itemLoaded.AttachmentFiles.AddAsync("test.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}test.docx"));
```

## Deleting or recycling list item attachments

To remove a list item attachment you can either delete it, which is permanent, or recycle it so that it can still be restored.

```csharp
// Load the list item with attachments
var item = await mylist.Items.GetByIdAsync(1, p => p.AttachmentFiles);

// Delete the attachment
await item.AttachmentFiles.AsRequested().First().Delete();

// Recycle the attachment
await item.AttachmentFiles.AsRequested().First().Recycle();
```
