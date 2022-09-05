# Sharing list items

List items are a key part of SharePoint and when you want to share a list item with other users you can use the PnP Core SDK Sharing support.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with lists
}
```

## Scoping the "share"

When you share content you often just want to share what's really needed, oversharing content is never a good idea. When using the share features in PnP Core SDK you can either share:

- A single list item (`IListItem`)
- A single file (`IFile`) (see [here](./sharing-intro.md) for more details)
- A single folder and all the contents inside that folder (`IFolder`). Note that to share the contents of a complete library you can use the `IList.RootFolder` property as sharing target (see [here](./sharing-intro.md) for more details)

After selecting what to share the next step is providing information about with whom you're sharing and how the sharing will be configured. To what extend you can configure all of this depends on the external sharing settings configured for your tenant. Navigate to your **SharePoint admin center**, click **Policies** in the left navigation and select **Sharing** to see the tenant sharing configuration.

Next to the sharing configuration and tenant level there are also sharing settings at site collection level as certain site collections can be configured with more restrictive sharing policies versus the tenant. If you're using the PnP Core SDK sharing API and you're running into issues then ensure the site collection sharing settings are set correctly. Navigate to your **SharePoint admin center**, click **Sites** in the left navigation and select **Active sites**. In the list of site collections find the one you want to share and verify, select it and the click on the **...** and pick **Sharing** to open the site collection sharing settings.

> [!Note]
> You can also programmatically verify and alter site collection sharing settings via the [PnP Core SDK Admin component](https://pnp.github.io/pnpcore/using-the-sdk/admin-sharepoint-sites.html#getting-and-setting-site-collection-properties).

## Sharing list items with your organization

To share list items within your tenant (your organization) you need to use one of the `CreateOrganizationalSharingLink` methods in combination with a configured `OrganizationalLinkOptions` instance. The only property to set here is the `Type` of share: do you want to enable people in your organization to view the shared content or also edit the list item?

```csharp
var myList = context.Web.Lists.GetByTitle("My List", 
                                          p => p.Title, p => p.Items, 
                                          p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                        p => p.FieldTypeKind, 
                                                                        p => p.TypeAsString, 
                                                                        p => p.Title));
// Get the item with title "Item1"
var listItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

var shareLinkRequestOptions = new OrganizationalLinkOptions()
{
    Type = ShareType.Edit
};

// Grant everyone in the organization edit permissions on the list item                
var share = await listItem.CreateOrganizationalSharingLinkAsync(shareLinkRequestOptions);
```

> [!Note]
> The supported share types for a list item are `View`, `Edit` and `Embed`

## Share list items with certain users

If you want to selectively share a list item with one or more users you can use one of the `CreateUserSharingLink` methods in combination with a configured `UserLinkOptions` instance. Key properties to set are the `Recipients` (people you're sharing to) and `Type` of share. Below example shows this:

```csharp
var myList = context.Web.Lists.GetByTitle("My List", 
                                          p => p.Title, p => p.Items, 
                                          p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                        p => p.FieldTypeKind, 
                                                                        p => p.TypeAsString, 
                                                                        p => p.Title));
// Get the item with title "Item1"
var listItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

// List of users to share the file/folder with
var recipients = new List<IDriveRecipient>()
{
    new DriveRecipient
    {
        Email = "linda@contoso.onmicrosoft.com"
    },
    new DriveRecipient
    {
        Email = "joe@contoso.onmicrosoft.com"
    }
};

var shareLinkRequestOptions = new UserLinkOptions()
{
    // Selected users can see the list item online
    Type = ShareType.View,
    Recipients = recipients
};

var share = await listItem.CreateUserSharingLinkAsync(shareLinkRequestOptions);
```

> [!Note]
> The supported share types for a list item are `View`, `Edit` and `Embed`

## Share list items with everyone (anonymous sharing)

If your tenant and site settings allow it you can also share list items anonymously so that everyone having the link can access the content. Doing so requires using one of the `CreateAnonymousSharingLink` methods together with a configured `AnonymousLinkOptions` instance. Key properties to set are the `Type` of share, a `Password` for consuming the sharing link and `ExpirationDateTime` defining how long the anonymous share will stay valid. Below example shows this:

```csharp
var myList = context.Web.Lists.GetByTitle("My List", 
                                          p => p.Title, p => p.Items, 
                                          p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                        p => p.FieldTypeKind, 
                                                                        p => p.TypeAsString, 
                                                                        p => p.Title));
// Get the item with title "Item1"
var listItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

var shareLinkRequestOptions = new AnonymousLinkOptions()
{
    Type = ShareType.View,
    Password = "PnP Rocks!",
    ExpirationDateTime = DateTime.Now.AddDays(5)
};                

var share = await listItem.CreateAnonymousSharingLinkAsync(shareLinkRequestOptions);
```

> [!Note]
> The supported share types for a list item are `View`, `Edit` and `Embed`
