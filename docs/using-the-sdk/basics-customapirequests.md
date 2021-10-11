# Making custom API calls

PnP Core SDK uses a strongly typed model allowing you to interact with SharePoint and Microsoft Teams using either SharePoint REST, Microsoft Graph or SharePoint CSOM requests. If the provided functionality is not meeting your needs you do have the option to execute SPO REST or Microsoft Graph requests yourselves, as explained in this article.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext
}
```

## General concepts

To perform a custom API request you use the `ExecuteRequestAsync` or `ExecuteRequest` methods providing as input a `ApiRequest` holding the information for making the API call. You can use these methods on any domain model object (e.g. `Web`, `List`, `Team`) and this will not impact the call. However calling the `ExecuteRequestAsync` or `ExecuteRequest` methods on the correct domain model object enables you to use the tokens in your API call. Following tokens can be used:

Token | Description
------|------------
`{Id}` | Value of the SharePoint Id property of the current model instance (e.g. List).
`{Parent.Id}` | Value of the SharePoint Id property of the current model's parent instance (e.g. Web for a List --> ListCollection is skipped in this approach).
`{GraphId}` | Value of the Microsoft Graph Id property of the current model instance (e.g. TeamChannel).
`{Parent.GraphId}` | Value of the Microsoft Graph Id property of the current model's parent instance (e.g. Team for a TeamChannel --> TeamChannelCollection is skipped in this approach).
`{Site.GroupId}` | Id value of the Microsoft 365 Group connected to the Site loaded in the current PnPContext (Id is the same for SharePoint REST as Microsoft Graph usage).
`{Site.Id}` | SharePoint Id value of the Site loaded in the current PnPContext.
`{Web.Id}` | SharePoint Id value of the Web loaded in the current PnPContext.
`{Web.GraphId}` | Microsoft Graph Id value of the Web loaded in the current PnPContext.
`{List.Id}` | SharePoint Id value of the List loaded in the current PnPContext (works only when the target object is of type List or ListItem).
`{hostname}` | Host name of the current site (so for https://contoso.sharepoint.com/sites/team1 this is contoso.sharepoint.com)
`{serverrelativepath}` | Server relative path of the current site (so for https://contoso.sharepoint.com/sites/team1 this is /sites/team1)

The returned response is of type `ApiRequestResponse` and contains the JSON result, the HTTP result code, the original `ApiRequest` instance and optional extra HTTP headers returned by the call.

## Making a custom SPO REST request

Below sample shows how you can perform a custom REST request on the current web and on a web in another site collection:

```csharp
// Custom SPO REST request for the current connected web
// Will result in a GET https://contoso.sharepoint.com/sites/currentsite/_api/web
var apiRequest = new ApiRequest(ApiRequestType.SPORest, "_api/web");
var response = context.Web.ExecuteRequest(apiRequest);

// Parse the json response returned via response.Response

// Custom SPO REST Request for another web
// Will result in a GET https://contoso.sharepoint.com/sites/anothersite/_api/web
var apiRequest = new ApiRequest(ApiRequestType.SPORest, "https://contoso.sharepoint.com/sites/anothersite/_api/web");
var response = await context.Web.ExecuteRequestAsync(apiRequest);
```

## Making a custom Microsoft Graph request

Below sample shows how to make a custom Microsoft Graph request:

```csharp
// Custom Microsoft Graph request
// Will result in a GET "https://graph.microsoft.com/v1.0/me" 
var apiRequest = new ApiRequest(ApiRequestType.Graph, "me");
var response = context.Team.ExecuteRequest(apiRequest);

// Parse the json response returned via response.Response
```

## Batching custom API requests

To optimize performance it's recommended to limit the amount of server roundtrips and therefore batching custom API requests can be used. Batching requests is quite similar to interactive requests, you simply use one of the available `ExecuteRequestBatch` methods. Since batching requests implies that you'll only get a result once the batch is executed you do not get back a JSON string, instead you get an `IBatchSingleResult<BatchResultValue<string>>` which allows you to check if the batch was executed and if so get the batch result via the `Value` property.

```csharp
// Create a new batch
var batch = context.NewBatch();

// Add requests to the batch
var meResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "me"));
// Note: meResponse.IsAvailable is false
var drivesResponse = context.Web.ExecuteRequestBatch(batch, new ApiRequest(ApiRequestType.Graph, "drives"));
// Note: drivesResponse.IsAvailable is false

// Execute the batch
await context.ExecuteAsync(batch);

// Use the batch results
if (meResponse.IsAvailable)
{
    string meJsonResonse = meResponse.Result.Value;
}

if (drivesResponse.IsAvailable)
{
    string drivesJsonResponse = drivesResponse.Result.Value;
}
```

Batching can be used with an dedicated batch like shown in above example, but it's also possible to use the implicit batch which is always available, in that case you'd just leave out the `batch` parameter in the `ExecuteRequestBatch` methods:

```csharp
// Add requests to the batch
var meResponse = await context.Web.ExecuteRequestBatchAsync(new ApiRequest(ApiRequestType.Graph, "me"));
var drivesResponse = await context.Web.ExecuteRequestBatchAsync(new ApiRequest(ApiRequestType.Graph, "drives"));

// Execute the batch
await context.ExecuteAsync();

// Use the batch results
if (meResponse.IsAvailable)
{
    string meJsonResonse = meResponse.Result.Value;
}

if (drivesResponse.IsAvailable)
{
    string drivesJsonResponse = drivesResponse.Result.Value;
}
```

## Handling failing requests in a batch

What if one of the requests in a batch fails? The default behavior is that a `SharePointRestServiceException` or `MicrosoftGraphServiceException` exception is thrown when the first failed request is processed, but you can also choose to get back a list of failed batch requests and then handle the follow-up in your code. To do this you need to tell the `Execute` methods to not throw an exception and collect the output of the `Execute` method. Following snippet shows how to do this:

```csharp
// Create a new batch
var batch = context.NewBatch();

// Add requests to the batch
var meResponse = await context.Web.ExecuteRequestBatchAsync(batch, new ApiRequest(ApiRequestType.Graph, "me"));
var drivesResponse = await context.Web.ExecuteRequestBatchAsync(batch, new ApiRequest(ApiRequestType.Graph, "thiswillgiveanerror"));

// Execute the batch, notice we tell here to not throw an exception and we collect the possible errors in a collection
var batchErrors = await context.ExecuteAsync(batch, false);

// Use the batch results
if (batchErrors.Any())
{
    // there were errors
}
else
{
    // all good
}
```

## Can I mix custom API requests with out-the-box API requests in a single batch?

Yes, this is perfectly possible, below example combines a custom API with an API request that will load a set of lists.

```csharp
// Create a new batch
var batch = context.NewBatch();

// Add requests to the batch
var result1 = await context.Web.ExecuteRequestBatchAsync(batch, new ApiRequest(ApiRequestType.Graph, "_api/web"));
var result2 = await context.Web.Lists
    .Where(p => p.TemplateType == ListTemplateType.GenericList)
    .QueryProperties(
        p => p.Title, p => p.TemplateType,
        p => p.ContentTypes.QueryProperties(
            p => p.Name, p => p.FieldLinks.QueryProperties(p => p.Name)))
    .AsBatchAsync(batch);

// Execute the batch
await context.ExecuteAsync(batch);
```

## I want to specify custom request headers or read the response headers

To specify request headers you can use the `WithHeaders` extension method from the `PnP.Core.Model` namespace: using a `Dictionary<string,string>` you provide input headers and using a delegate your code get notified of the resulting response headers. Below are some sample:

```csharp
Dictionary<string, string> extraHeaders = new Dictionary<string, string>() { { "myheader", "myheadervalue" } };

// Use WithHeaders on custom API request, process the returned response headers
var meResponse = await context.Web.WithHeaders(extraHeaders, (responseHeaders) => { /* process the response headers */ }).ExecuteRequestBatchAsync(batch, new ApiRequest(ApiRequestType.Graph, "me"));

// Use WithHeaders on OOB API request, ignore the response headers
context.Web.WithHeaders(extraHeaders).Load(p => p.All);
```
