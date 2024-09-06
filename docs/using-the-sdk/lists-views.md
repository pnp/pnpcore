# Working with list views

Each list has one or more views whereas a view presents the list data in certain manner: which fields are shown in which order, sorting and grouping and more is all defined in a view. A list can have multiple views and using PnP Core SDK you can manage views via the [IView interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IView.html).

## Getting the views

To get a view you need to load the [Views property of an IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Views) and then enumerate over the returned views to find the one you want to work with.

```csharp
// Get Documents list with views via title
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

// Get Documents list with views via title
var myList = await context.Web.Lists.GetByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/Shared Documents", p => p.Views);

// Get Documents list views via id, only load the needed properties
var myList = await context.Web.Lists.GetByIdAsync(new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5"), p => p.Views);

// Do something with the views
foreach(var view in myList.Views.AsRequested())
{
    // Do something
}
```

## Adding a view

To add a view you need to use the [AddAsync method on the IViewCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IViewCollection.html#collapsible-PnP_Core_Model_SharePoint_IViewCollection_AddAsync_PnP_Core_Model_SharePoint_ViewOptions_) and specify the [ViewOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ViewOptions.html) for the view to add. Common properties to use when you're adding a view are [Title](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ViewOptions.html#PnP_Core_Model_SharePoint_ViewOptions_Title), [ViewFields](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ViewOptions.html#collapsible-PnP_Core_Model_SharePoint_ViewOptions_ViewFields) and [ViewTypeKind](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ViewOptions.html#collapsible-PnP_Core_Model_SharePoint_ViewOptions_ViewTypeKind).

```csharp
// Get Documents list with views via title
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

var myView = myList.Views.Add(new ViewOptions()
                                {
                                    Title = "My custom view",
                                    RowLimit = 10,
                                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                                    ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu", "Modified" }
                                });
```

## Updating a view

To update a view you set the view properties you need and then call UpdateAsync.

```csharp
// Get Documents list with views via title
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

// Get the view to update
var viewToUpdate = myList.Views.AsRequested().FirstOrDefault(p => p.Title == "All Documents");

// Update the view
viewToUpdate.Title = "All";
await viewToUpdate.UpdateAsync();
```

## Deleting a view

Deleting a view can be done using the regular Delete methods.

```csharp
// Get Documents list with views via title
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

// Get the view to update
var viewToDelete = myList.Views.AsRequested().FirstOrDefault(p => p.Title == "View to delete");

await viewToDelete.DeleteAsync();
```

## Adding a field to an existing view

You can add a new field to an existing view using one of the `AddViewField` methods and supplying the field using it's internal name.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

var myView = myList.Views.Add(new ViewOptions()
                                {
                                    Title = "My custom view",
                                    RowLimit = 10,
                                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                                    ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu" }
                                });
// Add the "Modified" field
await myView.AddViewFieldAsync("Modified");                                
```

## Removing a field from an existing view

You can remove a field from an existing view using one of the `RemoveViewField` methods and supplying the field using it's internal name.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

var myView = myList.Views.Add(new ViewOptions()
                                {
                                    Title = "My custom view",
                                    RowLimit = 10,
                                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                                    ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu", "Modified" }
                                });
// Removes the "Modified" field from the view
await myView.RemoveViewFieldAsync("Modified");                                
```

## Re-ordering a field in an existing view

A field in a view can be moved to a new position using one of the `MoveViewFieldTo` methods.

```csharp
var myList = await context.Web.Lists.GetByTitleAsync("Documents", p => p.Views);

var myView = myList.Views.Add(new ViewOptions()
                                {
                                    Title = "My custom view",
                                    RowLimit = 10,
                                    Query = "<Where><Eq><FieldRef Name='LinkFilename' /><Value Type='Text'>General</Value></Eq></Where>",
                                    ViewFields = new string[] { "DocIcon", "LinkFilenameNoMenu", "Modified" }
                                });

// Moves the "Modified" field to be the first field in the view
await myView.MoveViewFieldToAsync("Modified", 0);                               
```
