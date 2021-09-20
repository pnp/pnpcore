# Commenting list item fields

List items can have comments and replies in SharePoint. This chapter explains how to enable or disable list item commenting and how to work with comments and replies.

> [!Note]
> Some of the samples assume you've loaded a list into the variable `mylist`, the code that shows how to do so is listed in the first examples.

## Enabling/Disabling list item comments

List items can have comments in SharePoint and using the [SetCommentsDisabledAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemBase.html#PnP_Core_Model_SharePoint_IListItemBase_SetCommentsDisabledAsync_System_Boolean_) you can turn off commenting for a given list item. This method goes hand in hand with the [AreCommentsDisabledAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItemBase.html#PnP_Core_Model_SharePoint_IListItemBase_AreCommentsDisabledAsync) method to get the current commenting status of a list item.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Get the item with title "Item1"
var addedItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

// Check if commenting was turned off
if (!(await addedItem.AreCommentsDisabledAsync()))
{
    // Turn commenting of the list item on
    await addedItem.SetCommentsDisabledAsync(false);
}
```

## Getting comments

To work with list item comments you first need to get a reference to the list item comments collection. Once you've done that, you can enumerate the existing comments, add new comments or delete comments. Below code snippet shows how the comments can be enumerated.

```csharp
// Assume the fields where not yet loaded, so loading them with the list
var myList = context.Web.Lists.GetByTitle("My List", p => p.Title, p => p.Items, 
                                                     p => p.Fields.QueryProperties(p => p.InternalName, 
                                                                                   p => p.FieldTypeKind, 
                                                                                   p => p.TypeAsString, 
                                                                                   p => p.Title));
// Get the item with title "Item1"
var myItem = myList.Items.AsRequested().FirstOrDefault(p => p.Title == "Item1");

// Get the comments for this list item
var comments = await myItem.GetCommentsAsync();

// Loop over the comments
foreach(var comment in comments.AsRequested())
{
    // Do something with the comment
}
```

## Adding a comment

Once you've load a comments collection via one of the `GetComments` methods you can add new comments or delete comments.

```csharp
// Get the comments for this list item
var comments = await myItem.GetCommentsAsync();

// Add a comment
var comment = await comments.AddAsync("this is great");
```

## Adding a reply to a comment

Comments can also have replies and since the collection of replies is similar to the collections of comments to code to add a reply is the same:

```csharp
// Get the comments for this list item
var comments = await myItem.GetCommentsAsync();

// Add a comment
var comment = await comments.AddAsync("this is great");

// Add a reply to the comment
var reply = await comment.Replies.AddAsync("yes this is great!");
```

## Removing a comment or reply

Once you've load a comments collection via one of the `GetComments` methods you can delete comments and/or their replies using one of the `Delete` methods.

```csharp
// Load the comments with replies 
comments = await myItem.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies);

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
comments = await myItem.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies);

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
comments = await myItem.GetCommentsAsync(p => p.Author, p => p.Text, p => p.Replies, p => p.LikedBy);

// Get first comment
var firstComment = comments.AsRequested().First();

// Enumerate the likes on this comment
foreach (var like on firstComment.LikedBy.AsRequested())
{
    // do something with the comment like information
}
```
