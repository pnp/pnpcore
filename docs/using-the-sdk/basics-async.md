# Asynchronous versus synchronous

Loading data (e.g. a SharePoint list or a Teams channel), updating, adding or deleting...all of these operations can be done either asynchronously (=async) or synchronously (=sync). In this chapter we explain the pro's and con's of each approach and provide some best practices.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext
}
```

## Async or sync?

Let's start with two code snippets which both result in a list item being loaded. The first snippet shows a sync way of loading a list via it's title followed by getting the item with id 1 from that list:

```csharp
using (var context = pnpContextFactory.Create("SiteToWorkWith"))
{
    IItem item = context.Web.Lists.GetByTitle("Site Pages").Items.GetById(1);
}
```

In the second example we do the same but then async:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    IList list = await context.Web.Lists.GetByTitleAsync("Site Pages");
    IItem item = await list.Items.GetByIdAsync(1);
}
```

> [!Note]
> Both approaches use 2 queries, there's no difference between async and sync when it comes to query efficiency.

Both approaches are comparable from a coding point of view: the difference is in the method names and the `await` keyword. You might also notice that for the sync approach the code is more fluent as there's no need for `await` statements, but a similar thing can be done with async via using the [AndThen](https://pnp.github.io/pnpcore/api/PnP.Core.QueryModel.BaseDataModelExtensions.html#collapsible-PnP_Core_QueryModel_BaseDataModelExtensions_AndThen__2_Task___0__Func___0_Task___1___) method to chain async method calls or via nesting each async call with it's corresponding `await` keyword:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: Using AndThen() to chain async method calls
    IItem item = await context.Web.Lists.GetByTitleAsync("Site Pages").AndThen(p => p.Items.GetByIdAsync(1));

    // Option B: using multiple awaits
    IItem item = await (await context.Web.Lists.GetByTitleAsync("Site Pages")).Items.GetByIdAsync(1);
}
```

In general the recommendation is to use the async methods because:

- They result in better performing code
- Improved application responsiveness (the UI thread is not blocked when a data retrieval is ongoing)
- Prevents deadlocks when using the PnP Core SDK from WPF apps and [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
  
Internally in the PnP Core SDK everything always happens async, the sync methods you see are wrappers over their async counterparts with a `GetAwaiter().GetResult()` to force the code to wait for the outcome.

> [!Important]
> Using the async methods is strongly recommended. Use the sync methods if you're using the PnP Core SDK in an already sync code base, if not use async.

If you want to learn more about async programming checkout these resources:

- [Asynchronous programming with async and await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Long Story Short: Async/Await Best Practices in .NET](https://medium.com/@deep_blue_day/long-story-short-async-await-best-practices-in-net-1f39d7d84050)
