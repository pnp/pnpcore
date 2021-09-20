# Configuring the page header

When you create pages you can also configure the page header. There are various options to configure a page header: you can add a nice image, change the header layout mode and more. What you can do via the SharePoint UI can also be done using the pages API, as explained in this chapter.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

## Creating a page without header

Sometimes the header takes up too much space and you prefer to create a page with a minimal header. You can do this by calling the [RemovePageHeader method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_RemovePageHeader) on a page:

```csharp
// Create the page
var newPage = await context.Web.NewPageAsync();

// configure the page
newPage.RemovePageHeader();

// Save the page
await newPage.SaveAsync("PageWithMinimalHeader.aspx");
```

## Configuring the default page header

When you create a new page it will be configured to use the default page header which contains the default background image and title. If you want to switch a page to the default header you can use the [SetDefaultPageHeader method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_SetDefaultPageHeader). The default page header can also be customized by setting the page header properties. Below is an example:

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Configure the page
page.SetDefaultPageHeader();
page.PageHeader.LayoutType = PageHeaderLayoutType.CutInShape;
page.PageHeader.ShowTopicHeader = true;
page.PageHeader.TopicHeader = "I'm a topic header";
page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
page.PageHeader.ShowPublishDate = true;

// Save the page
await page.SaveAsync("PageWithCustomizedDefaultHeader.aspx");
```

Above sample will switch the header to use the Cut in Shape layout option with a topic header. We're also changing the page title alignment to be centered versus the default left alignment and we're showing the date when the page was published in the header.

## Configuring a custom header

The previous section already showed some cool page header features, but we can do better 😊 You can also use a full custom header, which next to the options presented in the previous example, also allows you to configure the background image of the header. To configure a header with an image you can use the [SetCustomPageHeader method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_SetCustomPageHeader_System_String_System_Nullable_System_Double__System_Nullable_System_Double__) which takes the server relative url to the image to use as input.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Upload the header image to site assets library
IFolder parentFolder = await context.Web.Folders.GetFirstOrDefaultAsync(f => f.Name == "SiteAssets");
IFile headerImage = await parentFolder.Files.AddAsync("pageheader.jpg", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}pageheader.jpg"));

// Configure the page
page.SetCustomPageHeader(headerImage.ServerRelativeUrl, 5.3, 6.2);
page.PageHeader.LayoutType = PageHeaderLayoutType.ColorBlock;
page.PageHeader.ShowTopicHeader = true;
page.PageHeader.TopicHeader = "I'm a topic header";
page.PageHeader.TextAlignment = PageHeaderTitleAlignment.Center;
page.PageHeader.ShowPublishDate = true;

// Save the page
await page.SaveAsync("PageWithCustomHeader.aspx");
```

> [!Note]
> Note that you can control the header image offset by providing the needed X and Y offset values when calling the [SetCustomPageHeader method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_SetCustomPageHeader_System_String_System_Nullable_System_Double__System_Nullable_System_Double__).
