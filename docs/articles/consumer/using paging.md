# Using paging

Being able to retrieve data in a paged manner is important when you want to use the first data rows while you're loading still additional data, but also when you're loading large data sets. When you page data you can start from either a `GetPagedAsync()` method call or from a linq query that uses the `Take()` method. Once you've done one of these calls to Microsoft 365 can use the paging methods to get additional pages.

## Supported paging attributes and methods

Method/Attribute | Description
-----------------| -----------
`GetPagedAsync(pageSize, expression)` | Loads the first page of a given size. Optionally an expression can be specified to only return the properties you need
`CanPage` | This attribute indicates whether you can use the paging API's to request a next page or to request all remaining pages
`GetNextPageAsync()` | Get's the next page, this method assumes you've already loaded a first page using either the `GetPagedAsync()` method or via a linq query that included the `Take()` method
`GetAllPagesAsync()` | Loads all the pages in a collection until there's no new data returned anymore. This method assumes you've already loaded a first page using either the `GetPagedAsync()` method or via a linq query that included the `Take()` method

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
