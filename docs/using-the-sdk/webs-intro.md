# Working with webs

In SharePoint there are site collections and inside a site collection you have at least one web (the root web), which on it's own can have other webs (child webs). Quite often when talking about a web the term site or sub site is used. When using the PnP Core SDK a site collection is referred to via the [ISite interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) while a web (root web, child web or sub web) is defined via the [IWeb interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html).

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with webs
}
```

## Getting a web

A `PnPContext` always has a reference to the [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) of the underlying site collection and the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) of the connected site, so getting the current Web is as a simple as using the Web property on the PnPContext. When a context is created the Site and Web properties are loaded with some elementary properties like Id, Url and RegionalSettings.

```csharp
var web = context.Web;
var id = web.Id;
```

If you want to load additional web properties you can do that via using one of the `Load`/`Get` methods:

```csharp
// Load the root folder 
await context.Web.LoadAsync(p => p.RootFolder);

// Load the title and content types and lists. For content types ans lists load additional properties
var web = await context.Web.GetAsync(p => p.Title,
                           p => p.ContentTypes.QueryProperties(p => p.Name),
                           p => p.Lists.QueryProperties(p => p.Id, p => p.Title, p => p.DocumentTemplate));
```

### Getting sub webs

A web can have zero or more sub webs and to load these you use the [Webs property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_Webs) and enumerate over the sub webs of the current web:

> [!Important]
> When you want to work with the sub web you need to create a new `PnPContext` for the sub web as shown in below sample

```csharp
// Load the sub webs
await context.Web.LoadAsync(p => p.Webs);

foreach (var subWeb in context.Web.Webs.AsRequested())
{
    using (var contextSubWeb = await pnpContextFactory.CreateAsync(subWeb.Url))
    {
        // Work with the found web
    }
}
```

## Adding a web

Adding a web can be done using one of the Add methods in combination with specifying the information for the new web via the WebOptions class.

```csharp
// add a new web to the current web, uses default web template (STS#3) and default language (1033)
var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = "My web", Url = "myweb" });

using (var contextAddedWeb = await pnpContextFactory.CreateAsync(addedWeb.Url))
{
    // Work with the added web
}
```

## Updating a web

To update a web you can set the relevant web properties and call one of the update methods:

```csharp
// Load the web title
var web = await context.Web.GetAsync(p => p.Title);

web.Title = "PnP Rocks!";
await web.UpdateAsync();
```

### Web property bag

Each web also has a so called property bag, a list key/value pairs providing more information about the web. You can read this property bag, provided via the [IWeb AllProperties property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_AllProperties), and add new key/value pairs to it.

```csharp
// Load the web property bag
var web = await context.Web.GetAsync(p => p.AllProperties);

// Enumerate the web property bag
foreach(var property in web.AllProperties)
{
    // Do something with the property
}

// Add a new property
web.AllProperties["myPropertyKey"] = "Some value";
await web.AllProperties.UpdateAsync();

// Clear a property
web.AllProperties["myPropertyKey"] = null;
await web.AllProperties.UpdateAsync();
```

## Deleting a web

Deleting a web is a different compared to other deletes in PnP Core SDK: you can't delete the "current" loaded web, you can however delete another web using either the Delete or DeleteAsync methods.

> [!Note]
> The batch methods (DeleteBatch and DeleteBatchAsync) cannot be used to delete a web.

```csharp
// add a new web to the current web, uses default web template (STS#3) and default language (1033)
var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = "My web", Url = "myweb" });

// WORKS: delete added web
await addedWeb.DeleteAsync();

// DOES NOT WORK: deleting the current web
await context.Web.DeleteAsync();
```

## Getting changes for a web

You can use the `GetChanges` methods on an `IWeb` to list all the changes. See [Enumerating changes that happened in SharePoint](changes-sharepoint.md) to learn more.

## Re-indexing a web

SharePoint will index the content added in a web automatically. Typically re-indexing is not needed, but whenever you [make changes to the search schema (managed and crawled property settings) these will not be automatically picked up, thus requiring a re-indexing of the list or complete site](https://docs.microsoft.com/en-us/sharepoint/crawl-site-content). Re-indexing of a web is done using the `ReIndex` methods:

```csharp
// Reindex the web
await context.Web.ReIndexAsync();
```

> [!Note]
> When the web is configured for no-script, like is the case for all modern site types, then re-indexing of the web will happen by re-indexing each individual list.
