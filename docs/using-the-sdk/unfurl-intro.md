# Unfurling links

SharePoint supports many different types of links, you have direct links to lists, libraries and files, but there's also sharing links that user's have created for resources in SharePoint. Whenever your application needs to understand more about a given link we call that unfurling. A common scenario is where you allow your users to paste a link and your application gets the needed information to present the content behind the link (e.g. when you past a link in Teams you'll you'll see the file name, thumbnail and more). In PnP Core SDK there's support for link unfurling.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with lists
}
```

## Unfurling a link

To unfurl a received link you simply call one of the `UnfurlLink` methods on `IWeb` and you'll get back a populated [IUnfurledResource](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IUnfurledResource.html) instance:

```csharp
var unfurledResource = await context.Web.UnfurlLinkAsync($"https://contoso.sharepoint.com/:li:/s/mysitename/Ey-0-TrWIcxChNFTqyeIm7EB_ktJbcLFxa-EIkZedafGYQ?e=PhOfF5");

if (unfurledResource.LinkType == UnfurlLinkType.File)
{
    // E.g. Present the resource using the returned thumbnails
}
else
{
  // Present the resource in a different manner
}
```

## Unfurling a link with options

When you're unfurling a link you can also specify options by passing in an `UnfurlOptions` instance. Key use case for the `UnfurlOptions` is to allow you to detail the thumbnails you need:

```csharp
UnfurlOptions unfurlOptions = new()
{
    ThumbnailOptions = new()
    {
        StandardSizes = new List<ThumbnailSize>
        {
            ThumbnailSize.Medium,
            ThumbnailSize.Large
        },
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
    }
};

var unfurledResource = await context.Web.UnfurlLinkAsync($"https://contoso.sharepoint.com/:li:/s/mysitename/Ey-0-TrWIcxChNFTqyeIm7EB_ktJbcLFxa-EIkZedafGYQ?e=PhOfF5", unfurlOptions);

if (unfurledResource.LinkType == UnfurlLinkType.File)
{
    // E.g. Present the resource using the returned thumbnails
}
else
{
  // Present the resource in a different manner
}
```

## What resources can be unfurled?

Not every possible SharePoint URL will result in a populated unfurling data, below table will help you get a better understanding.

Resource | UnfurlLinkType | List information | List Item information | File information | Thumbnails
---------|----------------|------------------|-----------------------|------------------|-----------
Site | Unknown | | | |
List | List | x | | |
Document Library | Library | x | | |
Pages Library | SitePagesLibrary | x | | |
List item | ListItem | x | x | |
File | File | x | x | x | x
Site Page | SitePage | x | x | |
