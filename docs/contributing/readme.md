# The PnP Core SDK model

The model in PnP Core SDK is what the SDK users use to interact with Microsoft 365: it defines the model classes (e.g. List), their fields (Title, Description,...) and their operations (e.g. GetAsync or Get). This model has a public part (interfaces) and an implementation (internal, partial classes). In order to translate the model into respective SharePoint REST and/or Microsoft Graph v1.0 or beta queries the model needs to be decorated with attributes. These attributes drive the needed API calls to Microsoft 365 and the serialization of returned responses (JSON) into the model. **As a contributor, extending and enriching the model is how you provide functionality to the developers that will be using this SDK**.

![SDK overview](../images/sdk%20overview.png)

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore. You'll find:

- The code of the PnP Core SDK in the `src\sdk` folder
- Examples of how to use the PnP Core SDK in the `src\samples` folder
- Generated code to speed up contribution of new model logic in the `src\generated` folder
- The source of the documentation you are reading right now in the `docs` folder

## Setting up your environment for building the PnP Core SDK

Starting to code is simple, pull down the code from GitHub and then use either Visual Studio 2019 or Visual Studio Code. More details can be found in our [setup](setup.md) article.

## General model principles

The model design principles are agnostic to whether the model will be populated via a SharePoint REST or Microsoft Graph call, and therefore starting here to understand the general model principles is advised. Once you understand the model design principles you can learn more about how to decorate the model to work with either SharePoint REST and/or Microsoft Graph. Below picture gives an overview of the used classes in the model based on the Team model implementation:

![Model overview](../images/model%20overview.png)

In the model there are 2 types of classes:

- The majority of the model is built from **model classes**
- Model classes often live in a collection, so we do have **model collection classes**

Each of these classes has a public model implemented via interfaces and an internal model implemented via internal partial classes.

### Model classes

The model classes are the most commonly used classes in our domain model as they represent a Microsoft 365 object that can be queried via either the SharePoint REST or the Microsoft Graph interface. Samples of model classes are Web, Team, List,...

#### Public model

The public model is built via public interfaces. Below sample shows the public model for a SharePoint List

```csharp
/// <summary>
/// Public interface to define a List object of SharePoint Online
/// </summary>
[ConcreteType(typeof(List))]
public interface IList : IDataModel<IList>, IDataModelGet<IList>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
{
    /// <summary>
    /// The Unique ID of the List object
    /// </summary>
    public Guid Id { get; }

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
- Uses the `ConcreteType` attribute to define the implementation type that belongs to this interface
- Has inline documentation on the model class and fields
- Always implements the `IDataModel<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample)
- Optionally implements the `IDataModelGet<TModel>` interface whenever **get** functionality is needed on this model class (most of the models have this)
- Optionally implements the `IDataModelUpdate` interface whenever **update** functionality in needed on this model class
- Optionally implements the `IDataModelDelete` interface whenever **delete** functionality is needed on this model class
- Optionally implements the `IQueryableDataModel` interface whenever the model supports LINQ querying. This goes hand in hand with using the `QueryableDataModelCollection` base class for the model's collection class

The properties in the model use either basic .Net data types, enumerations, other model/collection types or so called complex types:

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

> [!Note]
> When a property is read-only you only need to provide a ´get´ in the public model.

#### Internal implementation

The internal model implementation is what brings the public model to life: this split approach ensures that library consumers only work with the public model, and as such the library implementation can be updated without breaking the public contract with library consumers. Here's a snippet of the `List.cs` class:

```csharp
[SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
[GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}", LinqGet = "sites/{Parent.GraphId}/lists")]
internal partial class List : BaseDataModel<IList>, IList
{
    public List()
    {
        // Handler to construct the Add request for this list
        AddApiCallHandler = () =>
        {
            var entity = EntityManager.GetClassInfo(GetType(), this);

            var addParameters = new
            {
                __metadata = new { type = entity.SharePointType },
                BaseTemplate = TemplateType,
                Title
            }.AsExpando();
            string body = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject));
            return new ApiCall($"_api/web/lists", ApiType.SPORest, body);
        };
    }

    public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

    [GraphProperty("displayName")]
    public string Title { get => GetValue<string>(); set => SetValue(value); }

    [GraphProperty("description")]
    public string Description { get => GetValue<string>(); set => SetValue(value); }

    public IFolder RootFolder { get => GetModelValue<IFolder>(); }

    [GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
    public IListItemCollection Items { get => GetModelCollectionValue<IListItemCollection>(); }

    // Other properties left for brevity

    [KeyProperty(nameof(Id))]
    public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
}
```

Each model class:

- Inherits from the `BaseDataModel<TModel>` class and implements `TModel`
- Is an **internal**, **partial** class
- Does have a public default constructor
- Can implement event handlers which are used to (see the [Event Handlers](event%20handlers.md) page for more details):

  - Optionally customize the JSON to Model mapping via the `MappingHandler = (FromJson input)` handler
  - Implement the API call for doing an Add operation via the `AddApiCallHandler = async ()` handler
  - Optionally implement API call overrides that allow you to update the generated API call before it's sent off to the server. There are these handlers: `GetApiCallOverrideHandler = async (ApiCall apiCall)`, `UpdateApiCallOverrideHandler = async (ApiCall apiCall)` and `DeleteApiCallOverrideHandler = async (ApiCall apiCall)`
  - Optionally implement property validation (prevent property updates, alter values) via the `ValidateUpdateHandler = (ref FieldUpdateRequest fieldUpdateRequest)` handler

- Contains class level attributes that are used to define the requests to Microsoft 365 and serialization of the received data. These attributes are explained in more detail in their respective chapters later on
- Has public properties that:
  
  - Use the `GetValue` and `SetValue` inherited methods to get and set simple property values
  - Use the `GetModelValue` and optionally `SetModelValue` base class methods to get and set model property values (e.g. `IFolder`)
  - Use the `GetModelCollectionValue` base class method to get a model collection property value (e.g. `IListItemCollection`)
  
- Has a `Key` property override which can be used to set/get the key value. The Key is used to organize objects in collections
- Has property attributes that are used to define the requests to Microsoft 365 and serialization of the received data. These attributes are explained in more detail in their respective chapters later on
- Model specific methods can be foreseen. These methods provide additional operations on the model class

### Collection classes

Collection classes contain zero or more model class instances, so for example the `ListCollection` will contain zero or more `List` model class instances.

#### Public model

The public model is built via public interfaces. Below sample shows the public model for a SharePoint ListCollection

```csharp
/// <summary>
/// Public interface to define a collection of List objects of SharePoint Online
/// </summary>
[ConcreteType(typeof(ListCollection))]
public interface IListCollection : IDataModelCollection<IList>, IQueryable<IList>, ISupportPaging<IList>, IDataModelCollectionDeleteByGuidId
{
    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public Task<IList> AddAsync(string title, int templateType);

    /// <summary>
    /// Select a list by title
    /// </summary>
    /// <param name="title">The title to search for</param>
    /// <param name="selectors">The expressions declaring the fields to select</param>
    /// <returns>The resulting list instance, if any</returns>
    public Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] selectors);

    // Other methods omitted for brevity

}
```

Each public model interface for a Collection class:

- Uses a public interface (e.g. `IListCollection` in our example) with optionally public methods
- Contains the implementation of the methods defined in the public interface
- Has inline documentation on the model class and methods
- Always implements the `IDataModelCollection<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample)
- Optionally implements the `IQueryable<TModel>` interface where `TModel` is the actual interface (e.g. `IList` in above sample) whenever the model can be queried using linq queries
- Optionally implements the `ISupportPaging<TModel>` interface whenever the data in the collection can be retrieved from the server via paging
- Optionally implements either the `IDataModelCollectionDeleteByGuidId`, `IDataModelCollectionDeleteByIntegerId` or `IDataModelCollectionDeleteByStringId` interface matching the data type of the collection model's key if you want to offer a `DeleteById` method on the model collection. You should only do this if you've also implemented the `IDataModelDelete` on the collection's model

Optionally a collection interface defines methods which add behavior to the collection.

#### Internal implementation

For the internal collection class implementation, we've opted to use internal partial classes. Here's a snippet of the `ListCollection.cs` class, which is linq queryable:

```csharp
internal partial class ListCollection : QueryableDataModelCollection<IList>, IListCollection
{
    public ListCollection(PnPContext context, IDataModelParent parent, string memberName = null)
        : base(context, parent, memberName)
    {
        this.PnPContext = context;
        this.Parent = parent;
    }

    // Other methods omitted for brevity

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

        var newList = CreateNewAndAdd() as List;

        newList.Title = title;
        newList.TemplateType = templateType;

        return await newList.AddAsync().ConfigureAwait(false) as List;
    }

    public async Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] selectors)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Title == title, selectors).ConfigureAwait(false);
    }

    // Other methods omitted for brevity
}
```

If the collection is not linq queryable the collection class is very simple:

```csharp
internal partial class TeamAppCollection : BaseDataModelCollection<ITeamApp>, ITeamAppCollection
{ }
```

Each collection class:

- Inherits from either the `BaseDataModelCollection<TModel>` for regular collections or from the `QueryableDataModelCollection<TModel>` class for linq queryable collections and implements the previously created collection interface (e.g. `IListCollection`)
- Is an **internal**, **partial** class
- Implements a specific constructor in case the class inherits from `QueryableDataModelCollection<TModel>`
- Can use the `CreateNewAndAdd` collection base class method to create a new instance and add it to the collection

## Decorating the model

The model, collections and complex type classes you create can be populated via either SharePoint REST queries, Microsoft Graph queries or both. Depending on the needed query approach you'll need to decorate the model classes and/or fields with properties. **It's these properties that drive the automatic query generation**.

When you populate your model via SharePoint REST queries then continue [here](extending%20the%20model%20-%20SharePoint%20REST.md), in case the model is populated via Microsoft Graph continue [here](extending%20the%20model%20-%20Microsoft%20Graph.md).
