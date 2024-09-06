# Getting the response headers for Microsoft Graph and SharePoint API requests

When PnP Core SDK makes requests is either uses Microsoft Graph, SharePoint REST or SharePoint CSOM. In most cases things are fine, but you might need to get insights in the key response headers for the made requests. For example you want to log the SharePoint correlation id or Microsoft Graph request id because that's needed for support/debug purposes.

## Generic way of getting response headers

The easiest way to get response headers, for both batch and interactive requests, is by using the `WithResponseHeaders` extension method. If there's a `SPRequestGuid` header than the request was fulfilled via a SharePoint REST/CSOM call, if there's a `client-id` header a Microsoft Graph request was used.

```csharp

// Interactive request capturing the 
await myList.WithResponseHeaders((responseHeaders) => { 
    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"])); 
}).DeleteAsync();

// Batch request
var batch = context.NewBatch();
// Shows how to get the response headers from the batch for the SharePoint REST and CSOM requests
await myList.WithResponseHeaders((responseHeaders) => {
    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["SPRequestGuid"]));
}).DeleteBatchAsync(batch);
await context.ExecuteAsync(batch);

// Sample of a possible Microsoft Graph request
var web = await context.Web.WithResponseHeaders((responseHeaders) => {
    Assert.IsTrue(!string.IsNullOrEmpty(responseHeaders["request-id"]));
}).GetAsync(p => p.Description);

```

