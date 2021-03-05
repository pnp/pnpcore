# Using paging

Being able to retrieve data in a paged manner is important when you want to use the first data rows while you're loading still additional data, but also when you're loading large data sets. When you page data you can start from from a LINQ query that uses the `Take()` method and from that point on PnP Core SDK will handle paging for you. Sometimes a query if paged by default (e.g. loading Teams channel messages) and then the paging will happen automatically behind the scenes.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for getting paged data
}
```

## Starting via the Take() LINQ method

This example shows how to use paging to load lists, in the sample only the `Title` property of the list is requested, if you do not provide the expression then list default properties are loaded.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Get a first page of lists of size 2
    var lists = await context.Web.Lists.Take(2).QueryProperties(p => p.Title).ToListAsync();

    // Get the next page
    lists = await context.Web.Lists.Take(2).Skip(2).QueryProperties(p => p.Title).ToListAsync();    
}
```

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
    //     - ContentTypes with for each content type the FieldLinks loaded
    var lists = await context.Web.Lists.Take(2)
                                        .Where(p => p.TemplateType == ListTemplateType.GenericList)
                                        .QueryProperties(p => p.Title, p => p.TemplateType,
                                                        p => p.ContentTypes.QueryProperties(
                                                        p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
                                        .ToListAsync();
}
```

### Implicit paging

In this example the messages in a Team channel are queried. When doing so the underlying Graph API call will return a "next page link" automatically and if so this will be used to get the next messages.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Retrieve the already created channel
    var channelForPaging2 = context.Team.Channels.FirstOrDefault(p => p.DisplayName == "My Channel");

    // Option A: Load the messages, this will load all messages via paged requests
    await channelForPaging2.LoadAsync(p => p.Messages);

    // Option B: 
    await foreach(var message in channelForPaging2.Messages)
    {
        // do something with the message
    }

    //or

    await foreach(var list in context.Web.Lists)
    {
        // do something with the list
    }
}
```
