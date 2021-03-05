# Using paging

Being able to retrieve data in a paged manner is important when you want to use the first data rows while you're loading still additional data, but also when you're loading large data sets. When you page data you can start from a LINQ query or from a whole collection of items. Either way, PnP Core SDK provides you "implicit paging", meaning that when you browse (for example with a `foreach` constructor) the items in the collection, under the cover PnP Core SDK will retrieve data page by page, giving you back data like it was already in memory (continous querying with implicit paging).
If you rather want to have full control on paging data, you can use the `Take()` method and from that point on PnP Core SDK will handle paging for you.

> [!Note]
> In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:
```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for getting paged data
}
```

## Implicit paging

In this example the messages in a Team channel are queried continously, using implicit paging. When doing so the underlying Graph API call will return a "next page link" automatically and if so this will be used to get the next messages. The paging dynamics will be completely transparent to you, and you will simply get back the data that you are looking for, with an optimized and paged querying approach.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Retrieve the already created channel
    var channelForPaging2 = context.Team.Channels.FirstOrDefault(p => p.DisplayName == "My Channel");

    // Retrieve the messages page by page and asynchronously in a trasparent way 
    await foreach(var message in channelForPaging2.Messages)
    {
        // do something with the message
    }
}
```

As you can see, you can just focus on consuming data, while under the cover PnP Core SDK will retrieve the items for you, page by page, optimizing bandwith and requests. Notice the `await` keyword just before the `foreach` constructor, in order to make the continous query fully asynchronous and highly optimized. This is the suggested pattern for consuming large collections of items. 

If your query needs to apply filters or properties selection on the queried data, you can leverage the `AsAsyncEnumerable` method to keep the query asynchronous and paged with continous implicit paging. Here is an example.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Efficient, dynamic and asynchronous with whatever LINQ query you like
    await foreach(var list in context.Web.Lists.Where(l => l.Title == "Documents").AsAsyncEnumerable())
    {
        // do something with the list
    }
}
```

If your code is not asynchronous, and as such you cannot rely on the `await` keyword just before the `foreach` constructor, you can still use the synchronous enumeration of items, like it is illustrated in the following sample.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Retrieve the already created channel
    var channelForPaging2 = context.Team.Channels.FirstOrDefault(p => p.DisplayName == "My Channel");

    // Retrieve the messages page by page, synchronously in a trasparent way 
    foreach(var message in channelForPaging2.Messages)
    {
        // do something with the message
    }
}
```

You will still get the whole set of items (messages in the previous example) with multiple REST queries page by page. However, the querying of all the items will be synchronous and your code will be blocked while waiting for the whole set of items to be queried. This a sub-optimal scenario, that you should try to avoid, preferring the asynchronous model. 

## Full load of items

Another option that you have is to load in memory the whole set of items, using any of the `Load` methods available in PnP Core SDK.

> [!Note]
> You can find further details about the `Load` methods in the article [Upgrade to Beta3](upgrade-to-beta3.md).

In the following sample, you can see how to load in memory the whole set of messages of a Teams channel.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Retrieve the already created channel
    var channelForPaging2 = context.Team.Channels.FirstOrDefault(p => p.DisplayName == "My Channel");

    // Load the messages, this will load all messages via paged requests
    await channelForPaging2.LoadAsync(p => p.Messages);

    // Consume the in-memory items 
    foreach(var message in channelForPaging2.Messages.AsRequested())
    {
        // do something with the message
    }
}
```

The `AsRequested` method will browse the already in-memory items. The items will be loaded just once by the `Load` method.

## Paging via the Take()/Skip() LINQ methods

If you want to have full control on paging data, you can rely on the `Take`/`Skip` extension methods. In the following example you can see how to use manual paging to load lists.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Get a first page of lists of size 2
    var lists = await context.Web.Lists.Take(2).QueryProperties(p => p.Title).ToListAsync();

    // Get the next page
    lists = await context.Web.Lists.Take(2).Skip(2).QueryProperties(p => p.Title).ToListAsync();    
}
```

In the above sample only the `Title` property of the lists is requested, if you do not provide the expression then the list default properties are loaded.

In the following code excerpt you can see a complete logic to manually page items in a collection using `Take`/`Skip` extension methods.

```csharp
int pageCount = 0;
int pageSize = 10;

while (true)
{
    var page = context.Web.Lists.Skip(pageSize * pageCount).Take(pageSize).ToArray();
    
    // Use the current page ...

    pageCount++;
    if (page.Length < pageSize)
    {
        break;
    }
}
```

Notice that the above sample relies on you for requesting the pages. As such, unless you really need to manually control paging, you should avoid this approach and rather use the continous paging with asynchronous code.

### Starting via the Take() LINQ method with a filter and complex data load expression

This example builds on top of the previous but shows some additional capabilities:

- A complex expression is used to define which List properties to load
- A filter expression is specified so that only a subset of lists are returned
- The returned, paged, data is made available via the method call output

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Get a first page of lists of size 2 
    //   filtered to generic lists
    //     returning list properties
    //     - Title and TemplateType
    //     - ContentTypes
    //          - For each content type the FieldLinks loaded
    //               - For each FieldLink the Name property
    var lists = await context.Web.Lists.Take(2)
                                        .Where(p => p.TemplateType == ListTemplateType.GenericList)
                                        .QueryProperties(p => p.Title, p => p.TemplateType,
                                                        p => p.ContentTypes.QueryProperties(
                                                        p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                        .ToListAsync();
}
```
