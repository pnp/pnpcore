# The PnP Core SDK model

The model in PnP Core SDK is what the SDK users use to interact with Microsoft 365: it defines the model classes (e.g. List), their fields (Title, Description,...) and the their operations (e.g. Get). This model has a public part (interfaces) and an implementation (internal, partial, classes). In order to translate the model into respective SharePoint REST and/or Microsoft Graph v1.0 or beta queries the model needs to be decorated with attributes. These attributes drive the needed API calls to Microsoft 365 and the serialization of returned responses (JSON) into the model. **As a contributor extending and enriching the model is how you provide functionality to the developers that will be using this SDK**.

![SDK overview](../../images/sdk%20overview.png)

## General model principles

The model design principles are agnostic to whether the model will be populated via a SharePoint REST or Microsoft Graph call and therefore starting here to understand the general model principles is advised. Once you understand the model design principles you can learn more about how to decorate the model to work with either SharePoint REST and/or Microsoft Graph. Below picture gives an overview of the used classes in the model based up on the Team model implementation:

![Model overview](../../images/model%20overview.png)

In the model there are 3 types of classes:

- The majority of the model is built from **model classes**
- Model classes typically use simple .Net types or enumerations as type for their properties, but sometimes a complex type is needed which is represented via a **complex type class**
- Model classes often live in a collection, so we do have **model collection classes**

A special case is **complex type** collections, they can be used in collections and the collection will then be a regular .Net `List` of **model collection classes**.

Each of these classes has a public model implemented via interfaces and an internal model implemented via internal partial classes.

### Model classes

The model classes are the most commonly used classes in our domain model as they represent a Microsoft 365 object that can be queried via either the SharePoint REST or the Microsoft Graph interface. Samples of model classes are Web, Team, List,...

#### Public model

The public model is build via public interfaces. Below sample shows the public model for a SharePoint List

```csharp
/// <summary>
/// Public interface to define a List object of SharePoint Online
/// </summary>
[ConcreteType(typeof(List))]
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

    // Other properties left for brevity
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
    public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

    [GraphProperty("displayName")]
    public string Title { get => GetValue<string>(); set => SetValue(value); }

    [GraphProperty("description")]
    public string Description { get => GetValue<string>(); set => SetValue(value); }

    // Other properties left for brevity

    [KeyProperty("Id")]
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
[SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Get = "_api/web/lists", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
[GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}")]
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
                case "ListTemplateType": return JsonMappingHelper.ToEnum<ListTemplateType>(input.JsonElement);
            }

            input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

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
public interface ITeamFunSettings: IComplexType
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
- Always implements the `IComplexType` interface

The properties in the model use either basic .Net data types, other **complex types** or enumerations:

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
[GraphType]
internal partial class TeamFunSettings : BaseComplexType<ITeamFunSettings>, ITeamFunSettings
{

    public TeamFunSettings()
    {
        MappingHandler = (FromJson input) =>
        {
            switch (input.TargetType.Name)
            {
                case "TeamGiphyContentRating": return ToEnum<TeamGiphyContentRating>(input.JsonElement);
            }

            input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

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

- Inherits from the `BaseComplexType<TModel>` class and implements `TModel`
- Is an **internal**, **partial** class
- Has the `GraphType` and/or the `SharePointType` class attribute
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
public interface IListCollection : IDataModelCollection<IList>, IQueryable<IList>, ISupportPaging
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
- Optionally implements the `IQueryable<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample) whenever the model can be queried using linq queries
- Optionally implements the `ISupportPaging` interface whenever the data in the collection can be retrieved from the server via paging

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

- Inherits from either the `BaseDataModelCollection<TModel>` for regular collections or from the `QueryableDataModelCollection<TModel>` class for linq queriable collections and implements the previously created collection interface (e.g. `IListCollection`)
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

### Complex type collections

Complex type classes are used to represent types which are too complex for simple .Net type or enumeration, but on the other hand not complex enough to be queried independently via an API call. Sometimes you need a collection of complex type classes, which can be released via the `List` .Net collection class.

#### Public model

The complex type collection is `List` of complex types as shown in below example. Properties that are a complex type collection should only support a property `get`.

```csharp
public interface ITeamChatMessage : IDataModel<ITeamChatMessage>
{
    // Other properties left for brevity

    /// <summary>
    /// Reactions for this chat message (for example, Like).
    /// </summary>
    public List<ITeamChatMessageReaction> Reactions { get; }

    /// <summary>
    /// List of entities mentioned in the chat message. Currently supports user, bot, team, channel.
    /// </summary>
    public List<ITeamChatMessageMention> Mentions { get; }

    // Other properties left for brevity
}
```

#### Internal implementation

For the internal complex type class collection implementation you need to update the generated partial class of the model class having the collection:

- A `Model.gen.cs` class for semi-generated complex type class code

Here's a snippet of the `TeamChatMessage.gen.cs` class:

```csharp
internal partial class TeamChatMessage : BaseDataModel<ITeamChatMessage>, ITeamChatMessage
{
    // Other properties left for brevity

    public List<ITeamChatMessageReaction> Reactions
    {
        get
        {
            if (!HasValue(nameof(Reactions)))
            {
                SetValue(new List<ITeamChatMessageReaction>());
            }
            return GetValue<List<ITeamChatMessageReaction>>();
        }
    }

    public List<ITeamChatMessageMention> Mentions
    {
        get
        {
            if (!HasValue(nameof(Mentions)))
            {
                SetValue(new List<ITeamChatMessageMention>());
            }
            return GetValue<List<ITeamChatMessageMention>>();
        }
    }

    // Other properties left for brevity
}
```

Each generated complex model class that contains complex type collections:

- Uses a `List<>` of the complex type class
- Implements the getter as shown in the example. It's important that the `HasValue` and `SetValue` methods are used to ensure the change tracking can detect changed values

## Decorating the model

The model, collections and complex type classes you create can be populated via either SharePoint REST queries, Microsoft Graph queries or both. Depending on the needed query approach you'll need to decorate the model class and/or fields with properties. **It's these properties that drive the automatic query generation**.

When you populate your model via SharePoint REST queries then continue [here](extending%20the%20model%20-%20SharePoint%20REST.md), in case the model is populated via Microsoft Graph continue [here](extending%20the%20model%20-%20Microsoft%20Graph.md).
