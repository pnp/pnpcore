# Working with files

Working with files (documents) is a core aspect of working with SharePoint. Learn how to add/upload files, set metadata and download files again and much more.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with files
}
```

## Getting files

In PnP Core SDK files are represented via an [IFile interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html). Before you get perform a file operation (e.g. like publish or download) you need to get the file as [IFile](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html). There are a number of ways to get an [IFile](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html) like loading a single file via a lookup or enumerating the files in library / folder.

### Getting a single file

If you know the name and location of a file you can get a reference to it via the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) method named [GetFileByServerRelativeUrl](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_GetFileByServerRelativeUrlAsync_System_String_Expression_Func_PnP_Core_Model_SharePoint_IFile_System_Object_____). This method takes a server relative path of the file and optionally allows you to specify which properties to load on the file.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Get a reference to the file, loading extra properties of the IFile 
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);
```

A file can also be loaded using it's unique id using one of the `GetFileById` methods:

```csharp
Guid fileId = Guid.Parse("{D61B52DE-2ED1-41E2-B3FC-360E7286B0F9}");

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByIdAsync(fileId);

// Get a reference to the file, loading extra properties of the IFile 
IFile testDocument = await context.Web.GetFileByIdAsync(fileId, f => f.CheckOutType, f => f.CheckedOutByUser);
```

> [!Important]
> See the **Updating/reading file metadata** chapter below in case you want to update/read the metadata of retrieved file.

### Get a file using a link

Whenever you've a fully qualified link to a file or a sharing link for a file you can use the `GetFileByLink` methods to get the corresponding `IFile` object. Note that when the file happens to live in another site or web then under the covers a new `PnPContext` is instantiated and returned with that file.

```csharp
// Test regular link
IFile file2 = await context.Web.GetFileByLinkAsync($"https://contoso.sharepoint.com/sites/asite/SiteAssets/somefile.docx");

// Test sharing link
IFile file1 = await context.Web.GetFileByLinkAsync("https://contoso.sharepoint.com/:v:/s/asite/Ed2UMAoNf0tJhAjSJLt94wYBUd9U-ZhCKOoOXZcGS2dLBQ?e=KobeE5");

// Test sharing link with extra properties of the IFile
IFile file1 = await context.Web.GetFileByLinkAsync("https://contoso.sharepoint.com/:v:/s/asite/Ed2UMAoNf0tJhAjSJLt94wYBUd9U-ZhCKOoOXZcGS2dLBQ?e=KobeE5", f => f.CheckOutType, f => f.CheckedOutByUser);
```

> [!Important]
> See the **Updating/reading file metadata** chapter below in case you want to update/read the metadata of retrieved file.

### Updating/reading file metadata

If you plan to update the corresponding file metadata when you've used either the `GetFileByServerRelativeUrl`, `GetFileById` or `GetFileByLink` methods then you need to ensure the needed parent list information is loaded as part of this request. Below sample shows the minimal properties to load:

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl,
                     f => f.ListItemAllFields.QueryProperties(li=>li.All, 
                        li => li.ParentList.QueryProperties(p => p.Title, 
                            p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, 
                                                          p => p.TypeAsString, p => p.Title))));

testDocument.ListItemAllFields["Title"] = "Hello!";
await testDocument.ListItemAllFields.UpdateOverwriteVersionAsync();
// Or
// await testDocument.ListItemAllFields.UpdateAsync()
```

If you don't load the `ParentList` details then field data of complex fields (e.g. Url fields) is not loaded correctly and metadata updates might fail.

> [!Note]
> Files can have audiences set enabling page web parts to conditionally show the file depending on who loads the page. See the [Using audience targeting](audience-targeting-intro.md) page to learn more.

### Getting the file of a list item

A document in a document library is an `IListItem` holding the file metadata with an `IFile` holding the actual file. If you have an `IListItem` you can load the connected file via `File` property:

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("My List");

// Load list item with id 1 with it's file
var first = await myList.Items.GetByIdAsync(1, li => li.All, li => li.File);

// Use the loaded IFile, e.g. for downloading it
byte[] downloadedContentBytes = await first.File.GetContentBytesAsync();
```

### Enumerating files

Files do live in an [IFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html), document libraries do have a [RootFolder property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_RootFolder) allowing you to enumerate files, but also the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) has a [collection of Folders](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_Folders), a [RootFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_RootFolder) and [GetFolderByIdAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_GetFolderByIdAsync_Guid_Expression_Func_PnP_Core_Model_SharePoint_IFolder_System_Object_____) and [GetFolderByServerRelativeUrlAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_GetFolderByServerRelativeUrlAsync_System_String_Expression_Func_PnP_Core_Model_SharePoint_IFolder_System_Object_____) methods. Once you've a folder you can enumerate the files inside it.

```csharp
// Get root folder of a library
IFolder folder = await context.Web.Folders.GetFirstOrDefaultAsync(f => f.Name == "SiteAssets");

// Get root folder of the web (for files living outside of a document library)
IFolder folder = (await context.Web.GetAsync(p => p.RootFolder)).RootFolder;

// Get folder collection of a web and pick the SiteAssets folder
await context.Web.LoadAsync(p => p.Folders);
var folder = context.Web.Folders.AsRequested().FirstOrDefault(p=>p.Name == "SiteAssets");

// Load files property of the folder
await folder.LoadAsync(p => p.Files);

foreach(var file in folder.Files.AsRequested())
{
    // Do something with the file
}
```

### Finding files

If you do not know the exact name and location of on or more file and need to find on any part of the filename, you can also perform a FindFiles operation. This operation can be perfomed on an [IFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_FindFiles_System_String_) or an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_FindFiles_System_String_)

The FindFiles method accepts a string value which is matched to any part of the filename using a case insensitive regular expression and returns all found matches.

> [!Note]
> - This operation can be slow, as it iterates over all the files in the list. If performance is key, then try using a search based solution.
> - It's important to use the * to indicate any wild cards in your search string

Find files in a list:

```csharp
// Get a reference to a list
IList documentsList = await context.Web.Lists.GetByTitleAsync("Documents");

// Get files from the list whose name contains "foo"
List<IFile> foundFiles = await documentsList.FindFilesAsync("*foo*");
```

Find files in a folder:

```csharp
// Get a reference to a folder
IFolder documentsFolder = await context.Web.Folders.Where(f => f.Name == "Documents").FirstOrDefaultAsync();

// Get files from folder whose name contains "bar"
List<IFile> foundFiles = await documentsFolder.FindFilesAsync("*bar*");
```

## Getting file properties

A file in SharePoint has properties which can be requested by loading them on the [IFile](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html). Below snippet shows some ways on how to load file properties.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Sample 1: Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Sample 2: Get a reference to the file, loading the file Author and ModifiedBy properties
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, w => w.Author, w => w.ModifiedBy);

// Sample 3: Get files by loading it's folder and the containing files with their selected properties
var folder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/SiteAssets", 
                    p => p.Name, p => p.Files.QueryProperties(p => p.Name, p => p.Author, p => p.ModifiedBy));
foreach(var file in folder.Files.AsRequested())
{
    // Do something with the file, properties Name, Author and ModifiedBy are loaded
}
```

### File property bag

Each file also has a so called property bag, a list key/value pairs providing more information about the file. You can read this property bag, provided via the [IFile Properties property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#collapsible-PnP_Core_Model_SharePoint_IFile_Properties), and add new key/value pairs to it.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, load the file property bag
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Properties);

// Enumerate the file property bag
foreach(var property in testDocument.Properties)
{
    // Do something with the property
}

// Add a new property
testDocument["myPropertyKey"] = "Some value";
await testDocument.Properties.UpdateAsync();
```

## Renaming files

To rename a file you can either use the `Rename` methods or use the `MoveTo` methods.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, load the file property bag
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Option A: Use the Rename methods
await testDocument.RenameAsync("renamed document.docx");

// Option B: Move the file to rename it
await testDocument.MoveToAsync($"{context.Uri.PathAndQuery}/Shared Documents/renamed document.docx");
```

## Publishing and un-publishing files

Publishing a file will move the file from draft into published status and increase it's major version by one. Publishing can be done using the [PublishAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_PublishAsync_System_String_), un-publishing a file will bring the file back to draft status and can be done using the [UnPublishAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_UnpublishAsync_System_String_).

>[!Note]
> Publishing a file requires the library to be configured to support major versions. See the [EnableVersioning property on the IList interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_EnableVersioning).

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, load the file property bag
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Publish the file
await testDocument.PublishAsync("Optional publish message");

// Un-publish the file
await testDocument.UnpublishAsync("Optional un-publish message");
```

## Checking out, undoing check out and checking in files

In SharePoint a file can be checked out by a user to "lock" the file and then later on checked in again. The same can be done using code, including undoing a checked out of another user via the [CheckoutAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_CheckoutAsync), [CheckinAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_CheckinAsync_System_String_PnP_Core_Model_SharePoint_CheckinType_) and [UndoCheckout](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_UndoCheckoutAsync) methods.

>[!Note]
> Publishing a file requires the library to be configured to support major versions. See the [ForceCheckout property on the IList interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_ForceCheckout).

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Check out the file
await testDocument.CheckoutAsync();

// Check in the file
await testDocument.CheckinAsync();
```

Undoing a checkout:

```csharp
// Get the default document library root folder
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, load the check out information as that can be needed before undoing the check out
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.CheckOutType, f => f.CheckedOutByUser);

// Checkout the file
await testDocument.UndoCheckoutAsync();
```

## Deleting and recycling files

You can delete a file (permanent operation) or move it to the site's recycle bin (the file can be restored). Deleting a file is done using the typical Delete methods like [DeleteAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelDelete.html#PnP_Core_Model_IDataModelDelete_DeleteAsync), recycling is done via [RecycleAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_RecycleAsync).

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Recycle the file
await testDocument.RecycleAsync();

// Delete the file
await testDocument.DeleteAsync();
```

## Sharing files

A file can be shared with your organization, with specific users or with everyone (anonymous), obviously all depending on how the sharing configuration of your tenant and site collection. Check out the [PnP Core SDK Sharing APIs](sharing-intro.md) to learn more on how you can share a file.

## Adding files (=uploading)

Adding a file comes down to create a file reference and uploading the file's bytes and this can be done via the [AddAsync method on a Files collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileCollection.html#PnP_Core_Model_SharePoint_IFileCollection_AddAsync_System_String_Stream_System_Boolean_). This method takes a stream of bytes as input for the file contents.

>[!Note]
> - See the [working with large files](files-large.md) page for some more complete file upload/download samples.
> - Don't forget to load the `ListItemFields` property if you want to set the file properties after adding. This can be done in multiple ways `await addedFile.ListItemAllFields.LoadAsync()`, `await addedFile.LoadAsync(p => p.ListItemAllFields)` or `addedFile = await context.Web.GetFileByServerRelativeUrlAsync(addedFile.ServerRelativeUrl, p => p.ListItemAllFields)`.

```csharp
// Get a reference to a folder
IFolder siteAssetsFolder = await context.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();

// Upload a file by adding it to the folder's files collection
IFile addedFile = await siteAssetsFolder.Files.AddAsync("test.docx", 
                  System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestFilesFolder{Path.DirectorySeparatorChar}test.docx"));
```

## Updating file metadata

The library in which you've uploaded a file might have additional columns to store metadata about the file. To update this metadata you first need to load the `IListItem` linked to the added file via the `ListItemAllFields` property, followed by setting the metadata and updating the `IListItem`.

>[!Note]
> See the [working with list items](listitems-intro.md) page for information on how to update an `IListItem`.

```csharp
// Get a reference to a folder
IFolder documentsFolder = await context.Web.Folders.Where(f => f.Name == "Documents").FirstOrDefaultAsync();

// Upload a file by adding it to the folder's files collection
IFile addedFile = await documentsFolder.Files.AddAsync("test.docx", 
                  System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestFilesFolder{Path.DirectorySeparatorChar}test.docx"));
// Load the corresponding ListItem
await addedFile.ListItemAllFields.LoadAsync();
// Set the metadata
addedFile.ListItemAllFields["Field1"] = "Hi there";
addedFile.ListItemAllFields["Field2"] = true;
// Persist the ListItem changes
await addedFile.ListItemAllFields.UpdateAsync();
```

> [!Note]
> Files can have audiences set enabling page web parts to conditionally show the file depending on who loads the page. See the [Using audience targeting](audience-targeting-intro.md) page to learn more.

## Updating the file Author, Editor, Created or Modified properties

Each file has an `Author` property (the one who created the file), an `Editor` property (the one who last changed the file), a `Created` property (when was the file added) and a `Modified` property (when was the file changed). These are system properties and they cannot be simply overwritten. Using the `UpdateOverwriteVersion` methods this however is possible as shown in below code snippet:

```csharp
// Load the default documents folder of the site
var doc = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Fields);
// Upload a file
var file = await doc.RootFolder.Files.AddAsync("demo.docx", System.IO.File.OpenRead($".{System.IO.Path.DirectorySeparatorChar}demo.docx"), true);

// Load the file metadata again to get the ListItemFields populated
await file.LoadAsync(p => p.ListItemAllFields)

// Get a user to use as author/editor
var currentUser = await context.Web.GetCurrentUserAsync();

// The new Created/Modified date to set
var newDate = new DateTime(2020, 10, 20);

// Get the earlier loaded Author and Editor fields
var author = doc.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Author");
var editor = doc.Fields.AsRequested().FirstOrDefault(p => p.InternalName == "Editor");

// Update file properties
file.ListItemAllFields["Title"] = "new title";
file.ListItemAllFields["Created"] = newDate;
file.ListItemAllFields["Modified"] = newDate;
file.ListItemAllFields["Author"] = author.NewFieldUserValue(currentUser);
file.ListItemAllFields["Editor"] = editor.NewFieldUserValue(currentUser);

// Persist the updated properties
await file.ListItemAllFields.UpdateOverwriteVersionAsync();
```

## Downloading files

If you want to download a file you do need to use either the [GetContentAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_GetContentAsync_System_Boolean_) if you prefer a Stream as result type or [GetContentBytesAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#collapsible-PnP_Core_Model_SharePoint_IFile_GetContentBytesAsync) if you prefer a byte array.

>[!Note]
> See the [working with large files](files-large.md) page for some more complete file upload/download samples.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Download the file as stream
Stream downloadedContentStream = await testDocument.GetContentAsync();

// Download the file as an array of bytes
byte[] downloadedContentBytes = await testDocument.GetContentBytesAsync();
```

## Copying and moving files

A file can be copied or moved into another SharePoint location and this can be done using the [CopyToAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_CopyToAsync_System_String_System_Boolean_PnP_Core_Model_SharePoint_MoveCopyOptions_) and [MoveToAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_MoveToAsync_System_String_PnP_Core_Model_SharePoint_MoveOperations_PnP_Core_Model_SharePoint_MoveCopyOptions_) methods.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

// Copy the file, overwrite if existing on the destination
await testDocument.CopyToAsync($"{context.Uri.PathAndQuery}/MyDocuments/document.docx", true);

// Move the file, overwrite if needed
await testDocument.MoveToAsync($"{context.Uri.PathAndQuery}/MyDocuments/document.docx", MoveOperations.Overwrite);

// Move the file with options
await testDocument.MoveToAsync($"{context.Uri.PathAndQuery}/MyDocuments/document.docx", MoveOperations.None, 
                            new MoveCopyOptions 
                            { 
                                KeepBoth = true, 
                                RetainEditorAndModifiedOnMove = true 
                            });
```

> [!Note]
> You can also opt for an asynchronous bulk file/folder copy/move via the `CreateCopyJobs` methods on `ISite`. See [here](sites-copymovecontent.md) for more details.

## Converting files

Some files can be transformed into another format by calling one of the `ConvertTo` methods.

> [!Note]
> Loading the `VroomDriveID` and  `VroomItemID` when you load the file to convert optimizes performance, these properties will be fetched if not present.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, p => p.VroomItemID, p => p.VroomDriveID);

// Convert the Word document to PDF, this returns a stream
var pdfContent = await testDocument.ConvertToAsync(new ConvertToOptions { Format = ConvertToFormat.Pdf });

// Get a reference to a folder to upload the PDF
IFolder siteAssetsFolder = await context.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();

// Upload content
await siteAssetsFolder.Files.AddAsync("document.pdf", pdfContent, true);
```

The following values are valid transformation targets and their supported source extensions:

| Target | Description                        | Supported source extensions
|:------|:-----------------------------------|---------------------------------
| glb   | Converts the item into GLB format  | cool, fbx, obj, ply, stl, 3mf
| html  | Converts the item into HTML format | eml, md, msg
| jpg   | Converts the item into JPG format  | 3g2, 3gp, 3gp2, 3gpp, 3mf, ai, arw, asf, avi, bas, bash, bat, bmp, c, cbl, cmd, cool, cpp, cr2, crw, cs, css, csv, cur, dcm, dcm30, dic, dicm, dicom, dng, doc, docx, dwg, eml, epi, eps, epsf, epsi, epub, erf, fbx, fppx, gif, glb, h, hcp, heic, heif, htm, html, ico, icon, java, jfif, jpeg, jpg, js, json, key, log, m2ts, m4a, m4v, markdown, md, mef, mov, movie, mp3, mp4, mp4v, mrw, msg, mts, nef, nrw, numbers, obj, odp, odt, ogg, orf, pages, pano, pdf, pef, php, pict, pl, ply, png, pot, potm, potx, pps, ppsx, ppsxm, ppt, pptm, pptx, ps, ps1, psb, psd, py, raw, rb, rtf, rw1, rw2, sh, sketch, sql, sr2, stl, tif, tiff, ts, txt, vb, webm, wma, wmv, xaml, xbm, xcf, xd, xml, xpm, yaml, yml
| pdf   | Converts the item into PDF format  | doc, docx, epub, eml, htm, html, md, msg, odp, ods, odt, pps, ppsx, ppt, pptx, rtf, tif, tiff, xls, xlsm, xlsx

## Getting file versions

When versioning on a file is enabled a file can have multiple versions and PnP Core SDK can be used to work with the older file versions. Each file version is represented via an [IFileVersion](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileVersion.html) in an [IFileVersionCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileVersionCollection.html). Loading file versions can be done by requesting the [Versions property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_Versions) of the file. Once you've an [IFileVersion](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileVersion.html) you can also download that specific version of the file by using one of the GetContent methods as shown in the example.

>[!Note]
> For a file to have versions the library needs to be configured to support major versions and/or minor versions. See the [EnableVersioning](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_EnableVersioning) and [EnableMinorVersions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_EnableMinorVersions) properties on the IList interface.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, also request the Versions property
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.Versions);

foreach(var fileVersion in testDocument.Versions)
{
    // Download the file version content as stream
    Stream fileVersionContent = await fileVersion.GetContentAsync();
}
```

## Getting thumbnails for a file

If you need to present a SharePoint file using a thumbnail and some basic file information (like `Title`) you can use one of the `GetThumbnail` methods. These methods optionally use an `ThumbnailOptions` class allowing you to specify one or more standard or custom thumbnail sizes. Once the call has ended you get a collection of `IThumbnail` instances each having a thumbnail URL completed with the thumbnail size.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, also request the Graph identifiers (VroomItemId and VroomDriveId) to 
// avoid an extra server roundtrip in the GetThumbnailsAsync call
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.VroomItemID, f => f.VroomDriveID);

// Define the thumbnails to generate
ThumbnailOptions options = new()
{
    // Standard sized thumbnails
    StandardSizes = new List<ThumbnailSize>
    {
        ThumbnailSize.Medium,
        ThumbnailSize.Large
    },

    // Custom sized thumbnails
    CustomSizes = new List<CustomThumbnailOptions>
    {
        new CustomThumbnailOptions
        {
            Width = 200,
            Height = 300,                                
        },
        new CustomThumbnailOptions
        {
            Width = 400,
            Height = 500,
            Cropped = true,
        },
    }
};

var thumbnails = await testDocument.GetThumbnailsAsync(options);

foreach(var thumbnail in thumbnails)
{
    // use thumbnail.Url 
}
```

> [!Note]
> Thumbnails can only be retrieved for files living in a document library. For pages in a pages library this will not work.

## Getting analytics

Using one of the `GetAnalytics` methods on `IFile` gives you back the file analytics for all time, the last seven days or for a custom interval of your choice. The returned `List<IActivityStat>` contains one row for the all time and seven days statistic requests, if you've requested a custom interval you also choose an aggregation interval (day, week, month) and depending on the interval and aggregation you'll get one or more rows with statistics.

> [!Note]
> Loading the `VroomDriveID` and  `VroomItemID` when you load the file to get statistics for optimizes performance, these properties will be fetched if not present.

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync($"{context.Uri.AbsolutePath}/sitepages/home.aspx", p => p.VroomItemID, p => p.VroomDriveID);

// Get analytics for all time
var analytics = await file.GetAnalyticsAsync();

// Get analytics for the last 7 days
var analytics = await file.GetAnalyticsAsync(new AnalyticsOptions { Interval = AnalyticsInterval.LastSevenDays });

// Get analytics for a custom interval for 11 days --> you'll see 11 rows with statistic data, one per day
DateTime startDate = DateTime.Now - new TimeSpan(20, 0, 0, 0);
DateTime endDate = DateTime.Now - new TimeSpan(10, 0, 0, 0);

var analytics = await file.GetAnalyticsAsync(
                new AnalyticsOptions
                {
                    Interval = AnalyticsInterval.Custom,
                    CustomStartDate = startDate,
                    CustomEndDate = endDate,
                    CustomAggregationInterval = AnalyticsAggregationInterval.Day
                });
```

> [!Note]
> The value of the `CustomStartDate` and `CustomEndDate` parameters must represent a time range of less than 90 days.

## Getting an embeddable preview url

Using on of the `GetPreview` methods allows you to obtain a short-lived embeddable URL for a file in order to render a temporary preview.

> [!Note]
> Loading the `VroomDriveID` and  `VroomItemID` when you load the file to get an embeddable URL optimizes performance, these properties will be fetched if not present.

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, also request the Versions property
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, p => p.VroomItemID, p => p.VroomDriveID);

// Get Preview URL
var filePreview = await testDocument.GetPreviewAsync(new PreviewOptions { Page = "2" });

// Use outcome, e.g. use filePreview.GetUrl in an IFRAME to show the file preview
```

## Getting file IRM settings

A SharePoint document library can be configured with an [Information Rights Management (IRM) policy](https://docs.microsoft.com/en-us/microsoft-365/compliance/set-up-irm-in-sp-admin-center?view=o365-worldwide) which then stamps an IRM policy on the documents obtained from that library. Use the [InformationRightsManagementSettings](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_InformationRightsManagementSettings) property to read the file's IRM settings.

>[!Note]
> The library holding files you want to protect need to be first setup for IRM by enabling IRM on it via the [IrmEnabled property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#collapsible-PnP_Core_Model_SharePoint_IList_IrmEnabled).

```csharp
string documentUrl = $"{context.Uri.PathAndQuery}/Shared Documents/document.docx";

// Get a reference to the file, also request the Versions property
IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl, f => f.InformationRightsManagementSettings);

var fileAccessExpirationTimeInDays = testDocument.InformationRightsManagementSettings.DocumentAccessExpireDays;
```
