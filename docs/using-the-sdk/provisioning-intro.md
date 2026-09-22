# Provisioning sites with templates

The **PnP Provisioning Engine** describes a SharePoint site as XML — its columns, content types, lists, pages, files, security, navigation and branding — so that you can **extract** that description from one site and **apply** it to another. It is the engine behind PnP provisioning templates and `.pnp` packages.

The engine lives in its own NuGet package, **`PnP.Core.Provisioning`**, which builds on `PnP.Core` and `PnP.Core.Admin`. Install all three at matching versions:

```shell
dotnet add package PnP.Core.Provisioning
```

> [!IMPORTANT]
> **This package is experimental and should be treated as a beta.** It is a migration of the PnP Provisioning Engine from [PnP Framework](https://github.com/pnp/pnpframework), where it is built on CSOM, onto the async REST and Microsoft Graph based PnP Core SDK. The API surface may still change and not every handler has equivalent coverage to PnP Framework yet. Test a template against a site you can afford to lose before relying on it, and please report what you find.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and shown below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapters on how to use the PnPContext for provisioning
}
```

The provisioning features shown in this article are used via the `GetProvisioningManager()` method on your `PnPContext` instance.

## What a template contains

A template is a declarative description of a site, made up of the things the engine knows how to read and write:

| | |
|---|---|
| **Information architecture** | site columns, content types, lists and libraries, views |
| **Content** | list items, files in libraries, client side pages and their web parts |
| **Configuration** | security and groups, navigation, property bags, custom actions, features |
| **Branding** | site header and footer, chrome, themes, site logo |
| **Taxonomy** | term groups, term sets and terms |

Each of these is handled by an *object handler*, and handlers run in a fixed order because they depend on each other — a lookup column cannot resolve a list that does not exist yet, and a taxonomy column cannot bind to a term set that has not been created.

## Extracting a template from a site

An extract captures **structure only** by default. Content is the expensive part of an extract, so you opt into it rather than get it by accident:

```csharp
var manager = context.GetProvisioningManager();

var template = await manager.GetTemplateAsync(new ExtractConfiguration
{
    ProgressDelegate = (step, current, total) => Console.WriteLine($"{current}/{total} {step}"),
    MessagesDelegate = (message, type) => Console.WriteLine($"[{type}] {message}")
});
```

An extract also leaves out what every site of the same kind already has. The site is compared against the out of the box template of its own web template, for example `SITEPAGEPUBLISHING#0` for a communication site, and SharePoint's own site columns and content types, and the custom actions, property bag entries and settings a fresh site starts with, are not written to the template. A template carrying them would re-apply SharePoint's own columns, which is redundant at best and refused outright for some of them. To extract everything regardless, turn the comparison off:

```csharp
var template = await manager.GetTemplateAsync(new ExtractConfiguration
{
    CompareWithBaseTemplate = false
});
```

Content is opted into per list. This asks for the items of one list, the files of a document library, and the site's pages:

```csharp
var configuration = new ExtractConfiguration();

configuration.Lists.Lists.Add(new ExtractListsListsConfiguration
{
    Title = "Announcements",
    IncludeItems = true
});

configuration.Lists.Lists.Add(new ExtractListsListsConfiguration
{
    Title = "Documents",
    IncludeFiles = true
});

configuration.Pages.IncludeAllClientSidePages = true;

// Exported files are written through this connector, so it must be set when asking for files
configuration.FileConnector = new FileSystemConnector(@"C:\templates", string.Empty);

var template = await manager.GetTemplateAsync(configuration);
```

A list's entry can ask for more of its structure and content too. They all cost extra requests, so they are off by default:

- `IncludeFolders` adds the list's folders to the template, each with its property bag. `MaxFolderDepth` limits how many levels are read: 1 takes only the folders at the root of the list, and 0, the default, takes every level.
- `IncludeSecurity` adds the unique permissions of the list's folders and items, so that one that breaks inheritance still does once the template is applied.
- `TokenizeUrls` replaces the urls and ids of the site in the extracted item values with tokens, so that links in the rows point at the site the template is applied to rather than back at the one it came from.

```csharp
configuration.Lists.Lists.Add(new ExtractListsListsConfiguration
{
    Title = "Projects",
    IncludeItems = true,
    IncludeFolders = true,
    IncludeSecurity = true,
    TokenizeUrls = true
});
```

## Saving a template

A template can be saved as XML, or as a `.pnp` package that holds the XML **and the files it references** in a single file:

```csharp
// As XML
using (var stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
using (var file = File.Create(@"C:\templates\marketing.xml"))
{
    stream.CopyTo(file);
}
```

```csharp
// As a .pnp package
var package = new OpenXMLConnector(
    "marketing.pnp",
    new FileSystemConnector(@"C:\templates", string.Empty),
    author: "Contoso",
    signingCertificate: null,
    templateFileName: "template.xml");

// Point the extract at the package so exported files land inside it
configuration.FileConnector = package;

var template = await manager.GetTemplateAsync(configuration);

new XMLOpenXMLTemplateProvider(package).SaveAs(template, "template.xml");
```

Prefer a `.pnp` package whenever the template ships content. An `.xml` template records its files but leaves the bytes on disk beside it, and moving the XML without them produces an apply that silently creates empty libraries.

## Applying a template to a site

```csharp
ProvisioningTemplate template;

using (var stream = File.OpenRead(@"C:\templates\marketing.xml"))
{
    template = XMLPnPSchemaFormatter.LatestFormatter.ToProvisioningTemplate(stream);
}

// The connector is how the template reaches the files next to it
template.Connector = new FileSystemConnector(@"C:\templates", string.Empty);

await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
{
    ProgressDelegate = (step, current, total) => Console.WriteLine($"{current}/{total} {step}"),
    MessagesDelegate = (message, type) => Console.WriteLine($"[{type}] {message}")
});
```

Applying a `.pnp` package reads the template out of it and points the connector at the package, so the files come along:

```csharp
var package = new OpenXMLConnector(
    "marketing.pnp",
    new FileSystemConnector(@"C:\templates", string.Empty));

var template = new XMLOpenXMLTemplateProvider(package).GetTemplate("template.xml");
template.Connector = package;

await context.GetProvisioningManager().ApplyTemplateAsync(template);
```

## Reading what the engine reports

**Wire up `MessagesDelegate`.** The engine reports a problem and carries on rather than stopping at the first one, so a run can finish having quietly skipped part of the template: a sealed content type it may not update, an artefact a NoScript site refuses, a webhook from another tenant that cannot be re-registered.

"It finished" and "it all worked" are different claims, and the reported messages are how you tell them apart. A console app built on this engine should make that distinction visible — for example by exiting with a different code when anything was reported.

## Tenant templates

A template can also describe **the site collections to create**, in a sequence. Applying such a hierarchy creates the sites and then applies the templates attached to them:

```csharp
var hierarchy = new XMLOpenXMLTemplateProvider(package).GetHierarchy();

await context.GetProvisioningManager().ApplyTenantTemplateAsync(
    hierarchy,
    hierarchy.Sequences.First().ID,
    new ApplyConfiguration
    {
        Parameters =
        {
            ["SiteTitle"] = "Marketing",
            ["SiteUrl"] = "/sites/marketing"
        }
    });
```

Two things to know:

- Site urls may be **relative** (`/sites/marketing`) and are resolved against the tenant of the context you apply with.
- A site collection needs an explicit **`Owner`** when you authenticate as an application. With a signed-in user the owner defaults to that user; app-only has no current user to fall back on, and site creation fails without it.

## Sample

**[Demo.Console.Provisioning](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.Console.Provisioning)** is a console application built on this package. It extracts templates with or without content, saves them as `.xml` or `.pnp`, shows what a template contains, and applies one to an existing site or to a new communication site it creates for you — with both interactive and app-only authentication. It is the quickest way to see the engine work end to end.
