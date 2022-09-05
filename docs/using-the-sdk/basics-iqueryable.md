# [`IQueryable`](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryable) performance considerations

In the [Requesting model collections](basics-getdata.md#requesting-model-collections) section you saw quite a lot of different examples of how to query collections. Almost all collections inside PnP Core SDK implement an [`IQueryable`](https://docs.microsoft.com/en-us/dotnet/api/system.linq.iqueryable) interface.

Thanks to this you can use LINQ expressions to dynamically filter or asynchronously load collection elements on demand. All your LINQ expressions will be accurately translated to the REST OData query operations (like `$filter`, `$select`, `$expand`, etc).

Having below code:

```csharp
var lists = await context.Web.Lists
  .Where(l => l.Hidden == false && l.TemplateType == ListTemplateType.DocumentLibrary)
  .QueryProperties(p => p.Title, p => p.TemplateType, p => p.ContentTypes.QueryProperties(p => p.Name)).ToListAsync();
```

upon execution will be translated to the below REST OData query:

```bash
_api/web/lists?$select=Id,Title,BaseTemplate,ContentTypes/Name,ContentTypes/StringId&$expand=ContentTypes&$filter=(BaseTemplate+eq+101)
```

It's a very powerful feature, however let's take a closer look at this technique to avoid some common performance issues.

> [!Important]
>
> The most important rule of `IQueryable` is that an `IQueryable` doesn't fire a request when it's declared, but only when it's enumerated over (inside foreach cycle or when calling `ToList()`/`ToListAsync()`).

## Loading collections into the PnPContext

Let's have a sample query to get a web's lists:

❌ *not efficient:*

```csharp
// All lists loaded into the context
await context.Web.LoadAsync(p => p.Lists);

foreach (var list in context.Web.Lists)
{
  // do something with the list here
}
```

What's wrong with this code? It works just fine, however it sends two identical HTTP requests to the SharePoint server to get lists (one in `LoadAsync(p => p.Lists)` and the second one in the `foreach` cycle). Why does it happen? Because `Lists` property implements `IQueryable`, inside `foreach` cycle you effectively enumerate the `IQueryable`, as a result, it sends an HTTP request to get data.

How to fix the code? Use `AsRequested()`:

✅ *better:*

```csharp
// All lists loaded into the context
await context.Web.LoadAsync(p => p.Lists);

foreach (var list in context.Web.Lists.AsRequested())
{
  // do something with the list here
}
```

As mentioned earlier, `AsRequested()` method returns an already loaded collection of items, you should use this method to avoid multiple unnecessary HTTP requests. In this case, we enumerate a collection loaded in memory before.

Alternatively, you can also use just one cycle without `LoadAsync(p => p.Lists)`:

✅ *better:*

```csharp
await foreach (var list in context.Web.Lists)
{
  // do something with list here
}
```

In this case, list collection will be requested at the beginning of the `foreach` cycle. Do remember though, that if you iterate over collection again somewhere in your code path, an additional request will be sent.

## Load related properties

The below code has a similar problem with the query efficiency:

❌ *not efficient:*

```csharp
var list = await context.Web.Lists.GetByTitleAsync("Documents", l => l.Fields);
var fields = await list.Fields.Where(l => l.InternalName.StartsWith("tax")).ToListAsync();
```

The first line loads a list by title and also loads related property - all list fields. On the second line we again send HTTP request to further filter fields. But what we need instead is to filter already loaded fields:

```csharp
var fields = list.Fields.AsRequested().Where(l => l.InternalName.StartsWith("tax")).ToList();
```

To make it even more efficient, you should change it like this:

✅ *better:*

```csharp
var list = await context.Web.Lists.GetByTitleAsync("Documents");
var fields = await list.Fields.Where(l => l.InternalName.StartsWith("tax")).ToListAsync();
```

It doesn't make sense to load all related fields with the list request. Thus we simply send a separate request with a filter (will be translated to the `$filter=startswith` OData query) to get desired fields.

## Cycles and/or method calls

Could you guess what's the problem with the below code:

❌ *not efficient:*

```csharp
var filteredList = context.Web.Lists.Where(l => l.TemplateType == ListTemplateType.DocumentLibrary);

for (int i = 0; i < 10; i++)
{
    DoSmth(filteredList);
}

private bool DoSmth(IEnumerable<IList> lists)
{
    foreach (var list in lists)
    {
        // do smth with list
    }
}
```

It also works just fine, however has an issue, that the above code sends 10 HTTP requests to get lists data. The `filteredList` is an instance of `IQueryable<IList>`, that's why it doesn't execute the request immediately, but only inside the `foreach` cycle in the `Check` function. Every time we visit the function, we send an HTTP request to get lists.

How to fix it? Change the filter query so that it executes immediately using `ToList()` or `ToListAsync()` methods:

✅ *better:*

```csharp
var filteredList = await context.Web.Lists.Where(l => l.TemplateType == ListTemplateType.DocumentLibrary).ToListAsync();
```

The code above executes the request instantly and loads all items into the memory, thus we don't have the issue with multiple HTTP queries. The type of `filteredList` will be `IList`, not `IQueryable`.
