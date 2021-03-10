# Requesting data from Microsoft 365

Requesting data (e.g. a SharePoint list or a Teams channel) is usually needed when you write applications using the PnP Core SDK. There are different methods to request data, ones that request data for a given model instance (e.g. an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html)), but also ones that populate a collection of model instances (e.g. an [IListCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html))

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for requesting data
}
```

## Requesting model data

The [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) is an entry point to the connected Site, Web, Team and more...but it's also an in-memory domain model. When you're requesting data you do have a choice: you can load the data into the domain model so it stays available as long as you keep your [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) or you can load the data into a variable which stays available for lifetime of the variable.

### I want to load data into the PnPContext

To load model data into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) you need to use one of the Load methods. The following Load methods exist on each model, allowing you to perform a direct query or add a query to a batch for a grouped execution (see the [batch article](basics-batching.md)).

- [LoadAsync](https://pnp.github.io/pnpcore)
- [Load](https://pnp.github.io/pnpcore/)
- [LoadBatchAsync](https://pnp.github.io/pnpcore/)
- [LoadBatch](https://pnp.github.io/pnpcore/)

These methods allow you to perform a default load or a controlled load in which you specify which properties you want to get loaded. Doing a controlled load allows you to only retrieve what your applications needs and typically is faster compared to doing a default load.

> [!Note]
>
> - When loading data the SDK will automatically load the primary key property of a model, even if you've not requested that when doing a controlled load.
> - When you create a `PnPContext` the SDK already loads the relevant [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) and [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) properties of the `PnPContext`. When loading these [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) and [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) properties, a controlled load is done to only retrieve what's needed to support the internal workings of the SDK.

Below sample shows some of the above discussed data loads.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // The Id property of the IWeb was loaded when the context was created
    var webId = context.Web.Id;

    // Do a default load of the IWeb
    // Data is loaded into the context
    await context.Web.LoadAsync();

    // Do a controlled load of the IWeb, only the web title is loaded
    // Data is loaded into the context
    await context.Web.LoadAsync(p => p.Title);

    // Do a controlled load of the IWeb, only the web title and all the lists in the web are loaded
    // Data is loaded into the context
    await context.Web.LoadAsync(p => p.Title, p => p.Lists);

    // We're using AsRequested() to query the already loaded domain models, if not a new query would 
    // issued to load the lists
    foreach (var list in context.Web.Lists.AsRequested())
    {
        // do something with the list
    }
}
```

### I want to load data into a variable

The `Load` methods do load data into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) and that's not always what you might need: doing a `Get` method will be equivalent to the `Load` method but instead of populating the data in the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) the data is loaded into a variable. The following `Get` methods exist on each:

- [GetAsync](https://pnp.github.io/pnpcore)
- [Get](https://pnp.github.io/pnpcore)
- [GetBatchAsync](https://pnp.github.io/pnpcore)
- [GetBatch](https://pnp.github.io/pnpcore)

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Load the data into variable
    var web = await context.Web.GetAsync(p => p.Title, p => p.Lists);

    // We're using AsRequested() to query the already loaded domain models, if not a new query would 
    // issued to load the lists
    foreach (var list in web.Lists.AsRequested())
    {
        // do something with the list
    }
}
```

## Requesting model collections

Previous chapter showed how to load data starting from a single model (e.g. loading the [Title](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_Title) property of [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html)), but what if you need to load the lists of a web? One approach for loading a model collection is loading the full collection via loading the relevant parent domain model property (e.g. loading the [Lists](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_Lists) property on the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html)) as shown in previous chapter. Using this approach you can either load the data in the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) (when using a `Load` method) or into a variable (when using a `Get` method), but this approach however does not allow you to apply a filter to reduce the data being returned. Writing queries against collections is possible via the PnP Core SDK LINQ provider as shown in below sample. Note that LINQ queries always result in data loaded into a variable and not into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html).

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: Load the Lists using a model load => no filtering option
    var lists = await context.Web.GetAsync(p => p.Title, p => p.Lists);

    // Option B: Load the Lists using a LINQ query ==> filtering is possible,
    // only lists with title "Site Pages" are returned
    var lists = await context.Web.Lists.Where(p => p.Title == "Site Pages").ToListAsync();

    // Option C: we assume there's only one list with that title so we can use FirstOrDefaultAsync
    var sitePagesList = await context.Web.Lists.Where(p => p.Title == "Site Pages").FirstOrDefaultAsync();
}
```

Like with loading the model in the previous chapter you've two ways of using the data: query the data that was loaded in the context or query the data loaded into a variable:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: Use the lists loaded into the context
    await context.Web.LoadAsync(p => p.Lists);

    foreach(var list in context.Web.Lists.AsRequested())
    {
        // Use list
    }

    // Option B: Use the lists from the returned collection, you're 
    // only working with the effectively returned lists. These lists
    // are not loaded into the context
    var lists = await context.Web.Lists.Where(p => p.Title == "Site Pages").ToListAsync();

    foreach(var list in lists.AsRequested())
    {
        // Use list
    }

    // Option C: directly enumerate the lists
    await foreach(var list in context.Web.Lists)
    {
        // Use List
    }
}
```

If you want to run a LINQ query on an already loaded collection of model objects you need to use the `AsRequested()` method to return the domain model objects as an `IEnumerable` so that you can then query them via LINQ:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // All lists loaded into the context
    await context.Web.LoadAsync(p => p.Lists);

    // Query the lists in the context using LINQ
    var sitePagesList = context.Web.Lists.AsRequested().FirstOrDefault(p => p.Title == "Site Pages"); 
}
```

## Loading additional data in a single request

When you perform a model or model collection get/load you can do a controlled load specifying the model properties to return, but sometimes you want to also specify what to return from a returned property (e.g. you're loading a list and want to also load the content types and their field links). The PnP Core SDK supports these type of additional data loads via the [QueryProperties method](https://pnp.github.io/pnpcore/). This method can be used with model data loads as well as with model collection data loads as shown here:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Load the Site with the RootWeb model populated with the Title, NoCrawl and List properties,
    //   for each list load the the Title property
    await context.Site.LoadAsync(p => p.RootWeb.QueryProperties(p => p.Title, p => p.NoCrawl,
                                                                p => p.Lists.QueryProperties(p => p.Title)));

    // Loads all lists with 
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    var lists = await context.Web.Lists.QueryProperties(
                                         p => p.Title, p => p.TemplateType, 
                                         p => p.ContentTypes.QueryProperties(
                                              p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                       .ToListAsync();

    // Loads all document libraries
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    var lists = await context.Web.Lists.Where(p => p.TemplateType == ListTemplateType.DocumentLibrary)
                                       .QueryProperties(
                                         p => p.Title, p => p.TemplateType, 
                                         p => p.ContentTypes.QueryProperties(
                                              p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                       .ToListAsync();

    // Loads the first hidden document library
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    var list = await context.Web.Lists.Where(p => p.TemplateType == ListTemplateType.DocumentLibrary && p.Hidden == true)
                                      .QueryProperties(
                                        p => p.Title, p => p.TemplateType, 
                                        p => p.ContentTypes.QueryProperties(
                                             p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                      .FirstOrDefaultAsync();    
}
```

> [!Important]
> When the API uses Microsoft Graph (e.g. when working with Microsoft Teams), you cannot use nested QueryProperties statements and you can't load a given properties that depend on a separate query to work. At that point you'll see a [ClientException](https://pnp.github.io/pnpcore/api/PnP.Core.ClientException.html) being thrown with as [ErrorType Unsupported](https://pnp.github.io/pnpcore/api/PnP.Core.ErrorType.html#PnP_Core_ErrorType_Unsupported).

## What about those GetByxxx methods?

In the domain model you quite often see a GetByxxx method (e.g. GetByTitleAsync and GetByIdAsync on an IList): these methods are simply shorthands for their respective PnP Core SDK LINQ method calls. There's no functional difference between both approaches and you can use whatever "feels" the best for you.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Using this GetByTitle method is... 
    var list = await context.Web.Lists.GetByTitleAsync("Site Pages", 
                                            p => p.Title, p => p.TemplateType,
                                            p => p.ContentTypes.QueryProperties(
                                                 p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)));

    // ...identical to this GetFirstOrDefaultAsync call
    var list = await context.Web.Lists.Where(p => p.Title == "Site Pages")
                                      .QueryProperties(p => p.Title, p => p.TemplateType,
                                                       p => p.ContentTypes.QueryProperties(
                                                        p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                      .FirstOrDefaultAsync();
}
```

## Cascading data loads

Depending on the scenario you might need to perform two or more data loads to reach the model instance you need (e.g. you need to load the list before you can get the list items). You can write the code for these cascading loads in multiple ways, but each of the approaches results in two queries being sent.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Async approach: recommended

    // Option A: using an intermediate variable
    IList list = await context.Web.Lists.GetByTitleAsync("Site Pages");
    IItem item = await list.Items.GetByIdAsync(1);

    // Option B: using the AndThen method to chain async method calls
    IItem item = await context.Web.Lists.GetByTitleAsync("Site Pages").AndThen(p => p.Items.GetByIdAsync(1));

    // Option C: using multiple awaits
    IItem item = await (await context.Web.Lists.GetByTitleAsync("Site Pages")).Items.GetByIdAsync(1);

    // Synchronous approach
    IItem item = context.Web.Lists.GetByTitle("Site Pages").Items.GetById(1);
}
```
