# Working with a Team Channel Chat messages

The Core SDK provides support for working with chat messages within a Teams Channel allowing you to post messages.

> [!NOTE]
> Currently, this is a limited implementation that only supports adding text messages, and message posting is currently limited to plain text formats.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Chat Messages

The following example will show you how to retrieve all the messages within a channel chat:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel               
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

```

## Adding Chat Messages

You can post messages to the chat within a channel, the following code demonstrates how this can be done:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = "Hello, I'm posting a message - PnP Rocks!";

// Perform the add operation
await chatMessages.AddAsync(body);

```
