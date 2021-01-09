# Working with Teams: Introduction

The PnP Core SDK provides support for working with Microsoft Teams, in this set of articles, you will learn to use the SDK to interact with the service to work with Teams, Channels, Tabs, Chat Messages and more.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Loading existing Team

When you want to get a Team, the SDK uses the associated Office 356 group ID attached to the current SharePoint Site to retrieve the Team from the Graph. Getting a Team can be achieved with the following:

```csharp
 var team = await context.Team.GetAsync();
```


## Loading existing Team with properties