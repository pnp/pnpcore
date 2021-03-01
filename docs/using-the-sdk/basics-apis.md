# APIs used by the PnP Core SDK

The PnP Core SDK is an SDK that provides an abstraction over the actual API calls being made to Microsoft 365. When looking at the type of API calls the SDK uses there are 4 possible API endpoints being used:

- **Microsoft Graph production endpoint (v1)**: for all queries that require Graph and are available on the v1 endpoint.
- **Microsoft Graph beta endpoint (beta)**: for all queries that require Graph and are currently not available on the v1 endpoint.
- **SharePoint REST endpoint**: for all SharePoint specific queries, note that by default the SDK favors uses Microsoft Graph for some of the SharePoint needs (see the **SharePoint REST versus Microsoft Graph** chapter to change that)
- **SharePoint client.svc endpoint (CSOM)**: only called in a few cases when there's no other suitable API that can fulfill the needs

Although the SDK can use these 4 API endpoints, for you as a developer there's no difference in how you write your code, but there might be cases where you want to influence the SDK's API selection behavior as explained in the next chapter.

## Influencing the SDK API selection

### SharePoint REST versus Microsoft Graph

The PnP Core SDK by default is configured to favor the Microsoft Graph API when you're reading SharePoint data assuming the requested properties are available via Graph. Let's give a simple example showing two similar PnP Core SDK method calls which under the covers result in different APIs being called.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Uses a Graph API call
    await context.Web.LoadAsync(p => p.Title);

    // Uses a SharePoint REST API call as the NoCrawl web property is not available in Graph
    await context.Web.LoadAsync(p => p.NoCrawl);
}
```

If you prefer a more consistent SharePoint experience you can configure the SDK to use SharePoint REST for everything that can be realized using SharePoint REST. You do this by setting the [GraphFirst property](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#collapsible-PnP_Core_Services_PnPContext_GraphFirst) on the [PnPContext](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html) to false:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // turn off the graph first behavior
    context.GraphFirst = false;

    // Uses a Graph API call
    await context.Web.LoadAsync(p => p.Title);

    // Uses a SharePoint REST API call as the NoCrawl web property is not available in Graph
    await context.Web.LoadAsync(p => p.NoCrawl);
}
```

Above code snippet showed how to turn off GraphFirst for a single PnPContext, but you can also turn off GraphFirst for all contexts being created via the configuration system, see [Configuring the PnP Core SDK](basics-settings.md) for more information.

### Graph V1 versus Graph Beta

In the PnP Core SDK we limit the usage of Microsoft Graph Beta endpoint, but in some cases we do have to use the Graph Beta endpoint. By default using Graph Beta is turned on, but if you don't want to use Graph Beta you can tell the SDK to not use it via the [GraphCanUseBeta](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#collapsible-PnP_Core_Services_PnPContext_GraphCanUseBeta) property. This obviously results in certain SDK features not working anymore. Depending on the solution you're building using the Graph beta endpoint might be perfect and you want to standardize on always using Graph Beta, which is an option as well via the [GraphAlwaysUsesBeta](https://pnp.github.io/pnpcore/api/PnP.Core.Services.PnPContext.html#collapsible-PnP_Core_Services_PnPContext_GraphAlwaysUseBeta) property.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // turn off the option to use Graph Beta
    context.GraphCanUseBeta = false;

    // Configure to use Graph Beta for all Graph requests
    context.GraphAlwaysUsesBeta = true;
}
```

Above code snippet showed how to configure the Microsoft Graph behavior for a single PnPContext, but you can also configure this behavior for all contexts being created using the configuration system, see [Configuring the PnP Core SDK](basics-settings.md) for more information.
