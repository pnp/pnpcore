# Content following APIs

Content following APIs allow you to manage different aspects of content following: follow/stop following users, docs, sites and tags, getting followers and checking whether you follow specific content or not.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and shown below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
}
```

## Get the general social following information for the current user

This information contains `MyFollowedDocumentsUri` - the url to see all documents the user is following, `MyFollowedSitesUri` - the url to see all sites the user is following, and the `SocialActor` - an object, which represents information about the current user (account name, email, status and other properties). To get the social following information, call `GetFollowingInfo`:

```csharp
var info = await context.Social.Following.GetFollowingInfoAsync();
```

## Follow the content

You can follow 4 types of entities in SharePoint using social API: a tag, a site, a document and a user. To follow the content, use the corresponding method `Follow`. As an argument you should pass an instance of an object, which represents specific following request:

```csharp
// follow a site
var result = await context.Social.Following.FollowAsync(new FollowSiteData
{
    ContentUri = context.Uri.AbsoluteUri
});

// follow a document
var result = await context.Social.Following.FollowAsync(new FollowDocumentData
{
    ContentUri = $"{siteUrl}/Shared Documents/test.docx"
});

// follow a user
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
var result = await context.Social.Following.FollowAsync(new FollowPersonData
{
    AccountName = accountName
});

// follow a tag
var result = await context.Social.Following.FollowAsync(new FollowTagData
{
    TagGuid = new Guid("4fd0d107-8df7-4ace-bffc-72aa0f9a736a")
});
```

The result is an enum, which contains the outcome - either `Ok`, `AlreadyFollowing`, `LimitReached` or `InternalError` if something went wrong.

## Stop following the content

From the code perspective stop following works very similarly to the `Follow`, except that the method name is `StopFollowing`. For example, to stop following a site, you should call:

```csharp
await context.Social.Following.StopFollowingAsync(new FollowSiteData
{
    ContentUri = context.Uri.AbsoluteUri
});
```

To stop following another object just use the right `FollowData` instance.

## Check whether you follow specific content

```csharp
var followData = new FollowSiteData
{
    ContentUri = context.Uri.AbsoluteUri
};
var isFollowed = await context.Social.Following.IsFollowed(followData);
```

The code above checks if the current user is following a site. To check other content (persons, docs and tags) you should use the corresponding `FollowData` object.

## Get people, who follow the specified user

To get a list of people, who follow the specified user, use code below:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
IList<IPersonProperties> followers = await context.Social.Following.GetFollowersForAsync(accountName);
```

## Gets the people who the specified user is following

As an opposite to `GetFollowersFor`, the below method returns a collection of people, who the specified user is following:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
IList<IPersonProperties> followers = await context.Social.Following.GetPeopleFollowedByAsync(accountName);
```

## Check if the current user is following another user

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
bool isFollowing = await context.Social.Following.AmIFollowingAsync(accountName);
```

The code checks whether the current user is following the specified user.

## Check if the current user is followed by another user

The opposite to `AmIFollowing` is `AmIFollowedBy`. It checks whether the current user is in the followers list for another user:

```csharp
var accountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";
bool followed = context.Social.Following.AmIFollowedBy(accountName);
```

## Get the content, followed by me

As shown before, a user can follow a tag, a document, a person or a site. The `FollowedByMe` method returns the content, which the current user is following:

```csharp
IList<ISocialActor> myFollowingSites = await context.Social.Following.FollowedByMeAsync(SocialActorTypes.Sites);
```

In the parameter you can specify which content you want to receive back. If you need more than one content type per request, you can apply bitwise operation, i.e.:

```csharp
IList<ISocialActor> myFollowingSitesAndUsers = await context.Social.Following.FollowedByMeAsync(SocialActorTypes.Users | SocialActorTypes.Sites);
```

`SocialActorTypes` also contains some special content types like `WithinLast24Hours`, `All` and some other.

## Count the number of following content

To count on how many sites, tags, documents or users you're following, use `FollowedByMeCount`. This method counts following content and returns a number:

```csharp
int count = await context.Social.Following.FollowedByMeCountAsync(SocialActorTypes.Users | SocialActorTypes.Sites);
```

It accepts the same `SocialActorTypes` enum so that you can filter on the following content according to your needs.

## Get my followers

With social following API you can get all current user's followers:

```csharp
IList<ISocialActor> followers = await context.Social.Following.MyFollowersAsync();
```

## Get user suggestions

It's possible to get the list of users, who the current user might want to follow:

```csharp
IList<ISocialActor> suggestions = await context.Social.Following.MySuggestionsAsync();
```
