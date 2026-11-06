# PnP.Core.Provisioning

The **PnP Provisioning Engine** on top of the [PnP Core SDK](https://github.com/pnp/pnpcore).

Describe a SharePoint site as XML — its columns, content types, lists, pages, files, security,
navigation and branding — then **extract** that description from an existing site and **apply** it to
another. It is the engine behind PnP provisioning templates and `.pnp` packages.

> ## ⚠️ Experimental
>
> **This package is experimental and should be treated as a beta.** It is a migration of the
> PnP Provisioning Engine from [PnP Framework](https://github.com/pnp/pnpframework) — where it is
> built on CSOM — onto the modern, async, REST and Microsoft Graph based PnP Core SDK.
>
> The API surface may still change, not every handler has equivalent coverage to PnP Framework yet,
> and you should test a template against a site you can afford to lose before using it in
> production. Please report what you find — real templates applied to real tenants are the most
> useful feedback we can get.

## Install

```shell
dotnet add package PnP.Core.Provisioning
```

It builds on `PnP.Core` and `PnP.Core.Admin`, and is **released in lockstep with them** — always use
matching versions of all three.

## Getting a context

The engine works from a `PnPContext`, so set up authentication the usual PnP Core way (see the
[PnP Core SDK docs](https://pnp.github.io/pnpcore/)). Everything below assumes you have one:

```csharp
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Provisioning.Providers.Xml;

using (PnPContext context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    IProvisioningManager manager = context.GetProvisioningManager();
}
```

## Extracting a template

By default an extract captures **structure only** — columns, content types, lists, security,
navigation, branding — and none of the content inside them:

```csharp
IProvisioningManager manager = context.GetProvisioningManager();

ProvisioningTemplate template = await manager.GetTemplateAsync(new ExtractConfiguration
{
    ProgressDelegate = (step, current, total) => Console.WriteLine($"{current}/{total} {step}"),
    MessagesDelegate = (message, type) => Console.WriteLine($"[{type}] {message}"),
});
```

Content is opted into per list. Ask for list items, the files in a document library, and the site's
pages:

```csharp
var configuration = new ExtractConfiguration();

configuration.Lists.Lists.Add(new ExtractListsListsConfiguration
{
    Title = "Announcements",
    IncludeItems = true,
});

configuration.Lists.Lists.Add(new ExtractListsListsConfiguration
{
    Title = "Documents",
    IncludeFiles = true,
});

configuration.Pages.IncludeAllClientSidePages = true;

// Exported files are written through this connector, so it has to be set when asking for files
configuration.FileConnector = new FileSystemConnector(@"C:\templates", string.Empty);

ProvisioningTemplate template = await manager.GetTemplateAsync(configuration);
```

## Saving a template as XML

```csharp
using (Stream stream = XMLPnPSchemaFormatter.LatestFormatter.ToFormattedTemplate(template))
using (FileStream file = File.Create(@"C:\templates\marketing.xml"))
{
    stream.CopyTo(file);
}
```

## Saving a template as a `.pnp` package

A `.pnp` package is a **single file holding the template and everything it references** — the XML
plus the files exported from libraries. Prefer it whenever the template ships content, because an
`.xml` template leaves those files beside it on disk and they are easy to lose:

```csharp
var package = new OpenXMLConnector(
    "marketing.pnp",
    new FileSystemConnector(@"C:\templates", string.Empty),
    author: "Contoso",
    signingCertificate: null,
    templateFileName: "template.xml");

// Point the extract at the package so exported files are written into it
configuration.FileConnector = package;

ProvisioningTemplate template = await manager.GetTemplateAsync(configuration);

new XMLOpenXMLTemplateProvider(package).SaveAs(template, "template.xml");
```

## Applying a template

### From XML

```csharp
ProvisioningTemplate template;

using (Stream stream = File.OpenRead(@"C:\templates\marketing.xml"))
{
    template = XMLPnPSchemaFormatter.LatestFormatter.ToProvisioningTemplate(stream);
}

// The connector is how the template reaches the files next to it. Without it a template that
// carries files applies and quietly brings none of them.
template.Connector = new FileSystemConnector(@"C:\templates", string.Empty);

await context.GetProvisioningManager().ApplyTemplateAsync(template, new ApplyConfiguration
{
    ProgressDelegate = (step, current, total) => Console.WriteLine($"{current}/{total} {step}"),
    MessagesDelegate = (message, type) => Console.WriteLine($"[{type}] {message}"),
});
```

### From a `.pnp` package

```csharp
var package = new OpenXMLConnector(
    "marketing.pnp",
    new FileSystemConnector(@"C:\templates", string.Empty));

ProvisioningTemplate template = new XMLOpenXMLTemplateProvider(package).GetTemplate("template.xml");
template.Connector = package;

await context.GetProvisioningManager().ApplyTemplateAsync(template);
```

### Reading what the engine reports

**`MessagesDelegate` is worth wiring up.** The engine reports a problem and carries on rather than
stopping at the first one, so a run can finish having quietly skipped part of the template — a
sealed content type it may not update, an artefact a NoScript site refuses, a webhook that cannot
be re-registered. "It finished" and "it all worked" are different claims, and the messages are how
you tell them apart.

## Tenant templates (hierarchies)

A template can also describe the **site collections to create**, in a sequence. Applying one creates
the sites and then applies the templates attached to them:

```csharp
ProvisioningHierarchy hierarchy = new XMLOpenXMLTemplateProvider(package).GetHierarchy();

await context.GetProvisioningManager().ApplyTenantTemplateAsync(
    hierarchy,
    hierarchy.Sequences.First().ID,
    new ApplyConfiguration
    {
        Parameters =
        {
            ["SiteTitle"] = "Marketing",
            ["SiteUrl"] = "/sites/marketing",
        },
    });
```

Two things to know about hierarchies:

- Site urls in a template may be **relative** (`/sites/marketing`); they are resolved against the
  tenant of the context you apply with.
- A site collection in a hierarchy needs an explicit **`Owner`** when you authenticate as an
  application. With a signed-in user the owner defaults to that user; app-only has no current user
  to fall back on.

## A working sample

**[Demo.Console.Provisioning](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.Console.Provisioning)**
is a console app built on this package. It extracts templates (with or without content), saves them
as `.xml` or `.pnp`, shows what a template contains, applies one to an existing site or to a new
communication site it creates for you, and covers both interactive and app-only authentication.
It is the quickest way to see the engine work end to end.

## Feedback

Issues and pull requests are welcome at
[github.com/pnp/pnpcore](https://github.com/pnp/pnpcore). Given this package's experimental status,
reports of templates that fail to extract or apply are especially valuable — please include the
messages the engine reported.
