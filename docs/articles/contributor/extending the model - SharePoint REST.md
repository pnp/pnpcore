
# Extending the model for SharePoint REST

The PnP Core SDK model contains model, collection, and complex type classes which are populated via either Microsoft Graph and/or SharePoint REST. In this chapter you'll learn more on how to decorate your classes and their properties to interact with Microsoft 365 via the SharePoint REST API.

## Configuring model classes

### Public model (interface) decoration

For model classes that **are linq queriable** one needs to link the concrete (so the implementation) to the public interface via the `ConcreteType` class attribute:

```csharp
[ConcreteType(typeof(List))]
public interface IList : IDataModel<IList>, IDataModelUpdate, IDataModelDelete
{
    // Ommitted for brevity
}
```

### Class decoration

Each model class that uses SharePoint REST does need to have at least one `SharePointType` attribute which is defined on the coded model class (e.g. List.cs):

```csharp
[SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Get = "_api/web/lists", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
internal partial class List
{
    // Omitted for brevity
}
```

When configuring the `SharePointType` attribute for SharePoint REST you need to set attribute properties:

Property | Required | Description
---------|----------|------------
Type | Yes | Defines the SharePoint REST type that maps with the model class. Each model that requires SharePoint REST requires this attribute, hence the type is requested via the attribute constructor.
Uri | Yes | Defines the URI that uniquely identifies this object. See [model tokens](model%20tokens.md) to learn more about the possible tokens you can use.
Target | No | A model can be used from multiple scope (e.g. the ContentTypeCollection is available for both Web and List model classes) and if so the `Target` property defines the scope of the `SharePointType` attribute.
Get | No | Overrides the Uri property for **get** operations.
LinqGet | No | Some model classes do support linq queries which are translated in corresponding server calls. If a class supports linq in this way, then it also needs to have the LinqGet attribute set.
Update | No | Overrides the Uri property for **update** operations.
Delete | No | Overrides the Uri property for **delete** operations.
OverflowProperty | No | Used when working with a dynamic property/value pair (e.g. fields in a SharePoint ListItem) whenever the SharePoint REST field containing these dynamic properties is not named `Values`.

#### Sample of using multiple SharePointType decorations

Below sample shows how a model can be decorated for multiple scopes:

```csharp
[SharePointType("SP.ContentType", Target = typeof(Web), Uri = "_api/Web/ContentTypes('{Id}')", Get = "_api/web/contenttypes", LinqGet = "_api/web/contenttypes")]
[SharePointType("SP.ContentType", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/ContentTypes('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes")]
internal partial class ContentType
{
    // Ommitted for brevity
}
```

### Property decoration

The property level decoration is done using the `SharePointProperty` and `KeyProperty` attributes. Each model instance does require to have a override of the `Key` property and that `Key` property **must** be decorated with the `KeyProperty` attribute, which specifies the actual fields in the model that must be selected as key. The key is for example used to ensure there are no duplicate model class instances in a single collection.

Whereas the `KeyProperty` attribute is always there once in each model class, the usage of the `SharePointProperty` attribute is only needed whenever it makes sense. For most properties you do not need to set this attribute, it's only required for special cases. Since the properties are defined in the generated model class (e.g. List.gen.cs) the decoration via attributes needs to happen in this class as well.

```csharp
// Configure the SharePoint REST field used to populate this model property
[SharePointProperty("DocumentTemplateUrl")]
public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

// Define a collection as expandable
[SharePointProperty("Items", Expandable = true)]
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

// Set the keyfield for this model class
[KeyProperty("Id")]
public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
```

You can set following properties on this attribute:

Property | Required | Description
---------|----------|------------
FieldName | Yes | Use this property when the SharePoint REST fieldname differs from the model property name, since the field name is required by the default constructor you always need to provide this value when you add this property.
JsonPath | No | When the information returned from SharePoint REST is a complex type and you only need a single value from it, then you can specify the JsonPath for that value. E.g. when you get sharePointIds.webId as response you tell the model that the fieldname is sharePointIds and the path to get there is webId. The path can be more complex, using a point to define property you need (e.g. property.child.childofchild).
Expandable | No | Defines that a collection is expandable, meaning it can be loaded via the $expand query parameter and used in the lambda expression in `Get` and `GetAsync` operations.
ExpandByDefault | No | When the model contains a collection of other model objects then setting this attribute to true will automatically result in the population of that collection. This can negatively impact performance, so only set this when the collection is almost always needed.
UseCustomMapping | No | Allows you to force a callout to the model's `MappingHandler` event handler whenever this property is populated. See the [Event Handlers](event%20handlers.md) article to learn more.

## Configuring complex type classes

Complex type classes are not used when the model is populated via SharePoint REST.

## Configuring collection classes

Collection classes **do not** have attribute based decoration.

## Implementing "Add" functionality

In contradiction with get, update, and delete which are fully handled by decorating classes and properties using attributes, you'll need to write actual code to implement add. Adding is implemented as follows:

- The public part (interface) is defined on the collection interface. Each functionality (like Add) is implemented via three methods:

  - An async method
  - A regular method
  - A regular method that allows to pass in a `Batch` as first method parameter

- Add methods defined on the interface are implemented in the collection classes as proxies that call into the respective add methods of the added model class.
- The implementation that performs the actual add is implemented as an `AddApiCallHandler` event handler in the model class. See the [Event Handlers](event%20handlers.md) page for more details.

Below code snippets show the above three concepts. First one shows the collection interface (e.g. IListCollection.cs) with the Add methods:

```csharp
/// <summary>
/// Public interface to define a collection of List objects of SharePoint Online
/// </summary>
public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>, ISupportPaging
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

### Providing additional parameters for add requests

The `AddApiCall` handler accepts an optional key value pair parameter: `ApiCall AddApiCall(Dictionary<string, object> keyValuePairs = null)`. You can use this to provide additional input when you call the `Add` from your code in the collection class. Below sample shows how this feature is used to offer different SDK consumer methods for creating Team channel tabs (on the `TeamChannelTabCollection` class) while there's only one generic creation method implementation in the `TeamChannelTab` class. Let's start with the code in the `TeamChannelTabCollection` class:

```csharp
public async Task<ITeamChannelTab> AddWikiTabAsync(string name)
{
    if (string.IsNullOrEmpty(name))
    {
        throw new ArgumentNullException(nameof(name));
    }

    (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelWikiTab(name);

    return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
}

public async Task<ITeamChannelTab> AddDocumentLibraryTabAsync(string name, Uri documentLibraryUri)
{
    if (string.IsNullOrEmpty(name))
    {
        throw new ArgumentNullException(nameof(name));
    }

    (TeamChannelTab newTab, Dictionary<string, object> keyValuePairs) = CreateTeamChannelDocumentLibraryTab(name, documentLibraryUri);

    return await newTab.AddAsync(keyValuePairs).ConfigureAwait(false) as TeamChannelTab;
}

private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelDocumentLibraryTab(string displayName, Uri documentLibraryUri)
{
    var newTab = CreateTeamChannelTab(displayName);

    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
    {
        { "teamsAppId", "com.microsoft.teamspace.tab.files.sharepoint" },
    };

    newTab.Configuration = new TeamChannelTabConfiguration
    {
        EntityId = "",
        ContentUrl = documentLibraryUri.ToString()
    };

    return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
}

private Tuple<TeamChannelTab, Dictionary<string, object>> CreateTeamChannelWikiTab(string displayName)
{
    var newTab = CreateTeamChannelTab(displayName);

    Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
    {
        { "teamsAppId", "com.microsoft.teamspace.tab.wiki" }
    };

    return new Tuple<TeamChannelTab, Dictionary<string, object>>(newTab, keyValuePairs);
}
```

The code in the `TeamChannelTab` class then uses the additional parameter values to drive the creation behavior:

```csharp
AddApiCallHandler = (keyValuePairs) =>
{
    // Define the JSON body of the update request based on the actual changes
    dynamic tab = new ExpandoObject();
    tab.displayName = DisplayName;

    string teamsAppId = keyValuePairs["teamsAppId"].ToString();
    tab.teamsAppId = teamsAppId;

    switch (teamsAppId)
    {
        case "com.microsoft.teamspace.tab.wiki": // Wiki, no configuration possible
            break;
        default:
            {
                tab.Configuration = new ExpandoObject();

                if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p=>p.EntityId))
                {
                    tab.Configuration.EntityId = Configuration.EntityId;
                }
                if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.ContentUrl))
                {
                    tab.Configuration.ContentUrl = Configuration.ContentUrl;
                }
                if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.RemoveUrl))
                {
                    tab.Configuration.RemoveUrl = Configuration.RemoveUrl;
                }
                if (Configuration.IsPropertyAvailable<ITeamChannelTabConfiguration>(p => p.WebsiteUrl))
                {
                    tab.Configuration.WebsiteUrl = Configuration.WebsiteUrl;
                }
                break;
            }
    }

    // Serialize object to json
    var bodyContent = JsonSerializer.Serialize(tab, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

    return new ApiCall(ApiHelper.ParseApiRequest(this, baseUri), ApiType.GraphBeta, bodyContent);
};
```

## Doing additional API calls

Above example showed the `AddApiCallHandler` which provides an framework for doing add requests, but you often also need to do other types of requests (e.g. adding an available content type to a list, recycling a list, ...) and for that you need to be able to execute API calls. There are 2 ways to do this:

- Run an API call and automatically load the resulting API call response in the model
- Run an API call and process the resulting json as part of your code

Above methods are described in the next chapters.

### Running an API call and loading the result in the model

When you know that the API call you're making will return json data that has to be loaded into the model then you should use the `RequestAsync` method for immediate async processing or `Request` method for batch processing. These methods accept an `ApiCall` instance as input together with the `HttpMethod`. Below sample shows how this can be used to add an existing content type to a list. The `AddAvailableContentTypeApiCall` method defines the API call to be executed and in the `AddAvailableContentType` and `AddAvailableContentTypeAsync` methods this API call is executed via the respective `Request` and `RequestAsync` methods. When executing the API calls the resulting json is automatically processed and loaded into the model, so in the below case the content type will show up in the list content type collection.

```csharp
private ApiCall AddAvailableContentTypeApiCall(string id)
{
    dynamic body = new ExpandoObject();
    body.contentTypeId = id;

    var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = false });

    // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will
    // automatically provide the correct 'parent'
    var entity = EntityManager.Instance.GetClassInfo<IContentType>(GetType(), this);

    return new ApiCall($"{entity.SharePointGet}/AddAvailableContentType", ApiType.SPORest, bodyContent);
}

internal IContentType AddAvailableContentType(Batch batch, string id)
{
    var apiCall = AddAvailableContentTypeApiCall(id);
    Request(batch, apiCall, HttpMethod.Post);
    return this;
}

internal async Task<IContentType> AddAvailableContentTypeAsync(string id)
{
    var apiCall = AddAvailableContentTypeApiCall(id);
    await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
    return this;
}
```

### Running an API call and processing the resulting json as part of your code

Some API calls do return data, but the returned data cannot be loaded into the current model. In those cases you should use the `RawRequestAsync` method. This method accepts an `ApiCall` instance as input together with the `HttpMethod`. Below sample shows how this can be used to recycle a list (= move list to the site's recycle bin). The sample shows how the `ApiCall` is built and executed via the `RawRequestAsync` method. This method returns an `ApiCallResponse` object that contains the json response from the server, which is processed and as a result the recycle bin item id is returned and the list is removed from the model.

```csharp
public async Task<Guid> RecycleAsync()
{
    var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/recycle", ApiType.SPORest);

    var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

    if (!string.IsNullOrEmpty(response.Json))
    {
        var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
        if (document.TryGetProperty("d", out JsonElement root))
        {
            if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
            {
                // Remove this item from the lists collection
                RemoveFromParentCollection();

                // return the recyclebin item id
                return recycleBinItemId.GetGuid();
            }
        }
    }

    return Guid.Empty;
}
```
