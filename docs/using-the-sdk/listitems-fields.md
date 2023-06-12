# Working with list item fields

When getting and setting list item values you'll need to work with the various field types SharePoint and PnP Core SDK support. Depending on the field type you'll need to work with different objects as you can see in below chapters.

> [!Note]
> - The samples assume you've loaded a list into the variable `mylist`.
> - When referencing a field keep in mind that you need to use the field's `InternalName`. If you've created a field with name `Version Tag` then the `InternalName` will be `Version_x0020_Tag`, so you will be using `myItem["Version_x0020_Tag"]` to work with the field.

## Text and Multiline text fields

Working with text fields is one of the most common tasks and is easy as shown in the sample.

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a text field
values.Add("MyField", "some text");

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the text field
addedItem["MyField"] = "updated text";
// Or clear the text field
addedItem["MyField"] = "";

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    addedItem["MyField"] = "PnP Rocks! " + addedItem["MyField"].ToString();
}
```

## Number and Currency fields

Number fields can hold both integer and double values and when working with them you can add either an integer or double value, when processing a field value it's best to cast the number field to the type you need.

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a text field
values.Add("MyField", 25);
// or
values.Add("MyField", 25.123);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the text field
addedItem["MyField"] = 100;
// Or clear the text field
addedItem["MyField"] = 0;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value
if (addedItem["MyField"] is double doubleValue)
{
    // do something with the field value as double
    addedItem["MyField"] = addedItem["MyField"] + doubleValue;
}
else
{
    // do something with the field value as integer 
    addedItem["MyField"] = ((int)addedItem["MyField"]) + 20;
}
```

## Boolean fields

Boolean fields are straightforward to use:

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a text field
values.Add("MyField", true);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the text field
addedItem["MyField"] = false;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value
addedItem["MyField"] = !((bool)addedItem["MyField"]);
```

## DateTime fields

DateTime are slightly special to work with as a SharePoint site can be configured to use a different timezone then the process running the PnP Core SDK is. When you provide a DateTime value as input you need to provide the value in the current timezone running in the process or as UTC time. PnP Core SDK will, if needed, translate the time to the site's timezone before submitting. When reading DateTime values from the server you'll get time back as local time, unless you've used one of the **DatesInUtc** property when using the [GetItemsByCamlQueryAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_GetItemsByCamlQueryAsync_PnP_Core_Model_SharePoint_CamlQueryOptions_) or [GetListDataAsStreamAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html#PnP_Core_Model_SharePoint_IList_GetListDataAsStreamAsync_PnP_Core_Model_SharePoint_RenderListDataOptions_) approaches to read data.

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a text field
values.Add("MyField", DateTime.Now);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the DateTime field
addedItem["MyField"] = DateTime.Now.Subtract(new TimeSpan(10,0,0,0));
// Or clear the text field
addedItem["MyField"] = null;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    addedItem["MyField"] = DateTime.Now - ((DateTime)addedItem["MyField"]);
}
```

## Calculated fields

You never set or update calculated fields, you however might use it's output.

```csharp
// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    // Cast to the type you need
    var a = addedItem["MyField"].ToString();
}
```

## Choice fields

Setting a choice field is quite identical as working with text fields, you set a string value. Only difference is that the string you use to set a value needs to be known value in the list of choices unless you've configured the choice field to allow the user to add values via the [FillInChoice property](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldChoiceMultiOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldChoiceMultiOptions_FillInChoice).

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a choice field
values.Add("MyField", "Choice B");

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the choice value
addedItem["MyField"] = "Choice A";
// Or clear the choice field
addedItem["MyField"] = "";

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    var a = "My choice was:" + addedItem["MyField"].ToString();
}
```

## Multi choice fields

A multi choice field allows a user to select multiple values from the offered choices, so to handle that you're working with a `List<string>`:

```csharp
// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a multi choice field
values.Add("MyField", new List<string> { "Choice A", "Choice B" } );

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the multi choice field
addedItem["MyField"] = new List<string> { "Choice A", "Choice B" , "Choice C", "Choice D"};
// Or clear the multi choice field
addedItem["MyField"] = new List<string>();

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    foreach(var choice in (addedItem["MyField"] as List<string>) )
    {
        // do something with the choice
    }
}
```

## Url fields

Url fields can be used to display a hyperlink with description or show an image from a hyperlink. Working with Url fields involves either directly instantiating an `FieldUrlValue` instance or working with the [IFieldUrlValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldUrlValue.html) for field setting and the [NewFieldUrlValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldUrlValue_System_String_System_String_) or [NewFieldUrlValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#PnP_Core_Model_SharePoint_IListItem_NewFieldUrlValue_PnP_Core_Model_SharePoint_IField_System_String_System_String_) methods to instantiate a FieldUrlValue class.

```csharp
// Add a url field
IField myField = await myList.Fields.AddUrlAsync("MyField", new FieldUrlOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Option A: instantiate a FieldUrlValue instance
values.Add("MyField", new FieldUrlValue("https://aka.ms/m365pnp", "PnP Rocks!"));

// Option B: Add a url field: using NewFieldUrlValue
values.Add("MyField", myField.NewFieldUrlValue("https://aka.ms/m365pnp", "PnP Rocks!"));

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the url field
(addedItem["MyField"] as IFieldUrlValue).Url = "https://aka.ms/pnp/coresdk";
// Or clear the url field
(addedItem["MyField"] as IFieldUrlValue).Url = "";
(addedItem["MyField"] as IFieldUrlValue).Url = "";

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    string urlToUse = (addedItem["MyField"] as IFieldUrlValue).Url;
}
```

## User fields

User fields hold a value to a user or group. Working with user fields involves either directly instantiating an `FieldUserValue` instance or working with the [IFieldUserValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldUserValue.html) for field setting and the [NewFieldUserValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldUserValue_PnP_Core_Model_Security_ISharePointPrincipal_) or [NewFieldUserValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#collapsible-PnP_Core_Model_SharePoint_IListItem_NewFieldUserValue_PnP_Core_Model_SharePoint_IField_PnP_Core_Model_Security_ISharePointPrincipal_) methods to instantiate a FieldUserValue class.

```csharp
// Add a user field
IField myField = await myList.Fields.AddUserAsync("MyField", new FieldUserOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Ensure a user can be used on a site
var myUser = await context.Web.EnsureUserAsync("ann@contoso.onmicrosoft.com");

// Option A: instantiate a FieldUserValue instance
values.Add("MyField", new FieldUserValue(myUser));

// Option B: Add a url field: using NewFieldUserValue
values.Add("MyField", myField.NewFieldUserValue(myUser));

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the url field
(addedItem["MyField"] as IFieldUserValue).Principal = await context.Web.EnsureUserAsync("Everyone except external users");
// Or clear the url field
addedItem["MyField"] = null;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    // Get the stored user lookup id value
    int userId = (addedItem["MyField"] as IFieldUserValue).LookupId;
}
```

## Multi user fields

Working with multi user fields builds on top of working with user fields, you still directly instantiating `FieldUserValue` instances or use the [IFieldUserValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldUserValue.html) for field setting and the [NewFieldUserValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldUserValue_PnP_Core_Model_Security_ISharePointPrincipal_) or [NewFieldUserValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#collapsible-PnP_Core_Model_SharePoint_IListItem_NewFieldUserValue_PnP_Core_Model_SharePoint_IField_PnP_Core_Model_Security_ISharePointPrincipal_) methods, but since you need to store multiple users you need to manage them via an [IFieldValueCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldValueCollection.html).

> [!Note]
> It's important to create an `IFieldValueCollection` (e.g. via `IField.NewFieldValueCollection()` or via `new FieldValueCollection()`) per `IListItem` you're adding as the `IFieldValueCollection` handles the change tracking for that specific `IListItem`.

```csharp
// Add a multi user field
IField myField = await myList.Fields.AddUserMultiAsync("MyField", new FieldUserOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Ensure a user can be used on a site
var myUser1 = await context.Web.EnsureUserAsync("ann@contoso.onmicrosoft.com");
var myUser2 = await context.Web.EnsureUserAsync("pat@contoso.onmicrosoft.com");

// Add a multi user field

// Option A: instantiate a FieldValueCollection
var userCollection = new FieldValueCollection();
userCollection.Values.Add(new FieldUserValue(myUser1));
userCollection.Values.Add(new FieldUserValue(myUser2));

// Option B: use the NewFieldValueCollection method
var userCollection = myField.NewFieldValueCollection();
userCollection.Values.Add(myField.NewFieldUserValue(myUser1));
userCollection.Values.Add(myField.NewFieldUserValue(myUser2));

values.Add("MyField", userCollection);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the multi user field
var myUser3 = await context.Web.EnsureUserAsync("bob@contoso.onmicrosoft.com");
(addedItem["MyField"] as IFieldValueCollection).Values.Add(myField.NewFieldUserValue(myUser3));
// Or clear the multi user field
addedItem["MyField"] = (addedItem["MyField"] as IFieldValueCollection).Values.Clear();

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    foreach(IFieldUserValue user in (addedItem["MyField"] as IFieldValueCollection).Values)
    {
        // Get the stored user lookup id value
        int userId = user.LookupId; 
    }    
}
```

## Lookup fields

Lookup fields point to another list item in another list. Working with lookup fields involves either directly instantiating an `FieldLookupValue` instance or working with the [IFieldLookupValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldLookupValue.html) for field setting and the [NewFieldLookupValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldLookupValue_System_Int32_) or [NewFieldLookupValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#PnP_Core_Model_SharePoint_IListItem_NewFieldLookupValue_PnP_Core_Model_SharePoint_IField_System_Int32_) methods to instantiate a FieldLookupValue class.

```csharp
IList sitePages = await context.Web.Lists.GetByTitleAsync("Site Pages");

// Add a lookup field
IField myField = await myList.Fields.AddLookupAsync("MyField", new FieldLookupOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    LookupListId = sitePages.Id,
    LookupFieldName = "Title",
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};


// Option A: instantiate a FieldLookupValue instance to add a lookup field to an item in the connected lookup list with id 4
values.Add("MyField", new FieldLookupValue(4));

// Option B: Add a lookup field using NewFieldLookupValue to add a lookup field to an item in the connected lookup list with id 4
values.Add("MyField", myField.NewFieldLookupValue(4));

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the lookup field
(addedItem["MyField"] as IFieldLookupValue).LookupId = 8;
// Or clear the lookup field
addedItem["MyField"] = null;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    // Get the stored user lookup id value
    int userId = (addedItem["MyField"] as IFieldLookupValue).LookupId;
}
```

## Multi lookup fields

Working with multi lookup fields builds on top of working with lookup fields, you still can directly instantiate `FieldLookupValue` instances or use the [IFieldLookupValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldLookupValue.html) for field setting and the [NewFieldLookupValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldLookupValue_System_Int32_) or [NewFieldLookupValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#PnP_Core_Model_SharePoint_IListItem_NewFieldLookupValue_PnP_Core_Model_SharePoint_IField_System_Int32_) methods, but since you need to store multiple lookups you need to manage them via an [IFieldValueCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldValueCollection.html).

> [!Note]
> It's important to create an `IFieldValueCollection` (e.g. via `IField.NewFieldValueCollection()` or via `new FieldValueCollection()`) per `IListItem` you're adding as the `IFieldValueCollection` handles the change tracking for that specific `IListItem`.

```csharp
IList sitePages = await context.Web.Lists.GetByTitleAsync("Site Pages");

// Add a multi lookup field
IField myField = await myList.Fields.AddLookupMultiAsync("MyField", new FieldLookupOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    LookupListId = sitePages.Id,
    LookupFieldName = "Title",
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a multi lookup field

// Option A: instantiate a FieldValueCollection
var lookupCollection = new FieldValueCollection();
lookupCollection.Values.Add(new FieldLookupValue(4));
lookupCollection.Values.Add(new FieldLookupValue(8));

// Option B: use the NewFieldValueCollection method
var lookupCollection = myField.NewFieldValueCollection();
lookupCollection.Values.Add(myField.NewFieldLookupValue(4));
lookupCollection.Values.Add(myField.NewFieldLookupValue(8));

values.Add("MyField", lookupCollection);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the multi lookup field
(addedItem["MyField"] as IFieldValueCollection).Values.Add(myField.NewFieldLookupValue(12));
// Or clear the multi user field
addedItem["MyField"] = (addedItem["MyField"] as IFieldValueCollection).Values.Clear();

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    foreach(IFieldLookupValue lookup in (addedItem["MyField"] as IFieldValueCollection).Values)
    {
        // Get the stored lookup id value
        int lookupId = lookup.LookupId; 
    }    
}
```

## Taxonomy fields

Taxonomy fields make it possible to select a value from a term set in your tenants managed metadata system. This involves either directly instantiating an `FieldTaxonomyValue` instance or working with the [IFieldTaxonomyValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldTaxonomyValue.html) for field setting and the [NewFieldTaxonomyValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldTaxonomyValue_Guid_System_String_System_Int32_) or [NewFieldTaxonomyValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#PnP_Core_Model_SharePoint_IListItem_NewFieldTaxonomyValue_PnP_Core_Model_SharePoint_IField_Guid_System_String_System_Int32_) methods to instantiate a FieldTaxonomyValue class.

```csharp
// Add a taxonomy field
IField myField = await myList.Fields.AddTaxonomyAsync("MyField", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Option A: Add a taxonomy field to a term with the given id and label "Dutch" by directly instantiating a FieldTaxonomyValue class
values.Add("MyField", new FieldTaxonomyValue(new Guid("108b34b1-87af-452d-be13-881a29477965", "Dutch")));

// Option B: Add a taxonomy field to a term with the given id and label "Dutch" using the NewFieldTaxonomyValue method
values.Add("MyField", myField.NewFieldTaxonomyValue(new Guid("108b34b1-87af-452d-be13-881a29477965", "Dutch")));

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the taxonomy field
(addedItem["MyField"] as IFieldTaxonomyValue).TermId = new Guid("8246e3c1-19ea-4b22-8ae3-df9cbc150a74");
(addedItem["MyField"] as IFieldTaxonomyValue).Label = "English";
// Or clear the taxonomy field
addedItem["MyField"] = null;

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    // Get the stored term id value
    Guid termId = (addedItem["MyField"] as IFieldTaxonomyValue).TermId;
}
```

## Multi taxonomy fields

Working with multi taxonomy fields builds on top of working with taxonomy fields, you still can directly instantiate `FieldTaxonomyValue` instances or use the [IFieldTaxonomyValue interface](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldTaxonomyValue.html) for field setting and the [NewFieldTaxonomyValue method on the IField](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IField.html#PnP_Core_Model_SharePoint_IField_NewFieldTaxonomyValue_Guid_System_String_System_Int32_) or [NewFieldTaxonomyValue method on the IListItem](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListItem.html#PnP_Core_Model_SharePoint_IListItem_NewFieldTaxonomyValue_PnP_Core_Model_SharePoint_IField_Guid_System_String_System_Int32_) methods, but since you need to store multiple taxonomy fields you need to manage them via an [IFieldValueCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldValueCollection.html).

> [!Note]
> It's important to create an `IFieldValueCollection` (e.g. via `IField.NewFieldValueCollection()` or via `new FieldValueCollection()`) per `IListItem` you're adding as the `IFieldValueCollection` handles the change tracking for that specific `IListItem`.

```csharp
// Add a multi taxonomy field
IField myField = await myList.Fields.AddTaxonomyMultiAsync("MyField", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
});

// Create a list item key/value pair collection
Dictionary<string, object> values = new Dictionary<string, object>()
{
    { "Title", "Item1" },
};

// Add a taxonomy field to a term with the given id and label "Dutch"
values.Add("MyField", myField.NewFieldTaxonomyValue(new Guid("108b34b1-87af-452d-be13-881a29477965", "Dutch")));

// Option A: instantiate a FieldValueCollection
var taxonomyCollection = new FieldValueCollection();
taxonomyCollection.Values.Add(new FieldTaxonomyValue(new Guid("108b34b1-87af-452d-be13-881a29477965", "Dutch")));
taxonomyCollection.Values.Add(new FieldTaxonomyValue(new Guid("8246e3c1-19ea-4b22-8ae3-df9cbc150a74", "English")));

// Option B: use the NewFieldValueCollection method
var taxonomyCollection = myField.NewFieldValueCollection();
taxonomyCollection.Values.Add(myField.NewFieldTaxonomyValue(new Guid("108b34b1-87af-452d-be13-881a29477965", "Dutch")));
taxonomyCollection.Values.Add(myField.NewFieldTaxonomyValue(new Guid("8246e3c1-19ea-4b22-8ae3-df9cbc150a74", "English")));

values.Add("MyField", taxonomyCollection);

// Persist the item
var addedItem = await myList.Items.AddAsync(values);

// Update the multi taxonomy field
(addedItem["MyField"] as IFieldValueCollection).Values.Add(myField.NewFieldTaxonomyValue(new Guid("3f773e87-24c3-4d0d-a07f-96eb0c1e905e", "French")));
// Or clear the multi taxonomy field
addedItem["MyField"] = (addedItem["MyField"] as IFieldValueCollection).Values.Clear();

// Update the item on the server
await addedItem.UpdateAsync();

// Using the value when not cleared
if (addedItem["MyField"] != null)
{
    foreach(IFieldTaxonomyValue taxField in (addedItem["MyField"] as IFieldValueCollection).Values)
    {
        // Get the stored term id value
        Guid termId = taxField.TermId;    
    }     
}
```
