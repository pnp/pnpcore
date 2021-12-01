# SharePoint CSOM vs PnP Core SDK

Some PnP Core SDK code looks very similar to the SharePoint CSOM code. While it feels like the PnP Core SDK works the same as CSOM, in some situations, it's not the case. Let's explore the similarities/differences between CSOM and PnP Core SDK.

## The context

In the same way as for CSOM, in the PnP Core SDK it's all starts with the context:

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
}
```

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
}
```

`PnPContext` is the starting point for all PnP Core SDK-related operations. However you have a lot more options on how to instantiate a `PnPContext` - using configuration name, site url, group id, using default or custom authentication provider and so on. All the method overloads are available at the `PnpContextFactory`.

## Loading objects

### Loading data into the context

You can load data directly to the `PnPContext`, which is very similar to the CSOM. In the PnP Core SDK all *`Load*`* methods are available on the SharePoint model objects themselves.

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
  csomContext.Load(csomContext.Web, w => w.Title, w => w.Id);

  csomContext.ExecuteQuery(); // executes the actual HTTP request

  Console.WriteLine(csomContext.Web.Title);
}
```

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  await context.Web.LoadAsync(w => w.Title, w => w.Id); // HTTP request is executed immediately

  Console.WriteLine(context.Web.Title);
}
```

Here we can see the very first and the very important difference:

> [!Note]
>
> All *`Load*`* methods in PnP Core SDK immediately execute the request, in contradiction to the CSOM where you should explicitly call `ExecuteQuery`. The only exception from this rule is [batching](./basics-batching.md) (*`LoadBatch*`* methods).

If you use batching, you should explicitly call `Execute` method:

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  await context.Web.LoadBatchAsync(w => w.Title, w => w.Id);

  await context.ExecuteAsync(); // executes the actual HTTP request with all batched queries

  Console.WriteLine(context.Web.Title);
}
```

### Loading data into the variable

With CSOM you can also load data into the variable:

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
  var web = csomContext.Web;
  csomContext.Load(web, w => w.Title, w => w.Id);

  var list = web.Lists.GetByTitle("Documents");
  csomContext.Load(list);

  csomContext.ExecuteQuery(); // executes the actual HTTP request

  Console.WriteLine(list.Title);
  Console.WriteLine(web.Title);
}
```

The PnP Core SDK equivalent will be:

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  var list = await context.Web.Lists.GetByTitleAsync("Documents"); // executes the first HTTP request
  var web = await context.Web.GetAsync(w => w.Title, w => w.Id); // executes the second HTTP request

  Console.WriteLine(list.Title);
  Console.WriteLine(web.Title);
}
```

The key difference is that the PnP Core SDK always creates a new variable when you use *`Get*`* methods, whereas in the CSOM sometimes you cannot do the same and can only reference a variable from the context. In the latter sample, the PnP Core SDK sends two HTTP requests to get data whereas the CSOM sends only one. If you want fewer HTTP requests in your code consider [batching](./basics-batching.md) approach.

Also consider a code to get an item by its id:

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
  var item = csomContext.Web.Lists.GetByTitle("AddTest").GetItemById(1);
  csomContext.Load(item);
  csomContext.ExecuteQuery();
}
```

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  var item = context.Web.Lists.GetByTitle("AddTest").Items.GetById(1);
}
```

In the latter case the code sends two HTTP requests to get the item. This is because we write it in a synchronous way. To learn more why it happens, check out [async vs sync](./basics-async.md) page.

## Loading collections

The differences in loading collections between the CSOM and the PnP Core SDK are somewhat similar to the regular object loading. You can either load it into the context or as a separate variable.

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
  // load the list collection into the context
  csomContext.Load(csomContext.Web.Lists, lists => lists.Include(
                                                    l => l.Id,
                                                    l => l.Title,
                                                    l => l.RootFolder));

  // load into the variable
  var lists = csomContext.LoadQuery(csomContext.Web.Lists.Where(l => !l.Hidden));

  // execute the request
  csomContext.ExecuteQuery();
}
```

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  // load the list collection into the context
  await context.Web.Lists.LoadAsync(l => l.Id, l => l.Title, l => l.RootFolder); // immediately executes the request

  // load into the variable
  var lists = await context.Web.Lists.Where(l => l.Hidden == false).ToListAsync(); // immediately executes the request
}
```

In the PnP Core SDK, if you need to filter a collection, you can apply the LINQ filter query directly to the collection. Upon execution, the LINQ will be translated to the REST OData filter.

## CRUD operations

CRUD operations are very similar in both libraries. Consider the code to update a list item:

*CSOM:*

```csharp
using (var csomContext = new ClientContext(siteUrl))
{
  var list = csomContext.Web.Lists.GetByTitle("ListTitle");
  var item = list.GetItemById(1);

  item["Title"] = "new title";

  item.Update();

  csomContext.ExecuteQuery();
}
```

*PnP Core SDK:*

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
  var list = await context.Web.Lists.GetByTitleAsync("AddTest");
  var item = await list.Items.GetByIdAsync(1);

  item["Title"] = "new value";
  await item.UpdateAsync(); // sends HTTP request to update the item
}
```

In the PnP Core SDK an item is updated immediately at the `UpdateAsync` line, in the CSOM you have to call `Update` and `ExecuteQuery` to commit the changes. If you need to CRUD multiple elements, consider [batching](./basics-batching.md).
