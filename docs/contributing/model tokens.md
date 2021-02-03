
# Model tokens

## Introduction

When decorating model classes and properties via attributes, often you need to define an API request. To ensure that these API requests offer the needed flexibility, you can use tokens in the URL request definition. Tokens are embedded between curly brackets. Below snippet shows some samples in which tokens are used:

```csharp
// Site.GroupId token to grab the id of the Microsoft 365 group connected to the current site
[GraphType(Uri = "teams/{Site.GroupId}")]
[GraphProperty("installedApps", Get = "teams/{Site.GroupId}/installedapps?expand=TeamsApp")]

// Parent.GraphId to get Microsoft Graph Id value of the parent class instance of the model
// GraphId to get the If value of the model class instance
[GraphType(Uri = "teams/{Parent.GraphId}/channels/{GraphId}")]

// Id property to get the SharePoint REST Id value of the model class instance
[SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Get = "_api/web/lists", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
[GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}")]

// Web.GraphId to get the Microsoft Graph Id value of the SharePoint Web model instance of the current PnPContext instance
[GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
```

## Model tokens that can be used

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
