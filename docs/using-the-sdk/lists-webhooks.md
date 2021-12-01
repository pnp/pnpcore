# Working with list webhooks

PnP Core SDK allows you to perform all needed CRUD operations with list webhooks in a convenient fluent manner. The webhook instance is represented via the [IListSubscription](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListSubscription.html) interface.

## Fetch and filter webhooks

To get all list webhooks, you can use regular `Load` methods.

Load webhooks as part of the list request:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List", l => l.Webhooks);
```

Or load webhooks explicitly:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");
list.Webhooks.Load();
```

Or load using list instance:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");
list.Load(l => l.Webhooks);
```

Later on you can iterate over the webhooks collection:

```csharp
foreach (var webhook in list.Webhooks.AsRequested())
{
    // do something
}
```

You can also use LINQ to filter webhooks by their properties. For example, to get all webhooks, where the client state contains specific string:

```csharp
var webhooks = await list.Webhooks.Where(w => w.ClientState.Contains("state")).ToListAsync();

foreach (var webhook in webhooks)
{
    // do something
}
```

## Get webhook by Id

To get a webhook by its Id, you can use a dedicated method:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");

// get by id
var webhook = await list.Webhooks.GetByIdAsync(new Guid("<webhook id>"));
```

Or you can use LINQ filter:

```csharp
var webhookId = new Guid("<id>");
var webhook = await list.Webhooks.FirstOrDefaultAsync(w => w.Id == webhookId);
```

## Add a webhook

To add a webhook, you should provide notification url, expiration date and optionally client state. You can use client state for validating notifications, tagging different subscriptions, or other reasons.

There are a few different method overloads available in PnP Core SDK to add a new webhook:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");

// creates a new webhook subscription with validity period of 180 days (maximum possible), doesn't set client state
var webhook = await list.Webhooks.AddAsync("https://my-handler.url");

// creates a new webhook subscription with validity period of 1 month, doesn't set client state
var webhook = await list.Webhooks.AddAsync("https://my-handler.url", 1);

// creates a new webhook subscription with validity period of 1 month and sets client state.
var webhook = await list.Webhooks.AddAsync("https://my-handler.url", DateTime.UtcNow.AddMonths(1), "tag:client");
```

> [!Note]
>
> The maximum expiration time for SharePoint list webhooks is 180 days.

## Update a webhook

To update a webhook, you should set properties you want to change and then call the `Update` method:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");
var webhook = await list.Webhooks.GetByIdAsync(new Guid("<webhook id>"));

// change expiration for the webhook
webhook.ExpirationDateTime = DateTime.UtcNow.AddDays(180);

// update it
await webhook.UpdateAsync();
```

## Delete a webhook

To delete a webhook just call the `Delete` method on the webhook instance:

```csharp
var list = await context.Web.Lists.GetByTitleAsync("My List");
var webhook = await list.Webhooks.GetByIdAsync(new Guid("<guid>"));

// delete it
await webhook.DeleteAsync();
```
