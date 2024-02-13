# Using batching

The PnP Core SDK has been built from the ground up with batching in mind, more precisely almost all requests (gets, posts, etc) are executed via the respective SharePoint REST and Microsoft Graph batch endpoints. If you for example load the Title property of an IWeb then that request is issued via the batch endpoint where the batch consists of a single request. When you write code you can however also add more requests to a batch, effectively improving performance as you're preventing unnecessary network calls.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for batching requests
}
```

## A typical use case

When you want to add items to a list you can do that one by one or you can group the items to add in a single batch and send that batch via one of the Execute methods, the latter approach is much faster and as easy to code as the first.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    var list = await web.Lists.AddAsync("Demo", ListTemplateType.GenericList);

    // Approach A: no batching
    for (int i = 0; i < 10; i++)
    {
        Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };
        
        // Add "list item add" to the server (= roundtrip)
        await list.Items.AddAsync(values);
    }

    // Approach B: batching
    for (int i = 0; i < 10; i++)
    {
        Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };
        
        // Add "list item add" to current batch (= NO roundtrip)
        await list.Items.AddBatchAsync(values);
    }
    
    // send the batch to server (= roundtrip)
    await context.ExecuteAsync();
}
```

For most of the methods in the PnP Core SDK you'll find equivalent batch methods: the followed convention is adding the "Batch" suffix e.g. we've a Get and GetAsync but also a GetBatch and GetBatchAsync. The "batch" methods have an override that allows you to specify a batch to add the request to, if you don't specify a batch the request will be added to the "current" batch. Creating a new batch can be done using the [NewBatch method](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#PnP_Core_Services_PnPContext_NewBatch) on PnPContext. Using a dedicated batch object you can be 100% no other code path in your application adds requests to your batch.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    var list = await web.Lists.AddAsync("Demo", ListTemplateType.GenericList);

    // Approach A: batching via the current batch
    for (int i = 0; i < 10; i++)
    {
        Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };
        
        // Add "list item add" to current batch (= NO roundtrip)
        await list.Items.AddBatchAsync(values);
    }
    
    // send the batch to server (= roundtrip)
    await context.ExecuteAsync();

    // Approach B: batching via the specific batch
    var newBatch = context.NewBatch();
    for (int i = 0; i < 10; i++)
    {
        Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };
        
        // Add "list item add" to current batch (= NO roundtrip)
        await list.Items.AddBatchAsync(newBatch, values);
    }
    
    // send the batch to server (= roundtrip)
    await context.ExecuteAsync(newBatch);
}
```

## Loading data in batch

While above example showed how to add items in a batch it's also possible to batch queries that load data. When you want to load data into the context you can use the `LoadBatch` or `LoadBatchAsync` methods, loading data into a variable can be done using the respective `GetBatchAsync` and `GetBatch` methods.

```csharp
// Create a single batch that loads SharePoint Site and Web data + Teams Members. As this involves REST and Graph the net
// result will issue two batch requests going to Microsoft 365. One SharePoint batch request for the web + site data, one Microsoft
// Graph batch request for the Teams data
var batch = context.NewBatch();
await context.Web.LoadBatchAsync(batch, p => p.Lists, p => p.Features);
await context.Site.LoadBatchAsync(batch, p => p.Features, p => p.IsHubSite);
await context.Team.LoadBatchAsync(batch, p => p.Members);
await context.ExecuteAsync(batch);

// Pick a list from the in batch loaded lists
var myList = context.Web.Lists.AsRequested().FirstOrDefault(l => l.Title == "Site Pages");

// Iterate over the in batch loaded Teams members
foreach(var member in context.Team.Members.AsRequested())
{
    // do something with the members
}
```

While the above approach loads data via a batch request it does load all data in a collection, all lists, features and members were loaded. What if you would want to perform a filtered batch load? For that you can use the `AsBatchAsync` and `AsBatch` methods. When you batch load data into a variable you can use the `IsAvailable` property of the variable to check if the batch was executed before consuming the data. In case of a collection the returned variable (e.g. `siteAssets` in below sample) simply allows you to query the results.

```csharp
// Do a two list gets with filter and custom properties specifying the data to load
var siteAssets = await context.Web.Lists.QueryProperties(
        p => p.Title, p => p.TemplateType,
        p => p.ContentTypes.QueryProperties(
            p => p.Name, 
            p => p.FieldLinks.QueryProperties(p => p.Name)))
    .Where(p => p.Title == "Site Assets")
    .AsBatchAsync();

var sitePages = await context.Web.Lists.QueryProperties(
        p => p.Title, p => p.TemplateType,
        p => p.ContentTypes.QueryProperties(
            p => p.Name, 
            p => p.FieldLinks.QueryProperties(p => p.Name)))
    .Where(p => p.Title == "Site Pages")
    .AsBatchAsync();

// Batch results are not available right now
Assert.IsFalse(siteAssets.IsAvailable);
Assert.IsFalse(sitePages.IsAvailable);

// Execute the batch
await context.ExecuteAsync();

// Batch results are now available
Assert.IsTrue(siteAssets.IsAvailable);
Assert.IsTrue(sitePages.IsAvailable);

// Get the loaded data to use it
var siteAssetsList = siteAssets.AsEnumerable().First();
var sitePagesList = sitePages.AsEnumerable().First();
```

In case of a collection the returned variable (e.g. `siteAssets` in above sample) simply allows you to query the results. When you're returning a simple type or a single model (e.g. `web` and `site` in below sample) you can access the batch loaded data via the `Result` property. Checking whether the batch was executed still happens via the `IsAvailable` property.

```csharp
var batch = context.NewBatch();
var web = await context.Web.GetBatchAsync(batch, p => p.Lists, p => p.Features);
var site = await context.Site.GetBatchAsync(batch, p => p.Features, p => p.IsHubSite);

// Batch results are not available right now
Assert.IsFalse(web.IsAvailable);

// Execute the batch
await context.ExecuteAsync(batch);

// Batch results are now available
Assert.IsTrue(web.IsAvailable);

// Pick a list from the in batch loaded lists
var myList = web.Result.Lists.AsRequested().FirstOrDefault(l => l.Title == "Site Pages");
```

## Batch limits

PnP Core SDK is not imposing limits on the number of requests you can add to a single batch before executing the batch, but internally the SDK uses the official limits being maximum 20 requests for a Graph batch and 100 requests for a SharePoint REST batch. What that means is that you for example can add 1000 items into a single batch and execute that batch, during execution that batch will then be split into 10 batches of 100 items and each of these 10 batches will be executed sequentially resulting in 10 network calls to the respective batch endpoint.

## Handling batch failures

The default behavior is that whenever a batch response is processed and a failing request inside the batch was detected an exception is thrown. This default mode is useful when you build small batches. However, when you perform bulk adds or bulk deletions then you don't want your batch processing being interrupted on a single failure. A sample case could be this: imagine you've created a batch to delete all list items from a list, but in parallel someone else already deleted an item you're also deleting via the submitted batch. With the default batch behavior you'll get an exception stating an item was not found, but you can turn off the `ThrowOnError` setting by providing it to the [ExecuteAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#collapsible-PnP_Core_Services_PnPContext_ExecuteAsync_System_Boolean_) method and then the batch continues and you'll get a list of [BatchResult](https://pnp.github.io/pnpcore/api/PnP.Core.Services.BatchResult.html) (= the errors) which you then have to handle in your code.

```csharp
// Create a list
var myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

// Add 150 items to the list
for (int i = 1; i <= 150; i++)
{
    Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };

    await myList.Items.AddBatchAsync(values);
}
await context.ExecuteAsync();

// Delete some items from within both 1..100 and 101..150 sets
await myList.Items.DeleteByIdBatchAsync(5);
await myList.Items.DeleteByIdBatchAsync(50);
await myList.Items.DeleteByIdBatchAsync(125);
await context.ExecuteAsync();

// Build a batch to delete all 150 items
for (int i = 1; i <= 150; i++)
{
    await myList.Items.DeleteByIdBatchAsync(i);
}

// Execute the batch without throwing an error, gets you a collection of errors back
var batchResponse = await context.ExecuteAsync(false);

foreach(var batchResponse in batchResponses)
{
    // Do something with the failed request
}

```

It's also possible to track additional request information (e.g. the id of the deleted items) and then link the failing requests in a batch back to that tracked information. Below code snippet shows how this could be implemented.

```csharp
// Create a list
var myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.GenericList);

// Add 150 items to the list
for (int i = 1; i <= 150; i++)
{
    Dictionary<string, object> values = new Dictionary<string, object>
        {
            { "Title", $"Item {i}" }
        };

    await myList.Items.AddBatchAsync(values);
}
await context.ExecuteAsync();

// Delete some items from within both 1..100 and 101..150 sets
await myList.Items.DeleteByIdBatchAsync(5);
await myList.Items.DeleteByIdBatchAsync(50);
await myList.Items.DeleteByIdBatchAsync(125);
await context.ExecuteAsync();

var batch = context.NewBatch();

// Sample on how to track additional information for a batch request
Dictionary<Guid, int> deletedListItemIds = new();

for (int i = 1; i <= 150; i++)
{
    await myList.Items.DeleteByIdBatchAsync(batch, i);
    deletedListItemIds.Add(batch.Requests.Last().Value.Id, i);
}

// Execute the batch without throwing an error, should get a result collection back
var batchResponse = await context.ExecuteAsync(batch, false);                

// Find the corresponding batch requests
foreach (var errorResult in errorResults)
{
    var failedListItemIdDelete = deletedListItemIds.FirstOrDefault(p=> p.Key == errorResult.BatchRequestId).Value;
}
```
