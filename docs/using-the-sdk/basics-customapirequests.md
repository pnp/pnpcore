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
