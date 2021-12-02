# Multi-threading with PnP Core SDK

The key PnP Core SDK classes like `PnPContext`, the model classes (e.g. `IWeb`, `ITeamChannel`,...) and model collection classes (e.g. `IListCollection`, `ITeamChannelCollection`) are not thread-safe and therefore it's not recommended to build solutions that have multiple threads sharing these classes. Doing so may lead to unexpected results and exceptions.

## What options do I have to parallelize operations?

The main thing to do is to **create a `PnPContext` per thread** by either creating a new one (e.g. if you're running a thread per site/web) or by cloning the current one in case you're parallelizing work inside the same site/web. Below code samples show two implementations of running operations for a given `IWeb` in parallel:

```csharp
var urls = new[] { "https://contoso.sharepoint.com/sites/siteA", "https://contoso.sharepoint.com/sites/siteB" };

var parallelOps = new List<Task>();

foreach (var url in urls)
{
    var contextForSite = await context.CloneAsync(new Uri(url));
    parallelOps.Add(DoWork(contextForSite));
}
await Task.WhenAll(parallelOps);

private async Task DoWork(PnPContext context)
{
    await foreach (var list in context.Web.Lists.Where(p => p.Hidden == false).AsAsyncEnumerable())
    {
        // do something with the list
        context.Logger.LogInformation($"Thread: {Thread.CurrentThread.ManagedThreadId}, list {list.Title}");
    }
    
    context.Dispose();
}
```

Slightly more complicated sample to setup, but once configured using a parallel async operation becomes really simple.

```csharp
// AsyncParallelForEach extension method
public static class ParallelExtensions
{    
    public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
    {
        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
        if (scheduler != null)
            options.TaskScheduler = scheduler;
        var block = new ActionBlock<T>(body, options);
        await foreach (var item in source)
            block.Post(item);
        block.Complete();
        await block.Completion;
    }
}

// Simple code to return test data as IAsyncEnumerable
public static async IAsyncEnumerable<string> GetUrlsToProcess()
{
    yield return "https://contoso.sharepoint.com/sites/siteA";
    yield return "https://contoso.sharepoint.com/sites/siteB";

    await Task.CompletedTask; 
}  

private async Task DoWorkAsync(PnPContext context)
{
    await foreach (var list in context.Web.Lists.Where(p => p.Hidden == false).AsAsyncEnumerable())
    {
        // do something with the list
        context.Logger.LogInformation($"Thread: {Thread.CurrentThread.ManagedThreadId}, list {list.Title}");
    }
    
    context.Dispose();
}

// Perform async parallel foreach
await GetUrlsToProcess().AsyncParallelForEach(async url =>
{
    var contextForSite = await context.CloneAsync(new Uri(url));
    await DoWorkAsync(contextForSite);
});
```
