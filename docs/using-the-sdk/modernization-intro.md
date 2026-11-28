# Modernizing classic pages

The `PnP.Core.Modernization` package converts classic SharePoint Online pages into modern pages. It supports wiki pages, web part pages, blog posts, publishing pages, and Enterprise Wiki pages. Mappings describe how to turn their content and web parts into modern sections and controls.

Use this package to transform existing classic content. To create or edit modern pages directly, use the [pages API](pages-intro.md).

Modernization works with authenticated `PnPContext` instances created through `PnPContextFactory`, as explained in the [overview article](readme.md). The examples below use these namespaces:

```csharp
using System;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;
using ModernizationWeb = PnP.Core.Modernization.WebExtensions;
```

Choose the transformator and configuration object for your source:

| Source | Transformator | Configuration |
| --- | --- | --- |
| Wiki pages, web part pages, blog posts | `PageTransformator` | `PageTransformationInformation` |
| Publishing and Enterprise Wiki pages | `PublishingPageTransformator` | `PublishingPageTransformationInformation` |

Both configuration objects take the source page's `IListItem`. Calling `Transform` creates the modern page and returns its URL.

## Transforming wiki and web part pages

This example transforms pages whose names start with `Classic` in the site's Site Pages library. Each result gets a new name and is saved as a draft in the same site.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    var transformator = new PageTransformator(
        context, BasePageTransformator.LoadDefaultWebPartMapping());
    transformator.RegisterObserver(new ConsoleObserver());

    foreach (var sourcePage in ModernizationWeb.GetPages(context.Web, "Classic"))
    {
        var information = new PageTransformationInformation(sourcePage)
        {
            TargetPageName = $"Modern-{sourcePage.Id}.aspx",
            Overwrite = false,
            PublishCreatedPage = false
        };

        string modernPageUrl = transformator.Transform(information);
        Console.WriteLine(modernPageUrl);
    }
}
```

`ModernizationWeb.GetPages` loads classic source items, rather than the modern `IPage` objects returned by `context.Web.GetPages`. For web part pages in another library, use `ModernizationWeb.GetPagesFromList(context.Web, "SiteAssets")`. For blog posts, use `ModernizationWeb.GetBlogsFromList(context.Web, "Posts")` with the same `PageTransformator`.

To write wiki or web part pages to another site, pass a destination context to the constructor:

```csharp
var transformator = new PageTransformator(
    sourceContext, targetContext, BasePageTransformator.LoadDefaultWebPartMapping());
```

## Transforming publishing pages

`PublishingPageTransformator` uses page layout mappings to turn publishing fields into modern content. This example reads articles from the source site's Pages library and creates modern pages in the destination site's Site Pages library.

```csharp
using (var sourceContext = await pnpContextFactory.CreateAsync("PublishingSite"))
using (var targetContext = await pnpContextFactory.CreateAsync("ModernSite"))
{
    var transformator = new PublishingPageTransformator(
        sourceContext, targetContext,
        BasePageTransformator.LoadDefaultWebPartMapping(), null);
    transformator.RegisterObserver(new ConsoleObserver());

    foreach (var sourcePage in ModernizationWeb.GetPagesFromList(
        sourceContext.Web, "Pages", "Article"))
    {
        var information = new PublishingPageTransformationInformation(sourcePage)
        {
            TargetPageName = $"Modern-{sourcePage.Id}.aspx",
            Overwrite = false,
            PublishCreatedPage = false
        };

        string modernPageUrl = transformator.Transform(information);
        Console.WriteLine(modernPageUrl);
    }
}
```

The final constructor argument is an optional custom page layout mapping model. Passing `null` uses the built-in layout mappings. To transform publishing pages within the same web, use `PublishingPageTransformator.CreateInPlace` as shown below.

### Enterprise Wiki pages

Transform Enterprise Wiki pages within their existing web. Set `EnableEnterpriseWikiCapabilities` to preserve category and rating metadata and prepare the corresponding Site Pages library columns using the existing site definitions.

```csharp
using (var context = await pnpContextFactory.CreateAsync("EnterpriseWiki"))
{
    var transformator = PublishingPageTransformator.CreateInPlace(
        context, BasePageTransformator.LoadDefaultWebPartMapping(), null);
    transformator.RegisterObserver(new ConsoleObserver());

    foreach (var sourcePage in ModernizationWeb.GetPagesFromList(
        context.Web, "Pages", "Handbook"))
    {
        var information = new PublishingPageTransformationInformation(sourcePage)
        {
            TargetPageName = $"Modern-{sourcePage.Id}.aspx",
            EnableEnterpriseWikiCapabilities = true,
            IncludeEnterpriseWikiSidebar = true,
            Overwrite = false,
            PublishCreatedPage = false
        };

        string modernPageUrl = transformator.Transform(information);
        Console.WriteLine(modernPageUrl);
    }
}
```

The web needs the Site Pages feature enabled and the native category and rating site columns, including the category term set. `IncludeEnterpriseWikiSidebar` adds the category and rating control to the modern page; set it to `false` when the site does not support the modern Enterprise Wiki sidebar renderer. The metadata is still copied when the sidebar is disabled.

## Configuring the result

Both transformation information classes expose these options:

| Property | Purpose |
| --- | --- |
| `TargetPageName` | Name of the modern page, including `.aspx`. |
| `Overwrite` | Replace an existing target page. Defaults to `false`. |
| `PublishCreatedPage` | Publish the result. Defaults to `true`; set to `false` to save a draft. |
| `KeepPageSpecificPermissions` | Copy the source page's unique permissions. Defaults to `true`. |
| `KeepPageCreationModificationInformation` | Preserve the source page's creation and modification information. Defaults to `false`. |
| `PostAsNews` | Create the result as a news post. Defaults to `false`. |

For wiki and web part pages, set `PageTransformationInformation.CopyPageMetadata` to `true` to copy additional page metadata. Publishing metadata is configured through the page layout mappings.

## Customizing mappings

`BasePageTransformator.LoadDefaultWebPartMapping()` loads the embedded web part mapping model. Web part mappings select the modern controls and their properties; publishing page layout mappings also define how source fields supply content and metadata.

To use custom XML mappings, pass their paths to the transformator:

```csharp
var transformator = new PublishingPageTransformator(
    sourceContext, targetContext,
    @"C:\Mappings\webpartmapping.xml",
    @"C:\Mappings\pagelayoutmapping.xml");
```

Custom publishing layout mappings are merged with the built-in mappings. Transformation information also accepts `UrlMappingFile`, `UserMappingFile`, and `TermMappingFile` for URL, user, and taxonomy mappings. Mapping between term sets requires Microsoft Graph term-store read permissions.

## Reading transformation reports

Register log observers before calling `Transform` to capture progress, warnings, and errors. The examples use `ConsoleObserver` for console output. `MarkdownObserver` writes a local Markdown report, while `MarkdownToSharePointObserver` saves a report page in SharePoint.

With a transformator and transformation information configured, save a SharePoint report as follows:

```csharp
var observer = new MarkdownToSharePointObserver(
    targetContext.Web, folderName: "Transformation-Reports");
transformator.RegisterObserver(observer);

transformator.Transform(information);

string reportPage = await observer.FlushAsync();
```

`FlushAsync` saves the accumulated report and returns its name relative to Site Pages. Keep the report context alive until the report has been saved. Review warnings and errors as well as the generated pages: an individual component or metadata field can fail without stopping the whole transformation.
