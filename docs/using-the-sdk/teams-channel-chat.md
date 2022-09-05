# Working with a Team Channel Chat messages

The Core SDK provides support for working with chat messages within a Teams Channel allowing you to post messages.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Chat Messages

The following example will show you how to retrieve all the messages within a channel chat:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel               
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

```

## Adding Chat Messages

You can post messages to the chat within a channel, the following code demonstrates how this can be done:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = "Hello, I'm posting a message - PnP Rocks!";

// Perform the add operation
await chatMessages.AddAsync(body);

```

## Add Chat Messages with HTML

You can add chat messages that contain a HTML body, the following code sample will demonstrate how to do this:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = $"<h1>Hello</h1><br />Example posting a HTML message - <strong>PnP Rocks!</strong>";

// Perform the add operation
await chatMessages.AddAsync(body, ChatMessageContentType.Html);
                
```

## Adding Chat Messages with Attachments

Chat messages can support file attachments.

The following code shows an example of how an attachment is done:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

// Upload File to SharePoint Library
IFolder folder = await context.Web.Lists.GetByTitle("Documents").RootFolder.GetAsync();
IFile existingFile = await folder.Files.GetFirstOrDefaultAsync(o => o.Name == "test_added.docx");
if(existingFile == default)
{
    existingFile = await folder.Files.AddAsync("test_added.docx", System.IO.File.OpenRead($"<path-to-file>test.docx"));
}

// Prepare the attachment ID
var attachmentId = existingFile.ETag.AsGraphEtag(); ; // Needs to be the documents eTag - just the GUID part - use this extension method

var body = $"<h1>Hello</h1><br />Example posting a message with a file attachment - <attachment id=\"{attachmentId}\"></attachment>";

var fileUri = new Uri(existingFile.LinkingUrl);

await chatMessages.AddAsync(new ChatMessageOptions{
    Content = body,
    ContentType = ChatMessageContentType.Html,
    Attachments = {
        new ChatMessageAttachmentOptions
        {
            Id = attachmentId,
            ContentType = "reference",
            // Cannot have the extension with a query graph doesn't recognise and think its part of file extension - include in docs.
            ContentUrl = new Uri(fileUri.ToString().Replace(fileUri.Query, "")),
            Name = $"{existingFile.Name}",
            ThumbnailUrl = null,
            Content = null
        }
    }
});
```

For advanced information about the specific area of the Graph that handles sending messages with attachments visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments)

> [!Note]
> There two areas you should be aware about this feature related to how the Graph works:
> - The Graph produces a eTag reference in the format "{GUID},ID" with quotes, however this has to be stripped down to just the GUID element, without the surrounding braces, comma, quotes and ID for this to be recognised. An extension method has been created to avoid having to write this adaption to the eTag.
> - File used to upload, must not have a query string parameter, this will not be recognised and the Graph treats this as part of the extension and thus will fail if the Name extension is different.

## Adding Chat Messages with Cards

Adding a chat message can be done using a Card, see below adaptive card example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

// Attachment ID must be unique, but the same in both body and content properties
var attachmentId = "74d20c7f34aa4a7fb74e2b30004247c5";
var body = $"<attachment id=\"{attachmentId}\"></attachment>";

await chatMessages.AddAsync(new ChatMessageOptions
{
    Content = body,
    ContentType = ChatMessageContentType.Html,
    Attachments = {
        new ChatMessageAttachmentOptions
        {
            Id = attachmentId,
            ContentType = "application/vnd.microsoft.card.adaptive",
            // Adaptive Card
            Content = "{\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"type\":\"AdaptiveCard\",\"version\":\"1.0\",\"body\":[{\"type\":\"Container\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Adaptive Card Unit Test\",\"weight\":\"bolder\",\"size\":\"medium\"},{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"Image\",\"url\":\"https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg\",\"size\":\"small\",\"style\":\"person\"}]},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Matt Hidinger\",\"weight\":\"bolder\",\"wrap\":true},{\"type\":\"TextBlock\",\"spacing\":\"none\",\"text\":\"Created {{DATE(2017-02-14T06:08:39Z,SHORT)}}\",\"isSubtle\":true,\"wrap\":true}]}]}]},{\"type\":\"Container\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Now that we have defined the main rule sand features of the format ,we need to produce a schema and publish it to GitHub.The schema will be the starting point of our reference documentation.\",\"wrap\":true},{\"type\":\"FactSet\",\"facts\":[{\"title\":\"Board:\",\"value\":\"Adaptive Card\"},{\"title\":\"List:\",\"value\":\"Backlog\"},{\"title\":\"Assigned to:\",\"value\":\"Matt Hidinger\"},{\"title\":\"Duedate:\",\"value\":\"Not set\"}]}]}],\"actions\":[{\"type\":\"Action.ShowCard\",\"title\":\"Set due date\",\"card\":{\"type\":\"AdaptiveCard\",\"body\":[{\"type\":\"Input.Date\",\"id\":\"dueDate\"}],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"OK\"}]}},{\"type\":\"Action.ShowCard\",\"title\":\"Comment\",\"card\":{\"type\":\"AdaptiveCard\",\"body\":[{\"type\":\"Input.Text\",\"id\":\"comment\",\"isMultiline\":true,\"placeholder\":\"Enter your comment\"}],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"OK\"}]}}]}",
            ContentUrl = null,
            Name = null,
            ThumbnailUrl = null
        }
    }
});

```

For advanced information about the specific area of the Graph that handles sending messages with cards visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-3-cards](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-3-cards)

Additionally, there are different types of cards, such as Adaptive Cards and Thumbnail - these have been tested with unit tests but not all types have yet.
For information about the different types of cards, visit: [https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference)

## Adding Chat Messages with Inline Images

Chat messages can also include inline images. The following example demonstrates this option:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = $"<div><div><h1>Hello</h1><p>Example posting a message with inline image</p><div><span><img height=\"392\" src=\"../hostedContents/1/$value\" width=\"300\" style=\"vertical-align:bottom; width:300px; height:392px\"></span></div></div></div>";
                                
await chatMessages.AddAsync(new ChatMessageOptions
{
    Content = body,
    ContentType = ChatMessageContentType.Html,
    HostedContents =
    {
        new ChatMessageHostedContentOptions
        {
            Id = "1",
            ContentBytes = "<base64-encoded bytes>",
            ContentType = "image/png"
        }
    }
});
```

For advanced information about the specific area of the Graph that handles sending messages with inline images visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-5-sending-inline-images-along-with-the-message](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-5-sending-inline-images-along-with-the-message)

## Adding chat messages with mentions

We can use mentions when creating a chat message. We can use the following options when mentioning:

- User
- Conversation (e.g. a team, a channel, ...)
- Team tag

In this example, a team message will be posted which will tag a channel and a user:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = $"Hello, PnP Rocks! <br/>This is a channel and user mention test <br/>Mention 1: <at id=\"0\">Channel</at><br/>Mention 2: <at id=\"1\">User</a>";

await chatMessages.AddAsync(new ChatMessageOptions
{
    Content = body,
    ContentType = ChatMessageContentType.Html,
    Mentions =
    {
        new ChatMessageMentionOptions
        {
            Id = 0,
            MentionText = "Channel",
            Mentioned = new TeamChatMessageMentionedIdentitySet
            {
                Conversation = new TeamConversationIdentity
                {
                    ConversationIdentityType = TeamConversationIdentityType.Channel,
                    Id = channel.Id
                }
            }
        },
        new ChatMessageMentionOptions
        {
            Id = 1,
            MentionText = "User",
            Mentioned = new TeamChatMessageMentionedIdentitySet
            {
                User = new Identity
                {
                    DisplayName = userToMention.Title,
                    Id = graphUser.Id,
                    UserIdentityType = TeamUserIdentityType.aadUser
                }
            }
        }
    }
});

```

In this example, a message will be created which will tag a team and a team tag. For more information regarding the usage of team tags in the PnP Core SDK, please refer to {insert link here}

```csharp

// Get the Team
var team = await context.Team.GetAsync(o => o.Channels, o => o.Tags);

// Get the channel
var channel = team.Channels.AsRequested().FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = $"Hello, PnP Rocks! <br/>This is a team and tag mention test <br/>Mention 1: <at id=\"0\">Team</at><br/>Mention 2: <at id=\"1\">Tag</at>";

await chatMessages.AddAsync(new ChatMessageOptions
{
    Content = body,
    ContentType = ChatMessageContentType.Html,
    Mentions =
    {
        new ChatMessageMentionOptions
        {
            Id = 0,
            MentionText = "Team",
            Mentioned = new TeamChatMessageMentionedIdentitySet
            {
                Conversation = new TeamConversationIdentity
                {
                    ConversationIdentityType = TeamConversationIdentityType.Team,
                    Id = team.Id.ToString()
                }
            }
        },
        new ChatMessageMentionOptions
        {
            Id = 1,
            MentionText = "Tag",
            Mentioned = new TeamChatMessageMentionedIdentitySet
            {
                Tag = new TeamTagIdentity
                {
                    DisplayName = team.Tag.First().DisplayName,
                    Id = team.Tags.First().Id,
                }
            }
        }
    }
});

```

For advanced information about the specific area of the Graph that handles sending messages with mentions, visit: [https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#examples](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#examples)
