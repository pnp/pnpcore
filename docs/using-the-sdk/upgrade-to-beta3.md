# Upgrade to Beta3

Beta3 introduces some fundamental changes which will require minor updates in your code base. These changes are done to provide a more consistent experience while also doing internal refactoring to centralize to a common query engine regardless of the entry point. In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    
}
```

## Getting data

Working with PnP Core SDK starts from a [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) which provides you with access to the connected Site, Web and Team but which also acts as an in memory domain model. Before Beta 3 all data you requested got loaded into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html). Any `Get` call on either a model (e.g. doing `context.Web.GetAsync(p => p.Title)`) or a collection (e.g. doing `context.Web.Lists.GetAsync(p => p.Title, p => p.TemplateType)`) loaded data into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) whereas now the default behavior for `Get` calls is to load the data into a variable. Loading data into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) is still possible using the new `Load` calls.

### Requesting a single model (e.g. a Web, List,...)

When requesting a model you can choose whether data is loaded into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) or not:

Pre Beta3 | Beta3 and later | Loads data into PnPContext
----------|-----------------|-------------------------
GetAsync() | LoadAsync() | Yes
Get() | Load() | Yes
GetBatchAsync() | LoadBatchAsync() | Yes
GetBatch() | LoadBatch() | Yes
N/A | GetAsync() | No
N/A | Get() | No
N/A | GetBatchAsync() | No
N/A | GetBatch() | No

> [!Note]
> When doing a `Load` specifying a model collection (e.g. `context.Web.LoadAsync(p => p.Lists)`) then all the data is loaded into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html). This approach is the only option to load a collection into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html), and unless it is really needed, you should avoid this technique and rather use paging.

### Requesting a collection of models

Loads initiated from a collection always return data into a variable and never into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) because using this approach you can also apply filters (using `Where`), or other query methods, which would result in an incomplete dataset in the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html). You can now chain methods (`Where`, `QueryProperties`, etc.) with a fluent syntax and trigger the collection query execution once you are ready, using methods like `ToList` or `ToListAsync`, or enumerating the result of the query.

Pre Beta3 | Beta3 and later | Loads data into PnPContext
----------|-----------------|-------------------------
GetAsync() | ToListAsync() | No
Get() | ToList() | No
GetBatchAsync() | AsBatchAsync() | No
GetBatch() | AsBatch() | No
GetFirstOrDefaultAsync() | FirstOrDefaultAsync() | No
GetFirstOrDefault() | FirstOrDefault() | No
GetFirstOrDefaultBatchAsync() | FirstOrDefault().AsBatchAsync() | No
GetFirstOrDefaultBatch() | FirstOrDefault().AsBatch() | No

## Querying collections of models

Starting from Beta3 you can easily leverage a [LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/) custom query provider that translates your LINQ queries into actual REST queries for Microsoft Graph or for Microsoft SharePoint REST APIs.

For example, to get the items of a SharePoint list, filtered by title and just loading their Id and Title fields, you can write a query like the following one:

```csharp
var query = (from i in context.Web.Lists.GetByTitle(listName).Items
                where i.Title == itemTitle
                select i)
                .QueryProperties(l => l.Id, l => l.Title);
```

You will then be able to consume the collection either using an enumeration constructor or a method like `ToList`.
Here you can see an example using an enumeration:

```csharp
foreach (var item in query)
{
    Console.WriteLine($"{item.Id} - {item.Title}");
}
```

While here you can see how to use the `ToList` method.

```csharp
var queryResult = query.ToList();
```

The above query can also be written just using the LINQ Extension Methods, rather than the LINQ query syntax, like it is illustrated in the following code excerpt:

```csharp
var query = context.Web.Lists.GetByTitle(listName).Items
        .Where(i => i.Title == itemTitle)
        .QueryProperties(l => l.Id, l => l.Title);
```

Under the cover, there will be exactly the same query model, regardless the syntax you like to use.

> [!Important]
> Since Beta3, whenever you enumerate a query or whenever you define a new query, the query provider will execute the query online targeting the back-end APIs, and you will always get fresh data. 

### Querying loaded collections using a foreach or LINQ to Objects

If you don't want to execute an online query targeting the back-end APIs, and you rather want to use data that you already loaded in memory with a previous query, you can use the `AsRequested()` method applied to a pre-loaded collection. As such, you will be able to enumerate the already in-memory data, eventually using LINQ to Objects or any other object browsing technique of your choice. In fact, the `AsRequested()` method casts the collection to an `IEnumerable` and gives you access to the in-memory copy of data.

```csharp
// Load all lists in the PnPContext
await context.Web.LoadAsync(p => p.Lists);

// Query the loaded lists via LINQ to Objects
var documentLibraries = context.Web.Lists.AsRequested().Where(p => p.TemplateType == ListTemplateType.DocumentLibrary);

// Iterate over the loaded lists
foreach(var list in context.Web.Lists.AsRequested())
{
    // do something with the list
}
```

> [!Note]
> As of beta3 you need to use `QueryProperties` instead of `LoadProperties` when you want to specify the properties to load into the model.
