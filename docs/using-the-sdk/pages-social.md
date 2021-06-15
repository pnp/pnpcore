# Using the "social" features of a page

A SharePoint Page can have comments, replies and likes. This chapter explains how to enable or disable page commenting and how to work with comments, replies and likes.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with pages
}
```

## Disabling/Enabling page comments

By default commenting is enabled on article pages and for the majority of use cases this default is fine. You can however also turn off commenting by calling the [DisableCommentsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_DisableCommentsAsync). Getting the current commenting status can be done using the [AreCommentsDisabledAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#PnP_Core_Model_SharePoint_IPage_AreCommentsDisabledAsync) and turning on commenting again is done via the [EnableCommentsAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IPage.html#collapsible-PnP_Core_Model_SharePoint_IPage_EnableCommentsAsync).

> [!Note]
> A page needs to be saved before you can configure it's commenting settings.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Are comments disabled?
var commentsDisabled = await newPage.AreCommentsDisabledAsync();

// disable comments
await page.DisableCommentsAsync();

// enabled comments again
await page.EnableCommentsAsync();
```

## Liking/unliking a page

The currently authenticated user can like/unlike a page using one of the `Like` or `Unlike` methods on the page.

> [!Note]
> A page needs to be published before it can be liked.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Publish the page, required before it can be liked
await newPage.PublishAsync();

// Like the page
await newPage.LikeAsync();

// Unlike the page
await newPage.UnlikeAsync();
```

## Enumerating page likes

If you want to understand who liked a page you can load the `LikedByInformation` of page which tell you if the page like count, whether the current user liked the page and which users liked the page:

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Publish the page, required before it can be liked
await newPage.PublishAsync();

// Like the page
await newPage.LikeAsync();

// Get a list of users who liked this page
var pageLikeInformation = await newPage.GetLikedByInformationAsync();

// Was page liked by the current user?
bool pageLikedByCurrentUser = pageLikeInformation.IsLikedByUser;

// Enumerate the persons that liked this page
foreach(var likedByUser in pageLikeInformation.LikedBy.AsRequested())
{
    // do something with the information about the user who liked this page
}
```

## Getting comments

To work with page comments you first need to get a reference to the page comments collection. Once you've done that, you can enumerate the existing comments, add new comments or delete comments. Below code snippet shows how the comments can be enumerated.

```csharp
// Create the page
var page = await context.Web.NewPageAsync();

// Save the page
await page.SaveAsync("PageA.aspx");

// Publish the page, required before it can be commented
await newPage.PublishAsync();

// Get the comments for this page
var comments = await newPage.GetCommentsAsync();

// Loop over the comments
foreach(var comment in comments.AsRequested())
{
    // Do something with the comment
}
```

## Adding a comment

Once you've load a comments collection via one of the `GetComments` methods you can add new comments or delete comments.

```csharp
// Get the comments for this page
var comments = await newPage.GetCommentsAsync();

// Add a comment
var comment = await comments.AddAsync("this is great");
```

## Adding a reply to a comment

Comments can also have replies and since the collection of replies is similar to the collections of comments to code to add a reply is the same:

```csharp
// Get the comments for this page
var comments = await newPage.GetCommentsAsync();

// Add a comment
var comment = await comments.AddAsync("this is great");

// Add a reply to the comment
var reply = await comment.Replies.AddAsync("yes this is great!");
```

## Removing a comment or reply

Once you've load a comments collection via one of the `GetComments` methods you can delete comments and/or their replies using one of the `Delete` methods.

```csharp
// Load the comments with replies 
comments = await newPage.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies);

// Get first comment
var firstComment = comments.AsRequested().First();

// Get first reply on the first comment
var firstCommentReply = firstComment.Replies.AsRequested().First();

// Delete the reply
await firstCommentReply.DeleteAsync();

// Delete the comment
await firstComment.DeleteAsync();
```

## Liking\Unliking comments and replies

A comment or reply can be liked by the authenticated user, this is done using one of the `Like` or `Unlike` methods on either the comment or reply.

```csharp
// Load the comments with replies 
comments = await newPage.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies);

// Get first comment
var firstComment = comments.AsRequested().First();

// Like the comment
await firstComment.LikeAsync();

// Unlike the comment
await firstComment.UnlikeAsync();

// Get first reply on the first comment
var firstCommentReply = firstComment.Replies.AsRequested().First();

// Like the reply
await firstCommentReply.LikeAsync();

// Unlike the reply
await firstCommentReply.UnlikeAsync();
```

## Enumerating likes on comments and replies

A comment or reply can be liked by multiple persons and you can enumerate the "likers" by loading the `LikedBy` collection of a comment or reply.

```csharp
// Load the comments with replies 
comments = await newPage.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies, p => p.LikedBy);

// Get first comment
var firstComment = comments.AsRequested().First();

// Enumerate the likes on this comment
foreach (var like on firstComment.LikedBy.AsRequested())
{
    // do something with the comment like information
}
```
