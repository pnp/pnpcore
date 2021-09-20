# Working with pages: an introduction

Each SharePoint site uses pages, being [modern pages](https://support.microsoft.com/en-us/office/pages-in-sharepoint-3833b917-a39d-43f9-85ff-f17949fc1034?ui=en-US&rs=en-US&ad=US) or classic pages like wiki, webpart or publishing pages. PnP Core SDK does not support editing those classic pages, but there's extensive support for modern pages. In this article you'll learn how to load pages, how to create, update, configure and delete them.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

## Loading an existing page

Quite often you need to update an previously created page, so you'll first need to load the existing page. Loading existing pages can be done using the [GetPagesAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_GetPagesAsync_System_String_).

### Load all the pages on a site

```csharp
var pages = await context.Web.GetPagesAsync();
```

### Load pages filtered on page name

Below call will get all the pages on the site for which the name starts with `PageA`.

```csharp
var pages = await context.Web.GetPagesAsync("PageA");
```

If you want to only find one page then provide the actual page name with the .aspx extension:

```csharp
var pages = await context.Web.GetPagesAsync("PageABC.aspx");
var pageABC = pages.First();
```

### Load pages in a given folder

Modern pages can be organized in folders, if you want to load a page from a given folder (the previous samples loaded pages across all folders) then you need to prepend the folder path to the page when searching for it:

```csharp
string pageName = "folder1/PageInFolder.aspx";
var pages = await context.Web.GetPagesAsync(pageName);
var pageInFolder = pages.First();
```

If the page lives in a nested folder you can provide the nest path:

```csharp
string pageName = "folder1/folder2/folder3/PageInFolder3.aspx";
var pages = await context.Web.GetPagesAsync(pageName);
var pageInFolder3 = pages.First();
```

Note that when a page was loaded from a folder the `Folder` property of the retuned page will contain the folder:

```csharp
string pageName = "folder1/folder2/folder3/PageInFolder3.aspx";
var pages = await context.Web.GetPagesAsync(pageName);
var pageInFolder3 = pages.First();

// Outputs folder1/folder2/folder3
Console.WriteLine(pagesInFolder3.Folder);
```

## Adding a new page

Adding a new page always consists out of two steps: you first create a new page via the [NewPageAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_NewPageAsync_PnP_Core_Model_SharePoint_PageLayoutType_) and persist the created page via the [SaveAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_SaveAsync_System_String_).

```csharp
// Create the page
var newPage = await context.Web.NewPageAsync();

// configure the page

// Save the page
await newPage.SaveAsync("MyPage.aspx");
```

When you create a new page the default will be an **Article** page, but you can also create other types of pages. The pages API supports these common types of pages:

- **Article page**: this is typical page you'd create using the SharePoint user interface
- **Repost page**: this page is a re-posting of existing content where existing content can be a link to a resource on the internet or an other page in your SharePoint environment
- **Home page**: this is a page created with as purpose to become the home page of the site, this page has no page header by design and has no page commenting
- **Spaces page**: [SharePoint Spaces](https://www.exploresharepointspaces.com/) pages can host mixed reality content

To pick another page type during creation specify the type as input of the [NewPageAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_NewPageAsync_PnP_Core_Model_SharePoint_PageLayoutType_):

```csharp
// Create the page
var newPage = await context.Web.NewPageAsync(PageLayoutType.RepostPage);
```

### Adding a page in a given folder

When you want to create a page in a folder you need to prepend the folder to the page name on save:

```csharp
// Create the page
var newPage = await context.Web.NewPageAsync();

// configure the page

// Save the page
await newPage.SaveAsync("folder1/folder2/MyPageInFolder2.aspx");
```

### Saving a page as a template

A page can also be saved as a template, when users of the site want to create a new page they can base themselves of a template page. To save a page as a template you instead of the `SaveAsync()` use the [SaveAsTemplateAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_SaveAsTemplateAsync_System_String_):

```csharp
// Create the page
var newPage = await context.Web.NewPageAsync();

// configure the page

// Save the page
await newPage.SaveAsTemplateAsync("MyTemplatePage.aspx");
```

> [!Note]
> You do not need to specify the templates folder, the page API will ensure the templates folder does exist and automatically use it.

## Configuring a page

Whenever you've created a new page or loaded an existing page, you often need to also configure the page by adding content to it. In this chapter you'll learn how to do so.

### Setting up your page layout

A modern page exists out of one or more sections of a given type:

- One column section: section contain one column
- One column fullwidth section: section containing one column that's covering the full width of the page (you can only have one of these per page)
- Two column section: section with two equally sized columns
- Two column left section: section with the left column 2/3 in size and the right one 1/3
- Two column right section: section with the left column 1/3 in size and the right one 2/3
- Three column section: section with three equally sized columms

The above sections, except one column fullwidth, are also available in a variant that includes a page wide vertical column on the right side. You can only use one "vertical column" section on a page and it's mutually exclusive with the one column fullwidth section. Adding sections is done using the [AddSection method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_AddSection_PnP_Core_Model_SharePoint_CanvasSectionTemplate_System_Single_):

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// adding sections to the page
page.AddSection(CanvasSectionTemplate.OneColumnFullWidth, 1);
page.AddSection(CanvasSectionTemplate.OneColumn, 2);
page.AddSection(CanvasSectionTemplate.TwoColumn, 3);
page.AddSection(CanvasSectionTemplate.TwoColumnLeft , 4);
page.AddSection(CanvasSectionTemplate.TwoColumnRight, 5);
page.AddSection(CanvasSectionTemplate.ThreeColumn, 6);

// Save the page
await page.SaveAsync("MyPage.aspx");
```

You can also control the emphasis (background color) of the section and vertical column via specifying the needed emphasis in the [AddSection method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_AddSection_PnP_Core_Model_SharePoint_CanvasSectionTemplate_System_Single_PnP_Core_Model_SharePoint_VariantThemeType_PnP_Core_Model_SharePoint_VariantThemeType_):

```csharp
page.AddSection(CanvasSectionTemplate.TwoColumnVerticalSection, 1, VariantThemeType.Neutral, VariantThemeType.Strong);
```

### Configuring page sections

Adding a section to a page gets you a default, non-collapsible, section. If you want your section to be collapsible then simply set the `Collapsible` property to true and optionally configure the additional settings for collapsible sections:

```csharp
page.AddSection(CanvasSectionTemplate.TwoColumn, 1, VariantThemeType.Soft);
page.Sections[0].Collapsible = true;
page.Sections[0].DisplayName = "My collapsible section";
page.Sections[0].IsExpanded = false;
page.Sections[0].ShowDividerLine = false;
page.Sections[0].IconAlignment = IconAlignment.Right;
```

### Adding controls to the page

A control is either a piece of text or a web part and can be added in one of the section columns. Below sample shows how to add the text part to an existing section column. It boils down to these two steps:

- Creating a text part via the [NewTextPart method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_NewTextPart_System_String_)
- Adding the text part into a previously created section column via the [AddControl method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_AddControl_PnP_Core_Model_SharePoint_ICanvasControl_PnP_Core_Model_SharePoint_ICanvasColumn_)

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// adding sections to the page
page.AddSection(CanvasSectionTemplate.OneColumn, 1);

// Adding text control to the first section, first column
page.AddControl(page.NewTextPart("PnP Rocks!"), page.Sections[0].Columns[0]);

// Save the page
await page.SaveAsync("MyPage.aspx");
```

### Adding web parts to the page

Adding web parts is quite similar to adding text parts, but there's more work needed to prep a web part before it can be added. Before you can add a web part you need to get a 'blueprint' of the web part to add. This needs to be done via the [AvailablePageComponentsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_AvailablePageComponentsAsync_System_String_). Once you've the list of possible web parts to add you need to pick the one you need. If you're adding an out of the box web part then using the [DefaultWebPartToWebPartId method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_DefaultWebPartToWebPartId_PnP_Core_Model_SharePoint_DefaultWebPart_) to map a readable name into a web part id works best. The final step is using the 'blueprint' with the [NewWebPart method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_NewWebPart_PnP_Core_Model_SharePoint_IPageComponent_) to create a web part instance that can be added. Below sample shows all these steps:

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// adding sections to the page
page.AddSection(CanvasSectionTemplate.OneColumn, 1);

// get the web part 'blueprint'
var availableComponents = await page.AvailablePageComponentsAsync();
var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

// add the web part to the first column of the first section
page.AddControl(page.NewWebPart(imageWebPartComponent), page.Sections[0].Columns[0]);

// Save the page
await page.SaveAsync("MyPage.aspx");
```

## Deleting a page

You can delete pages via the [DeleteAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_DeleteAsync):

```csharp
await page.DeleteAsync();
```
