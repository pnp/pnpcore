# Copying and moving files

When you want to copy or move files between libraries then you synchronously copy or move file by file but that's not convenient for bulk transfers. If you need to move multiple files or folders then using the asynchronous copy/move functionality is strongly recommended.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with sites
}
```

## Copying/moving multiple files

To copy or move one or more files between libraries (libraries can live in different site collections) you need to use one of the `CreateCopyJobs` methods on `ISite`. You specify the copy/move operation via creating a `CopyMigrationOptions` instance: to move files you need to set `IsMoveMode` to `true`, otherwise a copy will be performed. As this copy/move feature works asynchronously in SharePoint Online using the `CreateCopyJobs` method will add the copy/move job to a queue and control is returned to your code. If you want to wait for a copy/move job to finish you do have two options: using one of the `EnsureCopyJobHasFinished` will make your code wait until the passed in job is done, alternatively you can also specify you want to wait for the job via the `waitUntilFinished` optional parameter of the `CreateCopyJobs` methods. Sometimes waiting for a job is not ideal as it's a blocking operation, for those cases you can use one of the `GetCopyJobProgress` methods to retrieve the status of individual copy/move requests from an earlier submitted copy/move job.

```csharp
// Launch a job that will copy 2 documents to a new library
string destinationAbsoluteUrl = $"{context.Uri}/Target Library";
var jobUris = new List<string> { context.Uri + "/Shared Documents/document1.docx", context.Uri + "/Shared Documents/document2.docx" };
var copyJobs = await context.Site.CreateCopyJobsAsync(jobUris.ToArray(), destinationAbsoluteUrl, new CopyMigrationOptions
{
    AllowSchemaMismatch = true,
    AllowSmallerVersionLimitOnDestination = true,
    IgnoreVersionHistory = true,
    // Note: set IsMoveMode = true to move the file(s)
    IsMoveMode = false,
    BypassSharedLock = true,
    ExcludeChildren = true,
    NameConflictBehavior = SPMigrationNameConflictBehavior.Replace
});

// Option A: Wait for the async copy to be finished
await context.Site.EnsureCopyJobHasFinishedAsync(copyJob);

// Option B: check for the status of the copy/move job
// The output from CreateCopyJobsAsync is a list of ICopyMigrationInfo, so when checking progress you might 
// want to check all jobs
foreach(var copyJob in copyJobs)
{
    var progress = await GetCopyJobProgressAsync(copyJob);
    if (progress.JobState == MigrationJobState.None)
    {
        // The job is done!
    }
    else if (progress.JobState == MigrationJobState.Processing)
    {
        // The job is running
    }
    else 
    {
        // The job is queued
    }
}
```

## Copying/moving folders

Just like you can copy/move files you can also copy/move folders and the files living inside those folders. Instead of file names you simply specify folder names.

## Limitations

The underlying used API is described [here](https://docs.microsoft.com/en-us/sharepoint/dev/apis/spod-copy-move-api), key limitations to be aware of are:

What | Limitation
-----|-----------
File size | A file must be less than 2 GB.
Number of items | No more than 30,000 items in a job.
Total size of job | Job size not to exceed 100 GB.

> [!Important]
> Also do note that when using delegated permissions your account must have permissions on both target and source location. When using application permissions you need `Sites.FullControl.All` to enable all target locations. When using `Sites.Manage.All` you'll not be able to copy across site collections.
