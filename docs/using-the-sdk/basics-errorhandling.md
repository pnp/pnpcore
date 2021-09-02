# Error handling

PnP Core SDK, like any other library, will throw exceptions when something unexpected happens. Typically exceptions are throw due to failing service calls or configuration mistakes. PnP Core SDK uses different exception types allowing you to have more control over the exception.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext
}
```

## What type of exceptions are thrown by PnP Core SDK

Exceptions being thrown can be grouped into a number of buckets:

- Standard .NET exceptions (e.g. `ArgumentNullException`, `ArgumentOutOfRangeException`, `InvalidCastException`, `InvalidOperationException`, `ApplicationException`, ...) when validating user input and running
- Standard .NET exceptions for currently unsupported features (e.g. `NotSupportedException`, `NotImplementedException`)
- PnP Core SDK specific exceptions for general SDK failures: `ClientException`
- PnP Core SDK specific exceptions for authentication failures: `AuthenticationException`
- PnP Core SDK specific exceptions for service calls:
  - For failing SharePoint REST calls: `SharePointRestServiceException`
  - For failing Microsoft Graph calls: `MicrosoftGraphServiceException`
  - For failing SharePoint CSOM calls: `CsomServiceException`

All PnP Core exceptions inherit from `PnPException`, for service calls there's an extra base exception named `ServiceException` which enables you to filter out all service call related exceptions.

## Catching a PnP Core SDK exception and getting the exception details

All PnP Core SDK specific exceptions inherit from `PnPException` making it easy for you to filter them out. Also each `PnPException` does have an `Error` property holding detailed exception information. As both `PnPException` is an abstract base class you'd typically want to work with the actual exceptions like `ClientException`, `AuthenticationException`, `SharePointRestServiceException`, `MicrosoftGraphServiceException` and `CsomServiceException` as shown in below code snippets. The first snippet shows how to log the detailed exception information:

```csharp
try
{
    // Make service call    
}
// Catch all service exceptions
catch (ServiceException ex)
{
    // Option 1: Only handle SharePoint REST exceptions
    if (ex is SharePointRestServiceException)
    {
        // Let's get the detailed error information
        SharePointRestError error = ex.Error as SharePointRestError;

        // Write detailed error information to a log
        Console.WriteLine(error.ToString());
    }

    // Option 2: handle all service errors
    Console.WriteLine(ex.Error.ToString());
}
```

Having the detailed exception information sometimes is also needed to 'eat' expected exceptions as shown in this sample working with files:

```csharp
IFile image = null;
bool fileExists = false;
try
{
    image = await clientContext.Web.GetFileByServerRelativeUrlAsync("/sites/hrweb/siteassets/image1.png");
    fileExists = true;
}
catch(SharePointRestServiceException ex)
{
    var error = ex.Error as SharePointRestError;

    // Indicates the file did not exist
    if (error.HttpResponseCode == 404 && error.ServerErrorCode == -2130575338)
    {
        Console.WriteLine("File does not exist...we're eating the exception and continue");
    }
    else 
    {
        throw
    }
}

if (fileExists)
{
    // Do something with the image
}
```
