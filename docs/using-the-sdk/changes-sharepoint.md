# Enumerating changes that happened in SharePoint

SharePoint Online does track many of the changes made to sites, lists, list items, folders and more. Using the `GetChanges` methods you can enumerate these changes and use them in your application: a common scenario is using a web hook on a library that notifies your code a change has happened and then your code uses a `GetChanges` call to figure out the exact change details.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with files
}
```

## Scoping the change query

To get changes you issue a query via one of the `GetChanges` methods and to configure the change query you have to pass in a `ChangeQueryOptions` instance. Via the `ChangeQueryOptions` you specify for which SharePoint objects (e.g. Item, List,...) you need changes and which change types (e.g. Add, Update, ...) you're interested in. The possible SharePoint objects are:

- Item
- List
- Web
- Site
- File
- Folder
- Alert
- User
- Group
- ContentType
- Field
- SecurityPolicy
- View

And the possible change types are:

- Add
- Update
- DeleteObject
- Rename
- Move
- Restore
- RoleDefinitionAdd
- RoleDefinitionDelete
- RoleDefinitionUpdate
- RoleAssignmentAdd
- RoleAssignmentDelete
- GroupMembershipAdd
- GroupMembershipDelete
- SystemUpdate
- Navigation

## Performing a change query

A change query is performed via one of the `GetChanges` methods, passing in a configured `ChangeQueryOptions` instance. The `GetChanges` methods are available on these SharePoint model classes:

- ISite
- IWeb
- IList
- IListItem
- IFolder

The model on which you call `GetChanges` scopes the result: calling `GetChanges` on an IList will only return changes for that particular list. Below sample gets all types of changes for all SharePoint objects scoped to a web.

```csharp
// Get all type of changes for all SharePoint objects
var changes = context.Web.GetChanges(new ChangeQueryOptions(true, true));
```

Below sample shows how to get all ContentType and Field adds, updates and deletes for a site collection:

```csharp
// Get adds, updates and deletes for contenttypes and fields 
var changes = context.Site.GetChanges(new ChangeQueryOptions()
                    {
                      ContentType = true,  
                      Field = true,
                      Add = true,
                      Update = true,
                      DeleteObject = true
                    }
);
```

In this example the last 50 changes, regardless of change type, for items in a list are returned:

```csharp
// Get all type of changes for a SharePoint list limited to the 50 most recent changes
var list = await context.Web.Lists.GetByTitleAsync("Documents");
var changes = await context.Site.GetChangesAsync(new ChangeQueryOptions(false, true)
                    {
                      List = true,  
                      FetchLimit = 50
                    }
);
```

When getting changes you often want to just get the changes since you previously asked for changes and this can be done by using change tokens. Each change you get contains a change token and when making a next change query you can get all changes starting from a given change token:

```csharp
// Get all type of changes for all SharePoint objects
var changes = context.Web.GetChanges(new ChangeQueryOptions(true, true));

// Store the change token received with the last change
var lastChangetoken = changes.Last().ChangeToken;

// Get all changes that happened after the last change
var changes2 = await context.Web.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    ChangeTokenStart = firstChangeToken
                });
```

The `lastChangeToken` in previous example is of type `IChangeToken`, if you've stored the actual change token (e.g."1;2;8c8e101c-1b0d-4253-85e7-c30039bf46e2;637577313637500000;563287977") you can construct an `IChangeToken` for it as follows:

```csharp
var lastChangetoken = new ChangeTokenOptions("1;2;8c8e101c-1b0d-4253-85e7-c30039bf46e2;637577313637500000;563287977");
```

## Processing the returned changes

The returned collection of changes all inherit of the same base model `IChange`, but depending on the changed object you get different model. Changes on a web result in a `IChangeWeb` instance, changes on a list in a `IChangeList` instance and so on. When iterating over the changes you can check for the type and cast it as shown in below example. When a change is returned not all properties of the returned object are always populated, use the `IsPropertyAvailable` method to verify a property is populated before using it (in case the property is not always populated).

```csharp
// Get adds, updates and deletes for contenttypes and fields 
var changes = context.Site.GetChanges(new ChangeQueryOptions()
                    {
                      ContentType = true,  
                      Field = true,
                      Add = true,
                      Update = true,
                      DeleteObject = true
                    }
);

foreach (var change in changes)
{
    if (change is IChangeItem changeItem)
    {
        DateTime changeTime = changeItem.Time;
        
        if (changeItem.IsPropertyAvailable<IChangeItem>(p => p.ListId))
        {
            // do something with the returned list id
        }      

        if (changeItem.ChangeType == ChangeType.Add)
        {
            // do something "add" specific
        }
    } 
    else if (change is IChangeField changeField)
    {
        // process the change
    }
}
```
