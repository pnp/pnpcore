# Sharing files and folders

Files in SharePoint live inside libraries and possibly inside a folder structure inside the library. When you want to share a file, folder or library with other users you can use the PnP Core SDK Sharing support.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with lists
}
```

## Scoping the "share"

When you share content you often just want to share what's really needed, oversharing content is never a good idea. When using the share features in PnP Core SDK you can either share:

- A single list item (`IListItem`) (see [here](./sharing-listitems.md) for more details)
- A single file (`IFile`)
- A single folder and all the contents inside that folder (`IFolder`). Note that to share the contents of a complete library you can use the `IList.RootFolder` property as sharing target

> [!Important]
> In the remainder of the chapter the samples will show sharing of a `IFile`, but sharing of an `IFolder` works identically the same.

After selecting what to share the next step is providing information about with whom you're sharing and how the sharing will be configured. To what extend you can configure all of this depends on the external sharing settings configured for your tenant. Navigate to your **SharePoint admin center**, click **Policies** in the left navigation and select **Sharing** to see the tenant sharing configuration. In order to for example share a file with a new guest user the external sharing for SharePoint/OneDrive must be set to the "New and existing guests" level or higher.

Next to the sharing configuration and tenant level there are also sharing settings at site collection level as certain site collections can be configured with more restrictive sharing policies versus the tenant. If you're using the PnP Core SDK sharing API and you're running into issues then ensure the site collection sharing settings are set correctly. Navigate to your **SharePoint admin center**, click **Sites** in the left navigation and select **Active sites**. In the list of site collections find the one you want to share and verify, select it and the click on the **...** and pick **Sharing** to open the site collection sharing settings.

> [!Note]
> You can also programmatically verify and alter site collection sharing settings via the [PnP Core SDK Admin component](https://pnp.github.io/pnpcore/using-the-sdk/admin-sharepoint-sites.html#getting-and-setting-site-collection-properties).

## Get current shares for a folder or file

To list the shares defined on a folder or file you can use one of the `GetShareLinks` methods. These methods return an `IGraphPermissionCollection` containing one `IGraphPermission` row per configured share.

```csharp
// Assume this file has some shared defined
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");
var sharingLinks = await file.GetShareLinksAsync();
foreach(var share in sharingLinks)
{
    // Do something with the share
}
```

## Sharing content with your organization

To share content within your tenant (your organization) you need to use one of the `CreateOrganizationalSharingLink` methods in combination with a configured `OrganizationalLinkOptions` instance. The only property to set here is the `Type` of share: do you want to enable people in your organization to view the shared content or also edit the content?

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");
var shareLinkRequestOptions = new OrganizationalLinkOptions()
{
    Type = ShareType.Edit
};

// Grant everyone in the organization edit permissions on the file                
var share = await file.CreateOrganizationalSharingLinkAsync(shareLinkRequestOptions);
```

## Share content with certain users

If you want to selectively share content with one or more users you can use one of the `CreateUserSharingLink` methods in combination with a configured `UserLinkOptions` instance. Key properties to set are the `Recipients` (people you're sharing to) and `Type` of share. Below example shows this:

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// List of users to share the file/folder with
var driveRecipients = new List<IDriveRecipient>()
{
    UserLinkOptions.CreateDriveRecipient("linda@contoso.onmicrosoft.com"),
    UserLinkOptions.CreateDriveRecipient("joe@contoso.onmicrosoft.com")    
};

var shareLinkRequestOptions = new UserLinkOptions()
{
    // Users can see and edit the file online, but not download it
    Type = ShareType.BlocksDownload,
    Recipients = driveRecipients
};

var share = await file.CreateUserSharingLinkAsync(shareLinkRequestOptions);
```

## Share content with everyone (anonymous sharing)

If your tenant and site settings allow it you can also share content anonymously so that everyone having the link can access the content. Doing so requires using one of the `CreateAnonymousSharingLink` methods together with a configured `AnonymousLinkOptions` instance. Key properties to set are the `Type` of share, a `Password` for consuming the sharing link and `ExpirationDateTime` defining how long the anonymous share will stay valid. Below example shows this:

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

var shareLinkRequestOptions = new AnonymousLinkOptions()
{
    Type = ShareType.View,
    Password = "PnP Rocks!",
    ExpirationDateTime = DateTime.Now.AddDays(5)
};                

var share = await file.CreateAnonymousSharingLinkAsync(shareLinkRequestOptions);
```

## Share content via an invite

While above share methods do directly share the content with the user(s), there's also an option to share via inviting users to a file or folder. This can be done using one of the `CreateSharingInvite` methods in combination with an `InviteOptions` instance.

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

var driveRecipients = new List<IDriveRecipient>()
{
    InviteOptions.CreateDriveRecipient("linda@contoso.onmicrosoft.com")
};

var shareRequestOptions = new InviteOptions()
{
    Message = "I'd like to share this file with you",
    RequireSignIn = true,
    SendInvitation = true,
    Recipients = driveRecipients,
    Roles = new List<PermissionRole> { PermissionRole.Read },
    ExpirationDateTime = DateTime.Now.AddDays(5)
};

var share = await file.CreateSharingInviteAsync(shareRequestOptions);
```

## Update file shares

When you've created a sharing link granting access to users you can still make changes to it by adding or removing users. Once you've a reference to a sharing link use the `GrantUserPermissions` and `RemoveUserPermissions` methods to update the users that get access via this sharing link.

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Get the first sharing link for this file
var sharingLink = await file.GetShareLinksAsync().First();

// Define the new set users that need to get access
var driveRecipientsToAdd = new List<IDriveRecipient>()
{
    UserLinkOptions.CreateDriveRecipient("linda@contoso.onmicrosoft.com"),
    UserLinkOptions.CreateDriveRecipient("joe@contoso.onmicrosoft.com")    
};

// Add the new users
await sharingLink.GrantUserPermissionsAsync(driveRecipientsToAdd);

//Define the users to remove
var driveRecipientsToRemove = new List<IDriveRecipient>()
{
    UserLinkOptions.CreateDriveRecipient("tim@contoso.onmicrosoft.com")
};

// Remove user from sharing link
await sharingLink.RemoveUserPermissionsAsync(driveRecipientsToRemove);
```

## Deleting file shares

There are two options to delete sharing links for a file or folder: you can either delete all sharing links for a given file/folder or you can enumerate the current sharing links and delete a specific one. To delete all sharing links you can use one of the `DeleteShareLinks` methods, to delete an individual sharing link you first need to load the sharing links via one of the `GetShareLinks` methods followed by using one of the `DeletePermission` links on the returned `IGraphPermission` instance. Below code sample shows both approaches.

```csharp
var file = await context.Web.GetFileByServerRelativeUrlAsync("/sites/demo/docs/fileA.docx");

// Option A: delete all shares of the file/folder
await file.DeleteShareLinksAsync();

// Option B: delete specific shares of the file/folder
var sharingLinks = await file.GetShareLinksAsync();
foreach(var share in sharingLinks)
{
    // Let's imagine we're deleting all shares that are secured via password
    if (share.HasPassword)
    {
        await share.DeletePermissionAsync();
    }
}
```
