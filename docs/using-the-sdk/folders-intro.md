# Working with folders

A list or library can use folders to build a structure to manage the list items or files. Folders can also exist at the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) level, so outside the context of a list or library.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with folders
}
```

## Getting folders

Lists and libraries do have a [RootFolder property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_RootFolder) allowing you to enumerate content inside, but also the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) has a [collection of Folders](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_Folders), a [RootFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_RootFolder) and [GetFolderByIdAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_GetFolderByIdAsync_Guid_Expression_Func_PnP_Core_Model_SharePoint_IFolder_System_Object_____) and [GetFolderByServerRelativeUrlAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_GetFolderByServerRelativeUrlAsync_System_String_Expression_Func_PnP_Core_Model_SharePoint_IFolder_System_Object_____) methods. All of these approach either give you an [IFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html) or an [IFolderCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolderCollection.html) that you can use to work with.

```csharp
// Get a folder by loading it via it's server relative url
var folder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/SiteAssets"); 

// Get a folder by id
var folder = await context.Web.GetFolderByIdAsync(new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")); 

// Get a folder via the list
var folder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

// Get root folder of a library
IFolder folder = await context.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();

// Get root folder of the web (for files living outside of a document library)
IFolder folder = (await context.Web.GetAsync(p => p.RootFolder)).RootFolder;

// Get folder collection of a web and pick the SiteAssets folder
await context.Web.LoadAsync(p => p.Folders);
var folder = context.Web.Folders.AsRequested().FirstOrDefault(p=>p.Name == "SiteAssets");
```

### The Folder property bag

Each folder also has a so called property bag, a list key/value pairs providing more information about the folder. You can read this property bag, provided via the [IFolder Properties property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html#PnP_Core_Model_SharePoint_IFolder_Properties), and add new key/value pairs to it.

```csharp
// Get a folder by loading it via it's server relative url, also load the properties (= property bag)
var folder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/SiteAssets", f => f.Properties); 

// Enumerate the folder property bag
foreach(var property in folder.Properties)
{
    // Do something with the property
}

// Add a new property
folder.Properties["myPropertyKey"] = "Some value";
await folder.Properties.UpdateAsync();
```

## Adding folders

When working with folders there always is a root folder or folder collection present, a web has a root folder and folders collection and lists and libraries have a root folder. Adding folders implies adding folder as sub folder of an existing folder and this can be done by adding a new IFolder into an [IFolderCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolderCollection.html) by using the Add methods or using [EnsureFolderAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html#PnP_Core_Model_SharePoint_IFolder_EnsureFolderAsync_System_String_).

>[!Note]
> Before you can add folders to a list the list needs to be enabled for folder creation by setting the [EnableFolderCreation property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_EnableFolderCreation) to true.

```csharp
// Get the root folder of the Documents library
var folder = (await context.Web.Lists.GetByTitleAsync("Documents", p => p.RootFolder)).RootFolder;

// Add a folder 
var subFolder = await folder.Folders.AddAsync("My folder");
```

### Ensure a folder path

When adding folders a very convenient method to use is the [EnsureFolderAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html#PnP_Core_Model_SharePoint_IFolder_EnsureFolderAsync_System_String_) method as this one can take a folder path and verify each part of the path and create it if needed.

```csharp
// Get the root folder of the Site Pages library
IFolder folder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

// Below command will result in a folder hierarchy sub1/sub2/sub3/sub4/sub5, 5 levels deep
var subFolder = await folder.EnsureFolderAsync("sub1/sub2/sub3/sub4/sub5");
```

## Adding content in folders

Folders can contain other folders, but the common use case for folders is to hold files, see the [Files documentation](files-intro.md) to learn how to add a file into a folder.

## Updating folders

All but one IFolder properties are readonly, the only one that can be set is the [WelcomePage property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFolder.html#collapsible-PnP_Core_Model_SharePoint_IFolder_WelcomePage) and setting this property only makes sense on the [web's RootFolder](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_RootFolder) as that property determines the web's home page to load.

```csharp
// Check if the RootFolder property was loaded, if not request it from the server
await context.Web.EnsurePropertiesAsync(p => p.RootFolder);

// Set the WelcomePage and update the folder
context.Web.RootFolder.WelcomePage = "sitepages/myhomepage.aspx";
await context.Web.RootFolder.UpdateAsync();
```

## Renaming folders

To rename a folder you can either use the `Rename` methods or use the `MoveTo` methods.

```csharp
string folderUrl = $"{context.Uri.PathAndQuery}/Shared Documents/Test";

// Get a reference to the folder
IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(folderUrl);

// Option A: Use the Rename methods
await testFolder.RenameAsync("Test2");

// Option B: Move the folder to rename it
await testFolder.MoveToAsync($"{context.Uri.PathAndQuery}/Shared Documents/Test2");
```

## Deleting folders

```csharp
// Get the root folder of the Site Pages library
IFolder folder = (await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.RootFolder)).RootFolder;

// Add a folder 
var subFolder = await folder.Folders.AddAsync("My folder");

// Delete the folder again
await subFolder.DeleteAsync();
```

## Sharing folders

A folder can be shared with your organization, with specific users or with everyone (anonymous), obviously all depending on how the sharing configuration of your tenant and site collection. Check out the [PnP Core SDK Sharing APIs](sharing-intro.md) to learn more on how you can share a folder.

## Getting changes for a folder

You can use the `GetChanges` methods on an `IFolder` to list all the changes. See [Enumerating changes that happened in SharePoint](changes-sharepoint.md) to learn more.

## Copying or moving folders

A folder can be copied or moved into another SharePoint location and this can be done using the [CopyToAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_CopyToAsync_System_String_System_Boolean_PnP_Core_Model_SharePoint_MoveCopyOptions_) and [MoveToAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_MoveToAsync_System_String_PnP_Core_Model_SharePoint_MoveOperations_PnP_Core_Model_SharePoint_MoveCopyOptions_) methods.

```csharp
string folderUrl = $"{context.Uri.PathAndQuery}/Shared Documents/customFolder";

// Get a reference to the folder
IFolder testFolder = await context.Web.GetFolderByServerRelativeUrlAsync(folderUrl);

// Copy the Folder
await testFolder.CopyToAsync($"{context.Uri.PathAndQuery}/MyFolder");

// Move the folder
await testFolder.MoveToAsync($"{context.Uri.PathAndQuery}/MyFolder");

// Move the folder with options
await testFolder.MoveToAsync($"{context.Uri.PathAndQuery}/MyFolder",
                            new MoveCopyOptions 
                            { 
                                KeepBoth = false
                            });
```

> [!Note]
> To copy or move a folder you can use an asynchronous bulk file/folder copy/move via the `CreateCopyJobs` methods on `ISite`. See [here](sites-copymovecontent.md) for more details.


