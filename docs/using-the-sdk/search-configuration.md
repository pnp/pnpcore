# Configuring search

The search configuration is built up out of a search schema (e.g. defining managed properties) and search related settings an site level, all which can be controlled via the PnP Core SDK.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with files
}
```

## Exporting the search schema from a web or site

The search schema can be exported and imported via an XML file, exporting is done using one of the `GetSearchConfigurationXml` methods on either `IWeb` or `ISite`:

```csharp
// Get Web search configuration XML
var searchConfigXmlWeb = await context.Web.GetSearchConfigurationXmlAsync();

// Get Site search configuration XML
var searchConfigXmlSite = await context.Site.GetSearchConfigurationXmlAsync();
```

If you're only interested in understanding which managed properties were added for the `IWeb` or `ISite` then you can also use one of the `GetSearchConfigurationManagedProperties` methods. These methods get the needed search configuration XML first and then parses it to return a collection of managed properties:

```csharp
// Get Web search configuration managed properties
var webManagedProperties = await context.Web.GetSearchConfigurationManagedPropertiesAsync();
foreach(var mp in webManagedProperties)
{
    // Do something
}

// Get Site search configuration managed properties
var siteManagedProperties = await context.Site.GetSearchConfigurationManagedPropertiesAsync();
foreach(var mp in siteManagedProperties)
{
    // Do something
}
```

## Importing the search schema for a web or site

Just like the search schema can be exported it's also possible to import it again at `IWeb` or `ISite` level via calling the `SetSearchConfigurationXml` methods:

```csharp
// Set Web search configuration XML
await context.Web.SetSearchConfigurationXmlAsync(searchConfigXmlWeb);

// Set Site search configuration XML
await context.Site.SetSearchConfigurationXmlAsync(searchConfigXmlSite);
```
