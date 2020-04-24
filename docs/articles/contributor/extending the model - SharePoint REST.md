
# Extending the model via SharePoint REST

The PnP Core SDK model contains model, collection and complex type classes which are populated via either Microsoft Graph or SharePoint REST. In this chapter you'll learn more on how to decorate your classes and their properties to interact with Microsoft 365 via the SharePoint REST API.

## Configuring model classes

### Class decoration

Each model class does need to have a `ClassMapping` attribute which is defined on the coded model class (e.g. List.cs):

```csharp
[ClassMapping(SharePointType = "SP.List",
              SharePointUri = "_api/Web/Lists(guid'{Id}')",
              SharePointGet = "_api/web/lists",
              SharePointUpdate = "_api/web/lists/getbyid(guid'{Id}')")]
internal partial class List
{
    // Ommitted for brevity
}
```

When configuring the `ClassMapping` attribute for SharePoint REST you need to set attribute properties:

Property | Required | Description
---------|----------|------------
SharePointType | Yes | Defines the SharePoint REST type that maps with the model class. Each model that requires SharePoint REST requires this attribute
SharePointUri | Yes | Defines the URI that uniquely identifies this object. See [model tokens](model%20tokens.md) to learn more about the possible tokens you can use
SharePointGet | No | Overrides the SharePointUri property for **get** operations.
SharePointUpdate | No | Overrides the SharePointUri property for **update** operations.
SharePointDelete | No | Overrides the SharePointUri property for **delete** operations.
SharePointOverflowFieldName | No | Used when working with a dynamic property/value pair (e.g. fields in a SharePoint ListItem) whenever the SharePoint REST field containing these dynamic properties is not named `Values`

### Property decoration

The property level decoration is done using the `SharePointFieldMapping` attribute. For most properties you do not need to set this attribute, it's only required for special cases. Since the properties are defined in the generated model class (e.g. List.gen.cs) the decoration via attributes needs to happen in this class as well.

```csharp
// Configure the SharePoint REST field used to populate this model property
[SharePointFieldMapping(FieldName = "DocumentTemplateUrl")]
public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

// Mark the property that serves as Key field (used to ensure there are no duplicates in collections)
[SharePointFieldMapping(IsKey = true)]
public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

// Define a collection as expandable
[SharePointFieldMapping(Expandable = true)]
public IListItemCollection Items
{
    get
    {
        if (!HasValue(nameof(Items)))
        {
            var items = new ListItemCollection
            {
                PnPContext = this.PnPContext,
                Parent = this
            };
            SetValue(items);
        }
        return GetValue<IListItemCollection>();
    }
}
```

You can set following properties on this attribute:

Property | Required | Description
---------|----------|------------
FieldName | No | Use this property when the SharePoint REST fieldname differs from the model property name
IsKey | No | Marks the model property as holding a unique value. This value is used to ensure no duplicate model class instances are loaded in collection classes, if the model class has a unique property then that property should be decorated
JsonPath | No | When the information returned from SharePoint REST is a complex type and you only need a single value from it, then you can specify the JsonPath for that value. E.g. when you get sharePointIds.webId as response you tell the model that the fieldname is sharePointIds and the path to get there is webId. The path can be more complex, using a point to define property you need (e.g. property.child.childofchild)
Expandable | No | Defines that a collection is expandable, meaning it can be loaded via the $expand query parameter and used in the lambda expression in `Get` and `GetAsync` operations
ExpandByDefault | No | When the model contains a collection of other model objects then setting this attribute to true will automatically result in the population of that collection. This can negatively impact performance, so only set this when the collection is almost always needed
UseCustomMapping | No | Allows you to force a callout to the model's `MappingHandler` event handler whenever this property is populated. See the [Event Handlers](event%20handlers.md) article to learn more

## Configuring complex type classes

Complex type classes are not used when the model is populated via SharePoint REST.

## Configuring collection classes

Collection classes **do not** have attribute based decoration.

## Implementing "Add" functionality

In contradiction with get, update and delete which are fully handled by decorating classes and properties using attributes, you'll need to write actual code to implement add. Adding is implemented as follows:

- The public part (interface) is defined on the collection interface. Each functionality (like Add) is implemented via three methods:

  - An async method
  - A regular method
  - A regular method that allows to pass in a `Batch` as first method parameter

- Add methods defined on the interface are implemented in the collection classes as a proxies that call into the respective add methods of the added model class
- The implementation that performs the actual add is implemented as an `AddApiCallHandler` event handler in the model class. See the [Event Handlers](event%20handlers.md) page for more details.

Below code snippets show the above three concepts. First one shows the collection interface (e.g. IListCollection.cs) with the Add methods:

```csharp
/// <summary>
/// Public interface to define a collection of List objects of SharePoint Online
/// </summary>
public interface IListCollection : IDataModelCollection<IList>
{
    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public Task<IList> AddAsync(string title, int templateType);

    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="batch">Batch to use</param>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public IList Add(Batch batch, string title, int templateType);

    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public IList Add(string title, int templateType);
}
```

Implementation of the interface in the coded collection class (e.g. ListCollection.cs):

```csharp
internal partial class ListCollection
{
    public IList Add(string title, int templateType)
    {
        return Add(PnPContext.CurrentBatch, title, templateType);
    }

    public IList Add(Batch batch, string title, int templateType)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (templateType == 0)
        {
            throw new ArgumentException($"{nameof(templateType)} cannot be 0");
        }

        var newList = AddNewList();

        newList.Title = title;
        newList.TemplateType = templateType;

        return newList.Add(batch) as List;
    }

    public async Task<IList> AddAsync(string title, int templateType)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (templateType == 0)
        {
            throw new ArgumentException($"{nameof(templateType)} cannot be 0");
        }

        var newList = AddNewList();

        newList.Title = title;
        newList.TemplateType = templateType;

        return await newList.AddAsync().ConfigureAwait(false) as List;
    }
}
```

And finally you'll see the actual add logic being implemented in the coded model class (e.g. List.cs) via implementing the `AddApiCallHandler`:

```csharp
internal partial class List
{
    /// <summary>
    /// Class to model the Rest List Add request
    /// </summary>
    internal class ListAdd: RestBaseAdd<IList>
    {
        public int BaseTemplate { get; set; }

        public string Title { get; set; }

        internal ListAdd(BaseDataModel<IList> model, int templateType, string title) : base(model)
        {
            BaseTemplate = templateType;
            Title = title;
        }
    }

    internal List()
    {
        // Handler to construct the Add request for this list
        AddApiCallHandler = () =>
        {
            return new ApiCall($"_api/web/lists", ApiType.Rest, JsonSerializer.Serialize(new ListAdd(this, TemplateType, Title)));
        };
    }
}
```
