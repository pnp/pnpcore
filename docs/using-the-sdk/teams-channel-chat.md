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

## Adding Chat Messages with Attachments

Chat messages can support file attachments.

The following code shows an example of how an attachment is done:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

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
var attachmentId = existingFile.ETag.Replace("{","").Replace("}","").Replace("\"","").Split(',').First(); // Needs to be the documents eTag - just the GUID part

var body = $"<h1>Hello</h1><br />Example posting a message with a file attachment - <attachment id=\"{attachmentId}\"></attachment>";

var fileUri = new Uri(existingFile.LinkingUrl);

ITeamChatMessageAttachmentCollection coll = new TeamChatMessageAttachmentCollection
{
    new TeamChatMessageAttachment
    {
        Id = attachmentId,
        ContentType = "reference",
        // Cannot have the extension with a query graph doesn't recognise and think its part of file extension - include in docs.
        ContentUrl = new Uri(fileUri.ToString().Replace(fileUri.Query, "")),
        Name = existingFile.Name,
        ThumbnailUrl = null,
        Content = null
    }
};

// Add Message to the Chat
await chatMessages.AddAsync(body, ChatMessageContentType.Html, coll);
```

For advanced information about the specific area of the Graph that handles sending messages with attachments visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments)

> [!Note]
> There two areas you should be aware about this feature related to how the Graph works:
> - The Graph produces a eTag reference in the format "{GUID},ID" with quotes, however this has to be stripped down to just the GUID element, without the surrounding braces, comma, quotes and ID for this to be recognised.
> - File used to upload, must not have a query string parameter, this will not be recognised and the Graph treats this as part of the extension and thus will fail if the Name extension is different.

## Adding Chat Messages with Cards

Adding a chat message can be done using a Card, see below adaptive card example:

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

// Attachment ID must be unique, but the same in both body and content properties
var attachmentId = "74d20c7f34aa4a7fb74e2b30004247c5";
var body = $"<attachment id=\"{attachmentId}\"></attachment>";

ITeamChatMessageAttachmentCollection coll = new TeamChatMessageAttachmentCollection
{
    new TeamChatMessageAttachment
    {
        Id = attachmentId,
        ContentType = "application/vnd.microsoft.card.adaptive",
        // Adaptive Card
        Content = "{\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"type\":\"AdaptiveCard\",\"version\":\"1.0\",\"body\":[{\"type\":\"Container\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Adaptive Card Example\",\"weight\":\"bolder\",\"size\":\"medium\"},{\"type\":\"ColumnSet\",\"columns\":[{\"type\":\"Column\",\"width\":\"auto\",\"items\":[{\"type\":\"Image\",\"url\":\"https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg\",\"size\":\"small\",\"style\":\"person\"}]},{\"type\":\"Column\",\"width\":\"stretch\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Matt Hidinger\",\"weight\":\"bolder\",\"wrap\":true},{\"type\":\"TextBlock\",\"spacing\":\"none\",\"text\":\"Created {{DATE(2017-02-14T06:08:39Z,SHORT)}}\",\"isSubtle\":true,\"wrap\":true}]}]}]},{\"type\":\"Container\",\"items\":[{\"type\":\"TextBlock\",\"text\":\"Now that we have defined the main rule sand features of the format ,we need to produce a schema and publish it to GitHub.The schema will be the starting point of our reference documentation.\",\"wrap\":true},{\"type\":\"FactSet\",\"facts\":[{\"title\":\"Board:\",\"value\":\"Adaptive Card\"},{\"title\":\"List:\",\"value\":\"Backlog\"},{\"title\":\"Assigned to:\",\"value\":\"Matt Hidinger\"},{\"title\":\"Duedate:\",\"value\":\"Not set\"}]}]}],\"actions\":[{\"type\":\"Action.ShowCard\",\"title\":\"Set due date\",\"card\":{\"type\":\"AdaptiveCard\",\"body\":[{\"type\":\"Input.Date\",\"id\":\"dueDate\"}],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"OK\"}]}},{\"type\":\"Action.ShowCard\",\"title\":\"Comment\",\"card\":{\"type\":\"AdaptiveCard\",\"body\":[{\"type\":\"Input.Text\",\"id\":\"comment\",\"isMultiline\":true,\"placeholder\":\"Enter your comment\"}],\"actions\":[{\"type\":\"Action.Submit\",\"title\":\"OK\"}]}}]}",
        ContentUrl = null,
        Name = null,
        ThumbnailUrl = null
    }
};

await chatMessages.AddAsync(body, ChatMessageContentType.Html, coll);

```

For advanced information about the specific area of the Graph that handles sending messages with cards visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-3-cards](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-3-cards)

Additionally, there are different types of cards, such as Adaptive Cards and Thumbnail - these have been tested with unit tests but not all types have yet.
For information about the different types of cards, visit: [https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference](https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference)


## Adding Chat Messages with Inline Images

```csharp
// Get the Team
var team = await context.Team.GetAsync(o => o.Channels);

// Get the channel
var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General");

channel = await channel.GetAsync(o => o.Messages);
var chatMessages = channel.Messages;

var body = $"<div><div><h1>Hello</h1><p>Example posting a message with inline image</p><div><span><img height=\"392\" src=\"../hostedContents/1/$value\" width=\"300\" style=\"vertical-align:bottom; width:300px; height:392px\"></span></div></div></div>";
                                
ITeamChatMessageHostedContentCollection coll = new TeamChatMessageHostedContentCollection
{
    new TeamChatMessageHostedContent
    {
        Id = "1",
        ContentBytes = "<base64-encoded bytes>",
        ContentType = "image/png"
    }
};

await chatMessages.AddAsync(body, ChatMessageContentType.Html, hostedContents: coll);
```
For advanced information about the specific area of the Graph that handles sending messages with inline images visit:
[https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-5-sending-inline-images-along-with-the-message](https://docs.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-5-sending-inline-images-along-with-the-message)