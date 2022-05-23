# Working with Teams: Tags

Within Teams, you are able to use tags. Tags let you quickly reach a group of people all at once. This page will show you how you can Get, Create, Update and Delete tags.

[!INCLUDE [Creating Context](fragments/creating-context.md)]

## Getting Tags

Tags is a collection part of the ITeam interface, so when you get a team, you can include the tags on the request.

```csharp
// Get the Team
 var team = await context.Team.GetAsync(o => o.Tags);

// Get the Tags
 var tags = team.Tags;
 ```

## Creating Tags

To add a new tab, call the Add method, specifying a display name and the users to be associated with the tags.

> [!Note]
> The user to be associated with the tag has to be a member of the team.

```csharp
// Get the Team
var team = await context.Team.GetAsync(x => x.Tags, x => x.Members);

// Get the user to associate the tag with
var userId = team.Members.AsRequested().First().Id;

// Perform the add operation
await team.Tags.AddAsync(new TeamTagOptions
{
    DisplayName = "PnP Tag",
    Members = new List<TeamTagUserOptions>
    {
        new TeamTagUserOptions
        { 
            UserId = userId
        }
    }
});
```

## Updating Tags

You can update the tag by changing the properties you wish update and call the update method:

```csharp
// Get the Team
var team = await context.Team.GetAsync(p => p.Tags);

string tagName = "Tag to update";

// Get the tag you wish to update
var tagToUpdate = team.Tags.Where(p => p.DisplayName == tagName).FirstOrDefault();

if(tagToUpdate != default)
{
    tagToUpdate.DisplayName = "This tag has been updated by the PnP Core SDK!";
    
    // Perform the update to the tag
    await tagToUpdate.UpdateAsync();
}
```

## Deleting Tags

You can delete the tag with the following example:

```csharp
/// Get the Team
var team = await context.Team.GetAsync(p => p.Tags);

string tagName = $"Tag to delete";

// Get the tag you wish to delete
var tagToDelete = team.Tags.Where(p => p.DisplayName == tagName).FirstOrDefault();

if(tagToDelete != default)
{    
    // Perform the delete operation
    await tagToDelete.DeleteAsync();
}
```
