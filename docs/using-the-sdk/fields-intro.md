# Site and List fields

Each SharePoint site uses [fields](https://support.microsoft.com/en-us/office/list-and-library-column-types-and-options-0d8ddb7b-7dc7-414d-a283-ee9dca891df7), a site comes prepopulated with a set of site fields and a set of lists and libraries that use these fields. You can also create your own fields, either being a site field or list field. A site field can be reused across multiple lists in the site collection, whereas a list field only can be used in the list for which the field was created. Creating site and list fields is from a developer point of view quite similar so both options are discussed together in this article.

The PnP Core SDK does support the common fields SharePoint offers:

- Text
- Multiline text
- Number
- Boolean
- DateTime
- Currency
- Calculated
- Choice
- Multi choice
- Url
- User
- Multi user
- Lookup
- Multi lookup
- Taxonomy
- Multi taxonomy
- Location (by GA timeframe)

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with fields
}
```

## Getting the current site and list fields

Getting the currently defined fields on a web can be done by loading the [Fields](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#PnP_Core_Model_SharePoint_IWeb_Fields) property as that will return an [IFieldCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldCollection.html):

```csharp
//  Get site fields
var web = await context.Web.GetAsync(l => l.Fields);

foreach(var field in web.Fields.AsRequested())
{
    // do something with the field
}
```

For a list similar logic applies, you can load the [Fields](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_Fields) property on an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html):

```csharp
//  Get documents library with fields loaded
var documents = await context.Web.Lists.GetByTitleAsync("Documents", l => l.Fields);

foreach(var field in documents.Fields.AsRequested())
{
    // do something with the field
}
```

Above samples do load all the default Field properties, but what if you are only interested in a few properties or the property you need is not loaded by default? For that purpose the best approach is using the QueryProperties method as that allows for getting the needed field data in a single roundtrip to the server. Below code snippets show this method in action for loading specific field properties

```csharp
//  Get site fields
var web = await context.Web.GetAsync(l => l.Fields.QueryProperties(l => l.Id, l => l.InternalName, l => l.FieldTypeKind));

foreach(var field in web.Fields.AsRequested())
{
    // do something with the field
}
```

```csharp
//  Get documents library with fields loaded
var documents = await context.Web.Lists.GetByTitleAsync("Documents", l => l.Fields.QueryProperties(l => l.Id, l => l.InternalName, l => l.FieldTypeKind));

foreach(var field in documents.Fields.AsRequested())
{
    // do something with the field
}
```

## Adding site and list fields

Adding fields is described in the [Adding fields](fields-add.md) article.

## Updating site and list fields

To update a field you simply update it's value and call the Update or UpdateAsync methods:

```csharp
// Find a field with a given id
Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
IField field = await context.Web.Fields.Where(f => f.Id == titleFieldId).FirstOrDefaultAsync();

if (field != null)
{
    field.Hidden = true;
    await field.UpdateAsync();
}
```

If you want to update a site field and push the field updates to the lists using that field then you'd need to use one of the `UpdateAndPushChanges` methods:

```csharp
// Find a field with a given id
Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
IField field = await context.Web.Fields.Where(f => f.Id == titleFieldId).FirstOrDefaultAsync();

if (field != null)
{
    field.Hidden = true;
    await field.UpdateAndPushChangesAsync();
}
```

## Deleting site and list fields

Deleting a field can be done using the Delete or DeleteAsync methods:

```csharp
// Find a field with a given id
Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
IField field = await context.Web.Fields.Where(f => f.Id == titleFieldId).FirstOrDefaultAsync();

if (field != null)
{
    await field.DeleteAsync();
}
```

## Control field visibility on list display, edit and new forms

By default all fields on a list, unless marked as hidden, will appear on the list forms (display, edit and new). If you want to change the visibility of a field on one of these forms then you can use the `SetShowInDisplayForm`,  `SetShowInEditForm` or `SetShowInNewForm` methods.

```csharp
// Find a field with a given id on a web or list
Guid titleFieldId = new Guid("fa564e0f-0c70-4ab9-b863-0177e6ddd247");
IField field = await context.Web.Fields.Where(f => f.Id == titleFieldId).FirstOrDefaultAsync();

if (field != null)
{
    // Hide field in new display form
    await field.SetShowInDisplayFormAsync(false);

    // Hide field in new edit form
    await field.SetShowInEditFormAsync(false);

    // Hide field in new new form
    await field.SetShowInNewFormAsync(false);
}
```
