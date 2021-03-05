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
> When doing a `Load` specifying a model collection (e.g. `context.Web.LoadAsync(p => p.Lists)`) then all the data is loaded into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html). This approach is the only option to load a collection into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html), and unless it is really needed, you should avoid this technique and rather use paging to avoid overloading the server-side APIs.

### Requesting a collection of models

Loads initiated from a collection always return data into a variable and never into the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) because using this approach you can also apply a filter (using `Where`) which would result in an incomplete dataset in the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html).

Pre Beta3 | Beta3 and later | Loads data into PnPContext
----------|-----------------|-------------------------
GetAsync() | ToListAsync() | No
Get() | ToList() | No
GetBatchAsync() | AsBatchAsync() | No
GetBatch() | AsBatch() | No
GetFirstOrDefaultAsync() | FirstOrDefaultAsync() | No
GetFirstOrDefault() | FirstOrDefault() | No
GetFirstOrDefaultBatchAsync() | AsBatchAsync() | No
GetFirstOrDefaultBatch() | AsBatch() | No