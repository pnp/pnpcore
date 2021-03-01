# Working with large files

SharePoint [supports up to 100GB files](https://docs.microsoft.com/en-us/office365/servicedescriptions/sharepoint-online-service-description/sharepoint-online-limits#file-size-and-file-path-length) and uploading and downloading those requires some additional steps.

## Uploading large files

Uploading of files is done via the [AddAsync method on a Files collection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileCollection.html#PnP_Core_Model_SharePoint_IFileCollection_AddAsync_System_String_Stream_System_Boolean_). This method takes a stream of bytes as input for the file contents and when working with files larger than 10MB this API will automatically switch to a chunked upload, meaning it will upload the large file in chunks of 10MB. For a developer no special actions are required.

```csharp
// Get a reference to a folder
IFolder siteAssetsFolder = await context.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();

// Upload a file by adding it to the folder's files collection, the file will be uploaded in chunks of 10MB
IFile addedFile = await siteAssetsFolder.Files.AddAsync("2gbfile.test", 
                  System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestFilesFolder{Path.DirectorySeparatorChar}2gbfile.test"));
```

## Downloading large files

If you want to download a large file you do need to use the [GetContentAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_GetContentAsync_System_Boolean_) as that method allows you to specify you want to use a streamed download by specifying this in the [GetContentAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFile.html#PnP_Core_Model_SharePoint_IFile_GetContentAsync_System_Boolean_). In a streamed download the API will return the call whenever the first bytes have arrived from the server allowing you to process the file in chunks without the full file being loaded in the process's memory.

>[!Important]
> The default HTTP timeout is 100 seconds, which is not enough for large file downloads. You can increase this time out in the [PnP Core SDK configuration](basics-settings.md) up to an infinite timeout.

```csharp
IFolder parentFolder = await context.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();

// Get a reference to the file to download
IFile fileToDownload = await context.Web.GetFileByServerRelativeUrlAsync($"{parentFolder.ServerRelativeUrl}/2gbfile.test");

// Start the download
Stream downloadedContentStream = await fileToDownload.GetContentAsync(true);

// Download the file bytes in 2MB chunks and immediately write them to a file on disk 
// This approach avoids the file being fully loaded in the process memory
var bufferSize = 2 * 1024 * 1024;  // 2 MB buffer
using (var content = System.IO.File.Create("2gb.test.downloaded"))
{
    var buffer = new byte[bufferSize];
    int read;
    while ((read = await downloadedContentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
    {
        content.Write(buffer, 0, read);
    }
}
```

> [!Note]
> The above also applies to downloading older versions of large files, the same GetContent methods exist on the [IFileVersion](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFileVersion.html) model.
