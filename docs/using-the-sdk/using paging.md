# Using paging

Being able to retrieve data in a paged manner is important when you want to use the first data rows while you're loading still additional data, but also when you're loading large data sets. When you page data you can start from either a `GetPagedAsync()` method call or from a linq query that uses the `Take()` method. Once you've done one of these calls to Microsoft 365 can use the paging methods to get additional pages.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for getting paged data
}
```

## Supported paging attributes and methods

Method/Attribute | Description
-----------------| -----------
`GetPagedAsync(filter, pageSize, expression)` | Loads the first page of a given size with a filter scoping down the pages to request. Optionally an expression can be specified to only return the properties you need
`GetPagedAsync(pageSize, expression)` | Loads the first page of a given size. Optionally an expression can be specified to only return the properties you need
`CanPage` | This attribute indicates whether you can use the paging API's to request a next page or to request all remaining pages
`GetNextPageAsync()` | Get's the next page, this method assumes you've already loaded a first page using either the `GetPagedAsync()` method or via a linq query that included the `Take()` method
`GetAllPagesAsync()` | Loads all the pages in a collection until there's no new data returned anymore. This method assumes you've already loaded a first page using either the `GetPagedAsync()` method or via a linq query that included the `Take()` method

> [!Note]
> For all these methods there's also a synchronous equivalent (just drop `Async` from the method name).

## Examples

### Starting via the GetPagedAsync() method

This example shows how to use paging to load lists, in the sample only the `Title` property of the list is requested, if you do not provide the expression then list default properties are loaded.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Get a first page of lists of size 2
    await context.Web.Lists.GetPagedAsync(2, p => p.Title);

    // Do we have a pointer to a next page?
    if (context.Web.Lists.CanPage)
    {
        // Load the next page
        await context.Web.Lists.GetNextPageAsync();

        // Load all pages
        await context.Web.Lists.GetAllPagesAsync();
    }
}
```

### Starting via the GetPagedAsync() method with a filter and complex data load expression

This example builds on top of the previous but shows some additional capabilities:

- A complex expression is used to define which List properties to load
- A filter expression is specified so that only a subset of lists are returned
- The returned, paged, data is made available via the method call output

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Force rest
    context.GraphFirst = false;

    // Get a first page of lists of size 2 
    //   filtered to generic lists
    //     returning list properties
    //     - Title and TemplateType
    //     - ContentTypes with for each content type the FieldLinks loaded
    var lists = await context.Web.Lists.GetPagedAsync(p => p.TemplateType == ListTemplateType.GenericList, 
                                                      2,
                                                      p => p.Title, p => p.TemplateType,
                                                        p => p.ContentTypes.LoadProperties(
                                                            p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));

    // Do we have a pointer to a next page?
    if (context.Web.Lists.CanPage)
    {
        // Load the next page
        var nextLists = await context.Web.Lists.GetNextPageAsync();

        // Load all pages
        await context.Web.Lists.GetAllPagesAsync();
    }
}
```

### Starting via a linq query with Take()

In this example a linq query is executed first using the `Take()` method. Once the linq query was execution was triggered (in this case by calling `ToListAsync()`), you can using the paging methods to get additional pages of data.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Issue a linq query
    var lists = context.Web.Lists.Take(2);
    var queryResult = await lists.ToListAsync();

    // Do we have a pointer to a next page?
    if (context.Web.Lists.CanPage)
    {
        // Load the next page
        await context.Web.Lists.GetNextPageAsync();

        // Load all pages
        await context.Web.Lists.GetAllPagesAsync();
    }
}
```
