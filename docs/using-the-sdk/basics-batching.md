# Using batching

The PnP Core SDK has been built from day 1 with batching in mind, more precisely almost all operations (gets, posts, etc) are executed via the respective SharePoint REST and Microsoft Graph batch endpoints. If you for example load the Title property of an IWeb then that request is issued via the batch endpoint where the batch consists out of just one request. When you write code you can however also add more requests to a batch, effectively improving performance as you're preventing unnecessary roundtrips.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for batching requests
}
```

## A typical use case

When you want to add items to a list you can do that one by one or you can group the items to add in single batch and send that batch via one of the Execute methods, the latter approach is however much faster and as easy to code.

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

For most of the methods in the PnP Core SDK you'll find equivalent batch methods: the followed convention is adding the "Batch" suffix e.g. we've a Get and GetAsync but also a GetBatch and GetBatchAsync. The "batch" methods have an override that allows you to specify a batch to add the request to, if you don't specify a batch the request will be added to the "current" batch. Creating a new batch can be done using the [NewBatch method](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#PnP_Core_Services_PnPContext_NewBatch) on PnPContext.

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
