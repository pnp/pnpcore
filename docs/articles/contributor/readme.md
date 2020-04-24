# Extending the PnP Microsoft 365 model

The model in PnP Core SDK is what the SDK users use to interact with Microsoft 365: it defines the model classes (e.g. List), their fields (Title, Description,...) and the their operations (e.g. Get). This model has a public part (interfaces) and an implementation (internal, partial, classes). In order to translate the model into respective SharePoint REST and/or Microsoft Graph queries the model needs to be decorated with attributes. These attributes drive the needed API calls to Microsoft 365 and the serialization of returned responses (JSON) into the model.

## General model principles

The model design principles are agnostic to whether the model will be populated via a SharePoint REST or Microsoft Graph call and therefore starting here to understand the general model principles is advised. Once you understand the model design principles you can learn more about how to decorate the model to work with either SharePoint REST and/or Microsoft Graph. Below picture gives an overview of the used classes in the model based up on the Team model implementation:

![Model overview](../../images/model%20overview.png)

In the model there are 3 types of classes:

- The majority of the model is built from **model classes**
- Model classes typically use simple .Net types or enumerations as type for their properties, but sometimes a complex type is needed which is represented via a **complex type class**
- Model classes often live in a collection, so we do have **model collection classes**

Each of these classes has a public model implemented via interfaces and an internal model implemented via internal partial classes.

### Model classes

The model classes are the most commonly used classes in our domain model as they represent a Microsoft 365 object that can be queried via either the SharePoint REST or the Microsoft Graph interface. Samples of model classes are Web, Team, List,...

#### Public model

The public model is build via public interfaces. Below sample shows the public model for a SharePoint List

```csharp
/// <summary>
/// Public interface to define a List object of SharePoint Online
/// </summary>
public interface IList : IDataModel<IList>, IDataModelUpdate, IDataModelDelete
{
    /// <summary>
    /// The Unique ID of the List object
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the list title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the list
    /// </summary>
    public string Description { get; set; }

    // Other fields left for brevity
}
```

Each public model:

- Uses a public interface (e.g. `IList` in our example) with public fields
- Has inline documentation on the model class and fields
- Always implements the `IDataModel<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample)
- Optionally implements the `IDataModelUpdate` interface whenever **update** functionality in needed on this model class
- Optionally implements the `IDataModelDelete` interface whenever **delete** functionality is needed on this model class

The fields in the model use either basic .Net data types, enumerations, other model/collection types or so called complex types:

```csharp
// Simple .Net type
public string Title { get; set; }

// Enum
public ListReadingDirection Direction { get; set; }

// Other model/collection types
public IListItemCollection Items { get; }

// Complex types (sample comes from the Team model class)
public ITeamFunSettings FunSettings { get; set; }
```

#### Internal implementation

The internal model implementation is what brings the public model to life: this split approach ensures that library consumers only work off the public model and as such the library implementation can be updated without breaking the public contract with library consumers. For the internal model class implementation we've opted to use internal partial classes:

- A `Model.gen.cs` class for semi-generated model class code
- A `Model.cs` class for coded model class code

Here's a snippet of the `List.gen.cs` class:

```csharp
internal partial class List : BaseDataModel<IList>, IList
{
    [SharePointFieldMapping(IsKey = true)]
    [GraphFieldMapping(FieldName = "id", IsKey = true)]
    public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

    [GraphFieldMapping(FieldName = "displayName")]
    public string Title { get => GetValue<string>(); set => SetValue(value); }

    [GraphFieldMapping(FieldName = "description")]
    public string Description { get => GetValue<string>(); set => SetValue(value); }
    
    // Other fields left for brevity

    public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
}
```

Each generated model class:

- Inherits from the `BaseDataModel<TModel>` class and implements `TModel`
- Is an **internal**, **partial** class
- Has public properties that use the `GetValue` and `SetValue` inherited methods to get and set property values
- Has a `Key` property override which can be used to set/get the key value. The Key is used to organize objects in collections
- Has property attributes that are used to define the requests to Microsoft 365 and serialization of the received data. These attributes are explained in more detail in their respective chapters later on

Here's a snippet of the `List.cs` class:

```csharp
[ClassMapping(SharePointType = "SP.List",
              SharePointUri = "_api/Web/Lists(guid'{Id}')",
              SharePointGet = "_api/web/lists",
              SharePointUpdate = "_api/web/lists/getbyid(guid'{Id}')",
              GraphId = "id",
              GraphGet = "sites/{Parent.GraphId}/lists/{GraphId}")]
internal partial class List
{
    internal List()
    {
        MappingHandler = (FromJson input) =>
        {
            // Handle the mapping from json to the domain model for the cases which are not generically handled
            switch (input.TargetType.Name)
            {
                case "ListExperience": return ToEnum<ListExperience>(input.JsonElement);
                case "ListReadingDirection": return ToEnum<ListReadingDirection>(input.JsonElement);
            }

            input.Log.LogWarning($"Field {input.FieldName} could not be mapped when converting from JSON");

            return null;
        };

        // Handler to construct the Add request for this list
        AddApiCallHandler = () =>
        {
            return new ApiCall($"_api/web/lists", ApiType.Rest, JsonSerializer.Serialize(new ListAdd(this, TemplateType, Title)));
        };
    }
}
```

Each coded model class:

- Is an **internal**, **partial** class
- Does not inherit from another class (the inheriting is done in the `Model.gen.cs` partial class)
- Contains class level attributes that are used to define the requests to Microsoft 365 and serialization of the received data. These attributes are explain in more detail in their respective chapters later on
- Can implement event handlers which are used to (see the [Event Handlers](event%20handlers.md) page for more details):

  - Optionally customize the JSON to Model mapping via the `MappingHandler = (FromJson input)` handler
  - Implement the API call for doing an Add operation via the `AddApiCallHandler = ()` handler
  - Optionally implement API call overrides that allow you to update the generated API call before it's send off to the server. There are these handlers: `GetApiCallOverrideHandler = (ApiCall apiCall)`, `UpdateApiCallOverrideHandler = (ApiCall apiCall)` and `DeleteApiCallOverrideHandler = (ApiCall apiCall)`
  - Optionally implement property validation (prevent property updates, alter values) via the `ValidateUpdateHandler = (ref FieldUpdateRequest fieldUpdateRequest)` handler

- Model specific methods can be foreseen. These methods provide additional operations on the model class

### Complex type classes

Complex type classes are used to represent types which are too complex for simple .Net type or enumeration, but on the other hand not complex enough to be queried independently via an API call. A good example of this are the `TeamFunSettings`: when querying for a `Team` you'll get the `TeamFunSettings` in the response, but there's no API to directly query `TeamFunSettings` as such.

#### Public model

Like with all our public models, the complex type classes also use interfaces.

```csharp
/// <summary>
/// Public interface to define the fun settings for a Team
/// </summary>
public interface ITeamFunSettings: IComplexModel
{
    /// <summary>
    /// Defines whether the Giphy are allowed in the Team
    /// </summary>
    public bool AllowGiphy { get; set; }

    /// <summary>
    /// Defines the Giphy content rating (strict or moderate)
    /// </summary>
    public TeamGiphyContentRating GiphyContentRating { get; set; }

    /// <summary>
    /// Defines whether the stickers and meme are allowed in the Team
    /// </summary>
    public bool AllowStickersAndMemes { get; set; }

    /// <summary>
    /// Defines whether the custom memes are allowed in the Team
    /// </summary>
    public bool AllowCustomMemes { get; set; }
}

/// <summary>
/// Giphy content rating for giphies being used in a team
/// </summary>
public enum TeamGiphyContentRating
{
    Moderate,
    Strict
}
```

Each public model:

- Uses a public interface (e.g. `ITeamFunSettings` in our example) with public properties
- Has inline documentation on the class and properties
- Always implements the `IComplexModel` interface

The properties in the model use either basic .Net data types or enumerations:

```csharp
// Basic .Net type
public bool AllowGiphy { get; set; }

// Enumeration
public TeamGiphyContentRating GiphyContentRating { get; set; }
```

#### Internal implementation

For the internal complex type class implementation we've opted to use an internal partial class:

- A `ComplexType.gen.cs` class for semi-generated complex type class code

Here's a snippet of the `TeamFunSettings.gen.cs` class:

```csharp
[ClassMapping]
internal partial class TeamFunSettings : BaseComplexTypeModel<ITeamFunSettings>, ITeamFunSettings
{

    public TeamFunSettings()
    {
        MappingHandler = (FromJson input) =>
        {
            switch (input.TargetType.Name)
            {
                case "TeamGiphyContentRating": return ToEnum<TeamGiphyContentRating>(input.JsonElement);
            }

            input.Log.LogWarning($"Field {input.FieldName} could not be mapped when converting from JSON");

            return null;
        };
    }

    public bool AllowGiphy { get => GetValue<bool>(); set => SetValue(value); }
    public TeamGiphyContentRating GiphyContentRating { get => GetValue<TeamGiphyContentRating>(); set => SetValue(value); }
    public bool AllowStickersAndMemes { get => GetValue<bool>(); set => SetValue(value); }
    public bool AllowCustomMemes { get => GetValue<bool>(); set => SetValue(value); }
}
```

Each generated complex model class:

- Inherits from the `BaseComplexTypeModel<TModel>` class and implements `TModel`
- Is an **internal**, **partial** class
- Has the `ClassMapping` class attribute
- Has public properties that use the `GetValue` and `SetValue` inherited methods to get and set property values
- Possibly has property attributes that are used to define the requests to Microsoft 365 and serialization of the received data. These attributes are explained in more detail in their respective chapters later on
- Can implement event handlers which are used to (see the [Event Handlers](event%20handlers.md) page for more details):

  - Optionally customize the JSON to Model mapping via the `MappingHandler = (FromJson input)` handler
  - Optionally implement property validation (prevent property updates, alter values) via the `ValidateUpdateHandler = (ref PropertyUpdateRequest propertyUpdateRequest)` handler

### Collection classes

Collection classes contain zero or more model class instances, so for example the `ListCollection` will contain zero or more `List` model class instances.

#### Public model

The public model is build via public interfaces. Below sample shows the public model for a SharePoint ListCollection

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

    // Other methods ommitted for brevity
}
```

Each public model:

- Uses a public interface (e.g. `IListCollection` in our example) with optionally public methods
- Has inline documentation on the model class and methods
- Always implements the `IDataModelCollection<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample)

Optionally a collection interface defines methods which add behavior.

#### Internal implementation

For the internal collection class implementation we've opted to use internal partial classes:

- A `Collection.gen.cs` class for semi-generated collection class code
- A `Collection.cs` class for coded collection class code

Here's a snippet of the `ListCollection.gen.cs` class:

```csharp
internal partial class ListCollection : QueryableDataModelCollection<IList>, IListCollection
{

    [Browsable(false)]
    public override IList Add()
    {
        return AddNewList();
    }

    [Browsable(false)]
    internal override IList New()
    {
        return NewList();
    }

    private List AddNewList()
    {
        var newList = NewList();
        this.items.Add(newList);
        return newList;
    }

    private List NewList()
    {
        var newList = new List
        {
            PnPContext = this.PnPContext,
            Parent = this,
        };
        return newList;
    }
}
```

Each generated collection class:

- Inherits from the `QueryableDataModelCollection<TModel>` class and implements the previously created collection interface (e.g. `IListCollection`)
- Is an **internal**, **partial** class
- Overrides the `CreateNew()` base class methods

Here's a snippet of the `ListCollection.cs` class:

```csharp
internal partial class ListCollection
{
    // Other methods ommitted for brevity

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

    // Other methods ommitted for brevity
}
```

Each coded collection class:

- Is an **internal**, **partial** class
- Does not inherit from another class (the inheriting is done in the `Collection.gen.cs` partial class)
- Contains the implementation of the methods defined in the public interface

## Decorating the model

The model, collections and complex type classes you create can be populated via either SharePoint REST queries, Microsoft Graph queries or both. Depending on the needed query approach you'll need to decorate the model class and/or fields with properties. **It's these properties that drive the automatic query generation**.

When you populate your model via SharePoint REST queries then continue [here](extending%20the%20model%20-%20SharePoint%20REST.md), in case the model is populated via Microsoft Graph continue [here](extending%20the%20model%20-%20Microsoft%20Graph.md).
