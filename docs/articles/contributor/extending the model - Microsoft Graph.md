
# Extending the model via Microsoft Graph

The PnP Core SDK model contains model, collection and complex type classes which are populated via either Microsoft Graph or SharePoint REST. In this chapter you'll learn more on how to decorate your classes and their properties to interact with Microsoft 365 via the Microsoft Graph API.

## Configuring model classes

### Class decoration

Each model class does need to have a `ClassMapping` attribute which is defined on the coded model class (e.g. Team.cs):

```csharp
[ClassMapping(GraphId = "id",
              GraphUri = "teams/{Site.GroupId}")]
internal partial class Team
{
    // Ommitted for brevity
}
```

When configuring the `ClassMapping` attribute for Microsoft Graph you need to set attribute properties:

Property | Required | Description
---------|----------|------------
GraphId | Yes | Defines the Microsoft graph object field which serves as unique id for the object. Typically this field is called `id`
GraphUri | Yes | Defines the URI that uniquely identifies this object. See [model tokens](model%20tokens.md) to learn more about the possible tokens you can use
GraphGet | No | Overrides the GraphUri property for **get** operations
GraphUpdate | No | Overrides the GraphUri property for **update** operations
GraphDelete | No | Overrides the GraphUri property for **delete** operations
GraphOverflowFieldName | No | Used when working with a dynamic property/value pair (e.g. fields in a SharePoint ListItem) whenever the Microsoft Graph field containing these dynamic properties is not named `Values`

### Property decoration

The property level decoration is done using the `GraphFieldMapping` attribute. For most properties you do not need to set this attribute, it's only required for special cases. Since the properties are defined in the generated model class (e.g. Teams.gen.cs) the decoration via attributes needs to happen in this class as well.

```csharp
// Configure the Microsoft Graph field used to populate this model property
[GraphFieldMapping(FieldName = "displayName")]
public string Title { get => GetValue<string>(); set => SetValue(value); }

// Mark the property that serves as Key field (used to ensure there are no duplicates in collections), use a JsonPath to get the specific value you need
[GraphFieldMapping(FieldName = "sharepointIds", JsonPath = "webId", IsKey = true)]
public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

// Define a collection as expandable
[GraphFieldMapping(FieldName = "lists", Expandable = true)]
public IListCollection Lists
{
    get
    {
        if (!HasValue(nameof(Lists)))
        {
            var lists = new ListCollection
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            SetValue(lists);
        }
        return GetValue<IListCollection>();
    }
}

// Configure an additional query to load this model class, this is a non expandable collection
[GraphFieldMapping(FieldName = "channels", ExpandByDefault = true, GraphGet = "teams/{Site.GroupId}/channels")]
public ITeamChannelCollection Channels
{
    get
    {
        if (!HasValue(nameof(Channels)))
        {
            var channels = new TeamChannelCollection
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            SetValue(channels);
        }
        return GetValue<ITeamChannelCollection>();
    }
}
```

You can set following properties on this attribute:

Property | Required | Description
---------|----------|------------
FieldName | No | Use this property when the Microsoft Graph fieldname differs from the model property name
IsKey | No | Marks the model property as holding a unique value. This value is used to ensure no duplicate model class instances are loaded in collection classes, if the model class has a unique property then that property should be decorated
JsonPath | No | When the information returned from Microsoft Graph is a complex type and you only need a single value from it, then you can specify the JsonPath for that value. E.g. when you get sharePointIds.webId as response you tell the model that the fieldname is sharePointIds and the path to get there is webId. The path can be more complex, using a point to define property you need (e.g. property.child.childofchild)
Expandable | No | Defines that a collection is expandable, meaning it can be loaded via the $expand query parameter and used in the lambda expression in `Get` and `GetAsync` operations
ExpandByDefault | No | When the model contains a collection of other model objects then setting this attribute to true will automatically result in the population of that collection. This can negatively impact performance, so only set this when the collection is almost always needed
GraphGet | No | Sometimes it's not possible to load the complete model via a single Microsoft Graph request, often this is the case with collections (so the collection is **not** expandable). In this case you need to explain how to load the collection via specifying the needed query. See [model tokens](model%20tokens.md) to learn more about the possible tokens you can use
UseCustomMapping | No | Allows you to force a callout to the model's `MappingHandler` event handler whenever this property is populated. See the [Event Handlers](event%20handlers.md) article to learn more

## Configuring complex type classes

### Class decoration

Each complex type class does require a `ClassMapping` attribute which is defined on the generated complex type class (e.g. TeamFunSettings.gen.cs):

```csharp
[ClassMapping]
internal partial class TeamFunSettings : BaseComplexTypeModel<ITeamFunSettings>, ITeamFunSettings
{
    // Ommitted for brevity
}
```

Since the complex type class is not queried independently there's no need to further define properties on the `ClassMapping` attribute.

### Property decoration

The property level decoration is done using the `GraphFieldMapping` attribute. For most properties you do not need to set this attribute, it's only required for special cases. Since the properties are defined in the generated model class (e.g. TeamFunSettings.gen.cs) the decoration via attributes needs to happen in this class as well. Since complex types are not directly queried and are not used in collections only a few of the `GraphFieldMapping` properties make sense to be used.

Property | Required | Description
---------|----------|------------
FieldName | No | Use this property when the Microsoft Graph fieldname differs from the model property name
JsonPath | No | When the information returned from Microsoft Graph is a complex type and you only need a single value from it, then you can specify the JsonPath for that value. E.g. when you get sharePointIds.webId as response you tell the model that the fieldname is sharePointIds and the path to get there is webId. The path can be more complex, using a point to define property you need (e.g. property.child.childofchild)
UseCustomMapping | No | Allows you to force a callout to the model's `MappingHandler` event handler whenever this property is populated. See the [Event Handlers](event%20handlers.md) article to learn more

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

Below code snippets show the above three concepts. First one shows the collection interface (e.g. ITeamChannelCollection.cs) with the Add methods:

```csharp
/// <summary>
/// Public interface to define a collection of Team Channels
/// </summary>
public interface ITeamChannelCollection : IDataModelCollection<ITeamChannel>
{

    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public Task<ITeamChannel> AddAsync(string name, string description = null);

    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="batch">Batch to use</param>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public ITeamChannel Add(Batch batch, string name, string description = null);

    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public ITeamChannel Add(string name, string description = null);
}
```

Implementation of the interface in the coded collection class (e.g. TeamChannelCollection.cs):

```csharp
internal partial class TeamChannelCollection
{
    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public async Task<ITeamChannel> AddAsync(string name, string description = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var newChannel = AddNewTeamChannel();

        // Assign field values
        newChannel.DisplayName = name;
        newChannel.Description = description;

        return await newChannel.AddAsync().ConfigureAwait(false) as TeamChannel;
    }

    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="batch">Batch to use</param>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public ITeamChannel Add(Batch batch, string name, string description = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var newChannel = AddNewTeamChannel();

        // Assign field values
        newChannel.DisplayName = name;
        newChannel.Description = description;

        return newChannel.Add(batch) as TeamChannel;
    }

    /// <summary>
    /// Adds a new channel
    /// </summary>
    /// <param name="name">Display name of the channel</param>
    /// <param name="description">Optional description of the channel</param>
    /// <returns>Newly added channel</returns>
    public ITeamChannel Add(string name, string description = null)
    {
        return Add(PnPContext.CurrentBatch, name, description);
    }
}
```

And finally you'll see the actual add logic being implemented in the coded model class (e.g. TeamChannel.cs) via implementing the `AddApiCallHandler`:

```csharp
internal partial class TeamChannel
{
    private const string baseUri = "teams/{Parent.GraphId}/channels";

    internal TeamChannel()
    {
        // Handler to construct the Add request for this channel
        AddApiCallHandler = () =>
        {
            // Define the JSON body of the update request based on the actual changes
            dynamic body = new ExpandoObject();
            body.displayName = DisplayName;
            if (!string.IsNullOrEmpty(Description))
            {
                body.description = Description;
            }

            // Serialize object to json
            var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

            return new ApiCall(ParseApiRequest(baseUri), ApiType.Graph, bodyContent);
        };
    }
}
```
