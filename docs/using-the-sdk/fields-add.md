# Adding Site and List fields

Using PnP Core SDK you can configure and add the commonly used SharePoint fieldtypes as either a site field or list field. The approach to configure and add a field is quite similar, but given different fields do have different options there are some differences. In this chapter you'll learn how to add site and list fields. Before we start it's important to understand that all fields share a common set of properties that can be set at configuration time which are described in the [CommonFieldOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html). Commonly used field configuration options are [AddToDefaultView](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html#collapsible-PnP_Core_Model_SharePoint_CommonFieldOptions_AddToDefaultView), [Group](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html#collapsible-PnP_Core_Model_SharePoint_CommonFieldOptions_Group), [Required](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html#collapsible-PnP_Core_Model_SharePoint_CommonFieldOptions_Required), [ShowInEditForm](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html#collapsible-PnP_Core_Model_SharePoint_CommonFieldOptions_ShowInEditForm). When adding a field you can optionally also set the [Id](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.CommonFieldOptions.html#collapsible-PnP_Core_Model_SharePoint_CommonFieldOptions_Id) property of the field.

Adding fields typically is done using one of the Add*FieldType* or Add*FieldType*Async methods, but if the provided options are not sufficient for your use case you can always add a field by providing the raw field XML as described in the last chapter of this article.

> [!Note]
> The code samples create field on a list "myList" by adding the fields to the list's Fields collection. The same logic applies for adding site fields by adding them to the web's Fields collection.

> [!Important]
> By default the passed field title will be used as display name and the internal name will be deducted from that. In some cases you may want to control the internal name via code, you can do this by setting the `InternalName` property of the respective field options class. The sample in next paragraph shows this.

## Text fields

When adding a text field you use the [FieldTextOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldTextOptions.html) which allows to define the [MaxLenght](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldTextOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldTextOptions_MaxLength) of a text field.

```csharp
IField myField = await myList.Fields.AddTextAsync("My Field", new FieldTextOptions()
{
    InternalName = "MyField",
    Group = "Custom Fields",
    AddToDefaultView = true,
    MaxLength = 30
});
```

## Multiline text fields

The [FieldMultiLineTextOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldMultilineTextOptions.html) is used to configure Multiline fields. Commonly used options are [RichText](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldMultilineTextOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldMultilineTextOptions_RichText) and [NumberOfLines](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldMultilineTextOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldMultilineTextOptions_NumberOfLines).

```csharp
IField myField = await myList.Fields.AddMultilineTextAsync("My Field", new FieldMultilineTextOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    RichText = true
});
```

## Number

For adding number fields you need to provide the field configuration using the [FieldNumberOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldNumberOptions.html), setting the number of [Decimals](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldNumberOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldNumberOptions_Decimals) is the most commonly used number configuration.

```csharp
IField myField = await myList.Fields.AddNumberAsync("My Field", new FieldNumberOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    MinimumValue = 0,
    MaximumValue = 100
});
```

## Boolean

Boolean fields are added using the [FieldBooleanOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldBooleanOptions.html), but in contrast to the other fields there are no specific Boolean field configuration properties.

```csharp
IField myField = await myList.Fields.AddBooleanAsync("My Field", new FieldBooleanOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
});
```

## DateTime

Use the [FieldDateTimeOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldDateTimeOptions.html) to add a DateTime field. Setting the [DisplayFormat](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldDateTimeOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldDateTimeOptions_DisplayFormat) to choose between displaying date and time or just date is often used.

```csharp
IField myField = await myList.Fields.AddDateTimeAsync("My Field", new FieldDateTimeOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
});
```

## Currency

Currency fields are less frequently used but in case you want to create the currency field the [FieldCurrencyOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldCurrencyOptions.html) will allow you to configure the field.

```csharp
IField myField = await myList.Fields.AddCurrencyAsync("My Field", new FieldCurrencyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    Decimals = 2
});
```

## Calculated

Calculated are a special kind of field as you're specifying a formula that dynamically calculates the field value. To configure a calculated field you'd use the [FieldCalculatedOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldCalculatedOptions.html) and setting the [Formula](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldCalculatedOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldCalculatedOptions_Formula) is a required thing to do. In below sample a very simple formula is used as does a calculation always resulting in a value of 0.5 shown as a percentage. Another formula example is `=[Date Completed]-[Start Date]` returning a DateTime value. See the [Examples of common formulas in lists](https://support.microsoft.com/en-us/office/examples-of-common-formulas-in-lists-d81f5f21-2b4e-45ce-b170-bf7ebf6988b3) article to learn more about how to write formulas.

```csharp
IField myField = await myList.Fields.AddCalculatedAsync("My Field", new FieldCalculatedOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    Formula = @"=1-0.5",
    OutputType = FieldType.Number,
    ShowAsPercentage = true,
});
```

## Choice

Choice fields allow the user to select a value from a list of possible options. Configuring choice fields can be done via the [FieldChoiceOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldChoiceOptions.html) which allows to choose for example between displaying the choice field as `DropDown` or `RadioButtons` by setting the [EditFormat](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldChoiceOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldChoiceOptions_EditFormat) property. Possible choices themselves are added via the [Choices](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldChoiceMultiOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldChoiceMultiOptions_Choices) property.

```csharp
IField myField = await myList.Fields.AddChoiceAsync("My Field", new FieldChoiceOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    Choices = new List<string>() { "Option A", "Option B", "Option C" }.ToArray(),
    DefaultChoice = "Option B"
});
```

## Multi choice

Choice fields also can allow the user to select multiple options and then they're called multi choice fields. Use the [FieldChoiceMultiOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldChoiceMultiOptions.html) to configure your multi choice fields.

```csharp
IField myField = await myList.Fields.AddChoiceMultiAsync("My Field", new FieldChoiceOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    Choices = new List<string>() { "Option A", "Option B", "Option C", "Option D", "Option E" }.ToArray(),
});
```

## Url

Url fields store an URL and you can configure them via the [DisplayFormat](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldUrlOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldUrlOptions_DisplayFormat) property to display either the URL as a link or show the image defined by the URL. The configuration class to use is [FieldUrlOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldUrlOptions.html).

```csharp
IField myField = await myList.Fields.AddUrlAsync("My Field", new FieldUrlOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    DisplayFormat = UrlFieldFormatType.Hyperlink
});
```

## User

A user field stores the reference to a user or group and to configure it the [FieldUserOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldUserOptions.html) is needed. Key configuration to set is the [SelectionMode](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldUserOptions.html) as it determines what type of principals (users, groups) can be added in this field.

```csharp
IField myField = await myList.Fields.AddUserAsync("My Field", new FieldUserOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
});
```

## Multi user

This field is identical to the **User** field but with as difference that you can add multiple users/groups.

```csharp
IField myField = await myList.Fields.AddUserMultiAsync("My Field", new FieldUserOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    SelectionMode = FieldUserSelectionMode.PeopleAndGroups
});
```

## Lookup

Lookup fields allow you to configure a lookup to another list and then show a field from the looked value. These fields are configured via the [FieldLookupOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldLookupOptions.html), key properties to set are the id of the list to lookup from ([LookupListId](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldLookupOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldLookupOptions_LookupListId)) and the field of the looked up list for of which the value will be shown in this field ([LookupFieldName](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldLookupOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldLookupOptions_LookupFieldName)).

```csharp
IList listToLookupFrom = await context.Web.Lists.GetByTitleAsync("ListWithInterestingData");
IField myField = await myList.Fields.AddLookupAsync("My Field", new FieldLookupOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    LookupListId = listToLookupFrom.Id,
    LookupFieldName = "Title",
});
```

## Multi lookup

When you want to be able to select multiple lookup items you need a multi lookup field. Configuration wise this is very similar to the **Lookup** field described previously.

```csharp
IList listToLookupFrom = await context.Web.Lists.GetByTitleAsync("ListWithInterestingData");
IField myField = await myList.Fields.AddLookupMultiAsync("My Field", new FieldLookupOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    LookupListId = listToLookupFrom.Id,
    LookupFieldName = "Title",
});
```

## Taxonomy

Customers often use the SharePoint managed metadata feature to setup their enterprise metadata and when configuring your SharePoint site it's often needed to use fields that provide a lookup to a given termset. These type of fields are called Taxonomy fields and they can be configured via the [FieldTaxonomyOptions class](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldTaxonomyOptions.html). You're required to provide the id's for a [TermStore](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldTaxonomyOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldTaxonomyOptions_TermStoreId) and [TermSet](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.FieldTaxonomyOptions.html#collapsible-PnP_Core_Model_SharePoint_FieldTaxonomyOptions_TermSetId).

```csharp
IField myField = await myList.Fields.AddTaxonomyAsync("My Field", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
});
```

When creating taxonomy fields you might also want to set default term, which can be done via the `DefaultValue` property. When you want to allow users to add new terms to the connected termset you can define the termset as open via the `OpenTermSet` property. Setting a termset as open only is possible if the respective termset has an open submission policy.

```csharp
// Load the first termset under the System group
var termStore = await context.TermStore.GetAsync(t => t.Groups);
var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
await group.LoadAsync(g => g.Sets);
var termSet = group.Sets.AsRequested().FirstOrDefault();

// Load the terms in this termset
await termSet.LoadAsync(p => p.Terms);
ITerm term1 = termSet.Terms.AsRequested().First();

// Create the taxonomy field as open with a default value
IField myField = await myList.Fields.AddTaxonomyAsync("My Field", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid(termStore.Id),
    TermSetId = new Guid(termSet.Id),
    DefaultValue = term1,
    OpenTermSet = true
});
```

If you want to update the default value of an existing taxonomy field then you need to create the default value string according to below sample:

```csharp
myField.DefaultValue = $"-1;#{term1.Labels.First(p => p.IsDefault == true).Name}|{term1.Id}";
await myField.UpdateAsync();
```

## Multi taxonomy

If you want to be able to select multiple taxonomy values to fill your field you need a multi taxonomy field, configuration wise this is the same as for the single value Taxonomy field but now you'd use the [AddTaxonomyMultiAsync method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldCollection.html#collapsible-PnP_Core_Model_SharePoint_IFieldCollection_AddTaxonomyMulti_System_String_PnP_Core_Model_SharePoint_FieldTaxonomyOptions_).

```csharp
IField myField = await myList.Fields.AddTaxonomyMultiAsync("My Field", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid("437b86fc-1258-45a9-85ea-87a29156ce3c"),
    TermSetId = new Guid("d50ec969-cb27-4a49-839f-3c25d1d607d5")
});
```

When creating taxonomy fields you might also want to set default terms, which can be done via the `DefaultValues` property. When you want to allow users to add new terms to the connected termset you can define the termset as open via the `OpenTermSet` property. Setting a termset as open only is possible is the respective termset is created as open termset.

```csharp
// Load the first termset under the System group
var termStore = await context.TermStore.GetAsync(t => t.Groups);
var group = termStore.Groups.AsRequested().FirstOrDefault(g => g.Name == "System");
await group.LoadAsync(g => g.Sets);
var termSet = group.Sets.AsRequested().FirstOrDefault();

// Load the terms in this termset
await termSet.LoadAsync(p => p.Terms);
ITerm term1 = termSet.Terms.AsRequested().First();
ITerm term2 = termSet.Terms.AsRequested().Last();

// Create the taxonomy field as open with a default values
IField myField = await myList.Fields.AddTaxonomyMultiAsync("My Field", new FieldTaxonomyOptions()
{
    Group = "Custom Fields",
    AddToDefaultView = true,
    TermStoreId = new Guid(termStore.Id),
    TermSetId = new Guid(termSet.Id),
    DefaultValues = new System.Collections.Generic.List<ITerm>() { term1, term2 },
    OpenTermSet = true
});
```

If you want to update the default value of an existing taxonomy field then you need to create the default value string according to below sample:

```csharp
myField.DefaultValue = $"-1;#{term1.Labels.First(p => p.IsDefault == true).Name}|{term1.Id};#-1;#{term3.Labels.First(p => p.IsDefault == true).Name}|{term3.Id}";
await myField.UpdateAsync();
```

## Add field via field XML

While above options should cover majority of the field creation needs it might happen that you need to be able to configure additional options and for that there's a solution allowing you to specify the field XML that will create the field yourselve. Adding the field is done via the [AddFieldAsXml methods](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IFieldCollection.html#PnP_Core_Model_SharePoint_IFieldCollection_AddFieldAsXmlAsync_System_String_System_Boolean_PnP_Core_Model_SharePoint_AddFieldOptionsFlags_). You can learn more about the possible options for [fields](https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/ms437580(v=office.14)) to understand how the field XML needs to be crafted.

```csharp
IField myField = await myList.Fields.AddFieldAsXmlAsync(@"<Field Type=""Text"" Name=""ADDEDFIELD"" DisplayName=""ADDED FIELD""/>", true);
```
