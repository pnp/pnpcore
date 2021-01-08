# Getting data from Microsoft 365

Loading data (e.g. a SharePoint list or a Teams channel) is usually needed when you write applications using the PnP Core SDK. There are different methods to load data, ones that data for a given model instance (e.g. an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html)), but also ones that populate a collection of model instances (e.g. an [IListCollection](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IListCollection.html))

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for getting data
}
```

## Model load basics

To load model data you need to use one of the Get methods on the model. The following Get methods exist on each model, allowing you to perform a direct query or add a query to a batch for a grouped execution (see the [batch article](basics-batching.md)).

- [GetAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelGet-1.html#PnP_Core_Model_IDataModelGet_1_GetAsync_Expression_Func__0_System_Object_____)
- [Get](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelGet-1.html#collapsible-PnP_Core_Model_IDataModelGet_1_Get_Expression_Func__0_System_Object_____)
- [GetBatchAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelGet-1.html#collapsible-PnP_Core_Model_IDataModelGet_1_GetBatchAsync_Expression_Func__0_System_Object_____)
- [GetBatch](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelGet-1.html#collapsible-PnP_Core_Model_IDataModelGet_1_GetBatch_PnP_Core_Services_Batch_Expression_Func__0_System_Object_____)

These methods allow you to perform a default load or a controlled load in which you specify which properties you want to get loaded. Doing a controlled load allows you to only retrieve what your applications needs and typically is faster compared to doing a default load.

> [!Note]
>
> - When loading data the SDK will automatically load the primary key property of a model, even if you've not requested that when doing a controlled load.
> - When you create a `PnPContext` the SDK already loads the relevant [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) and [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) properties of the `PnPContext`. When loading these [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html) and [ISite](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISite.html) properties, a controlled load is done to only retrieve what's needed to support the internal workings of the SDK.

Below sample shows some of the above discussed data loads.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // The Id property of the IWeb was loaded when the context was created
    var webId = context.Web.Id;

    // Do a default load of the IWeb
    await context.Web.GetAsync();

    // Do a controlled load of the IWeb, only the web title is loaded
    await context.Web.GetAsync(p => p.Title);

    // Do a controlled load of the IWeb, only the web title and all the lists in the web are loaded
    await context.Web.GetAsync(p => p.Title, p => p.Lists);

    foreach (var list in context.Web.Lists)
    {
        // do something with the list
    }
}
```

When you're loading data then the domain model is populated with the loaded data, but you'll also get a reference back to the domain model instance effectively giving you two ways to work with the loaded data:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: use the domain model
    await context.Web.GetAsync(p => p.Title, p => p.Lists);

    foreach (var list in context.Web.Lists)
    {
        // do something with the list
    }

    // Option B: use the returned reference to the domain model
    var web = await context.Web.GetAsync(p => p.Title, p => p.Lists);

    foreach (var list in web.Lists)
    {
        // do something with the list
    }
}
```

## Model collection load basics

Previous chapter showed how to load data starting from a single model (e.g. loading the [Title](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_Title) property of [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html)), but what if you need to load the lists of a web? One approach for loading a model collection is loading the full collection via loading the relevant parent domain model property (e.g. loading the [Lists](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_Lists) property on the [IWeb](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html)) as shown in previous chapter. This approach however does not allow you to apply a filter to reduce the data being returned and therefore it can be better to use one of the specific collection load methods:

- [GetAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetAsync_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [Get](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_Get_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetBatchAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetBatchAsync_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetBatch](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetBatch_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetFirstOrDefaultAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetFirstOrDefaultAsync_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetFirstOrDefault](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetFirstOrDefault_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetFirstOrDefaultBatchAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetFirstOrDefaultBatchAsync_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)
- [GetFirstOrDefaultBatch](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#collapsible-PnP_Core_Model_IDataModelCollection_1_GetFirstOrDefaultBatch_Expression_Func__0_System_Boolean___Expression_Func__0_System_Object_____)

These methods do allow you to perform either a default load or a controlled load allowing you to specify which properties of the collection model instances (e.g. IList instances in the IListCollection) need to be fetched and what filter must be applied to reduce the number of returned model instances. The GetFirstOrDefault methods only return a single model instance of the collection. These methods create either a direct query or add a query to a batch for a grouped execution (see the [batch article](basics-batching.md)).

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: Load the Lists using a model load => no filtering option
    await context.Web.GetAsync(p => p.Title, p => p.Lists);

    // Option B: Load the Lists using the model collection load ==> filtering is possible,
    // only lists with title "Site Pages" are returned
    await context.Web.Lists.GetAsync(p => p.Title == "Site Pages");

    // Option C: we assume there's only one list with that title so we can use GetFirstOrDefaultAsync
    await context.Web.Lists.GetFirstOrDefaultAsync(p => p.Title == "Site Pages");
}
```

> [!Note]
> When you only need one model instance from a collection it's recommended to use the GetFirstOrDefault methods as they only return the data for one model instance and offer better performance.

Like with loading the model in the previous chapter you've two ways of using the data, although that for model collection loads there are some differences:

- Only regular data loads have a return value, batch methods don't
- When a collection is returned (GetAsync, Get, GetBatchAsync and GetBatch) the collection is an IEnumerable

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Option A: Use the lists from the model, not that previously 
    // loaded lists can still be in the domain model
    await context.Web.Lists.GetAsync(p => p.Title == "Site Pages");

    foreach(var list in context.Web.Lists)
    {
        // Use list
    }

    // Option B: Use the lists from the returned collection, you're 
    // only working with the effectively returned lists
    var lists = await context.Web.Lists.GetAsync(p => p.Title == "Site Pages");

    foreach(var list in lists)
    {
        // Use list
    }
}
```

## Loading additional data in a single request

When you perform a model or model collection load you can do a controlled load specifying the model properties to return, but sometimes you want to also specify what to return from a returned property (e.g. you're loading a list and want to also load the content types and their field links). The PnP Core SDK supports these type of additional data loads via the [LoadProperties method](https://pnp.github.io/pnpcore/api/PnP.Core.Model.IDataModelCollection-1.html#PnP_Core_Model_IDataModelCollection_1_LoadProperties_Expression_Func__0_System_Object_____). This method can be used with model data loads as well as with model collection data loads as shown here:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Load the Site with the RootWeb model populated with the Title, NoCrawl and List properties,
    //   for each list load the the Title property
    await context.Site.GetAsync(p => p.RootWeb.LoadProperties(p => p.Title, p => p.NoCrawl,
                                                              p => p.Lists.LoadProperties(p => p.Title)));

    // Loads all lists with 
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    await context.Web.Lists.GetAsync(p => p.Title, p => p.TemplateType,
                                     p => p.ContentTypes.LoadProperties(
                                        p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));

    // Loads all document libraries
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    await context.Web.Lists.GetAsync(p => p.TemplateType == ListTemplateType.DocumentLibrary,
                                     p => p.Title, p => p.TemplateType,
                                     p => p.ContentTypes.LoadProperties(
                                          p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));

    // Loads the first hidden document library
    //   their content types controlled loaded and 
    //     for each content type the field links are controlled loaded 
    //       with the name property
    await context.Web.Lists.GetFirstOrDefaultAsync(p => p.TemplateType == ListTemplateType.DocumentLibrary && Hidden == true,
                                                   p => p.Title, p => p.TemplateType,
                                                   p => p.ContentTypes.LoadProperties(
                                                        p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));
}
```

> [!Important]
> When the API uses Microsoft Graph (e.g. when working with Microsoft Teams), you cannot use nested LoadProperties statements and you can't load a given properties that depend on a separate query to work. At that point you'll see a [ClientException](https://pnp.github.io/pnpcore/api/PnP.Core.ClientException.html) being thrown with as [ErrorType Unsupported](https://pnp.github.io/pnpcore/api/PnP.Core.ErrorType.html#PnP_Core_ErrorType_Unsupported).

## What about those GetByxxx methods?

In the domain model you quite often see a GetByxxx method (e.g. GetByTitleAsync and GetByIdAsync on an IList): these methods are simply shorthands for their respective GetFirstOrDefaultAsync method calls. There's no functional difference between both approaches and you can use whatever "feels" the best for you.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Using this GetByTitle method is... 
    await context.Web.Lists.GetByTitleAsync("Site Pages", 
                                            p => p.Title, p => p.TemplateType,
                                            p => p.ContentTypes.LoadProperties(
                                                 p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));

    // ...identical to this GetFirstOrDefaultAsync call
    await context.Web.Lists.GetFirstOrDefaultAsync(p => p.Title == "Site Pages",
                                                   p => p.Title, p => p.TemplateType,
                                                   p => p.ContentTypes.LoadProperties(
                                                        p => p.Name, p => p.FieldLinks.LoadProperties(p => p.Name)));
}
```

## Cascading data loads

Depending on the scenario you might need to perform two or more data loads to reach the model instance you need (e.g. you need to load the list before you can get the list items). You can write the code for these cascading loads in multiple ways, but each of the approaches results in two queries being sent.

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Async approach: recommended

    // Option A: using an intermediate variable
    IList list = await context.Web.Lists.GetByTitleAsync("Site Pages");
    IItem item = await list.Items.GetByIdAsync(1);

    // Option B: using the AndThen method to chain async method calls
    IItem item = await context.Web.Lists.GetByTitleAsync("Site Pages").AndThen(p => p.Items.GetByIdAsync(1));

    // Option C: using multiple awaits
    IItem item = await (await context.Web.Lists.GetByTitleAsync("Site Pages")).Items.GetByIdAsync(1);

    // Synchronous approach
    IItem item = context.Web.Lists.GetByTitle("Site Pages").Items.GetById(1);
}
```
