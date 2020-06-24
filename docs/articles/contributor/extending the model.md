# Extending the model - step by step guidance

Extending the model is a very common type of work, especially in the early days of this SDK. This page will walk you through the needed steps, but before engaging it's recommended that you've read this article: [The PnP Core SDK model](readme.md).

> [!Important]
> In order to speed up the model development the needed SharePoint REST model classes are for the most part pre created, these classes can be copied over and modified to suit your needs. You can find the pre-generated classes in https://github.com/pnp/pnpcore/tree/dev/src/generated. If you're not able to find your model classes then please create an issue in the issue list so we can update the generation logic to include those.

## Step 1: Define the public model

### Step 1.1: Create the interface(s)

The public model is an interface and lives in the **Public** folder. So when extending the SharePoint model you would create the new interface in for example `Model\SharePoint\Core\Public` or in `Model\SharePoint\Navigation\Public`. If you feel there's a need to add a new sub level (e.g. Core, Navigation, etc) then you can do that. Key things to check are:

Let's consider a model called `Model` in SharePoint

- Your interface(s) are in the `Public` folder
- Your interface(s) are public (e.g. `public interface IWeb`) and follow the standard interface naming convention (so starting with an I e.g. `IModel`)
- Your interface(s) extends the needed base interfaces (see [The PnP Core SDK model](readme.md) for more details). At a minimum, a model interface extends the `IDataModel<IModel>` interface and a collection interface extends the `IDataModelCollection<IModel>` interface
- Your interface(s) namespace reflects the top level model folder (e.g. all SharePoint interfaces live in the `PnP.Core.Model.SharePoint` namespace)
- Your interface(s) have triple slash comments explaining their purpose

#### Sample of a Model interface

```csharp
namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a {Model} object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Model))]
    public interface IModel : IDataModel<IModel>
    {
    }
}
```

#### Sample of a collection of Model items interface

```csharp
namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of {Model} object of SharePoint Online
    /// </summary>
    public interface IModelCollection : IDataModelCollection<IModel>
    {
    }
}
```

### Step 1.2: Add the properties

Once the interface is created you need to add properties to it, the properties you want to add are quite often inspired by what the called API's return. So if the API you plan to call to populate this model returns a string field with name FieldA you would want to add a property for it: `public string FieldA { get; set; }`. Key things to check are:

- The added properties are public properties
- The added properties have a getter and a setter, unless the property is model collection (e.g. `public IListCollection Lists { get; }` in `IWeb.cs`)
- The properties are simple .Net types, enums (see below), complex types or collections of already defined model classes or complex types. See [The PnP Core SDK model](./readme.md) for more details
- Properties have triple slash comments explaining their purpose

#### Sample of a Model interface with properties

```csharp
namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a {Model} object of SharePoint Online
    /// </summary>
    public interface IModel : IDataModel<IModel>
    {
        /// <summary>
        /// The unique ID of the {Model} object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets the FieldA of the {Model}
        /// </summary>
        public string FieldA { get; set; }

        /// <summary>
        /// Gets or Sets whether the Model is "FieldB"
        /// </summary>
        public bool FieldB { get; set; }
    }
}    
```

#### Sample of a ModelCollection interface and a Model2 interface that uses it

```csharp
namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of {Model} object of SharePoint Online
    /// </summary>
    public interface IModelCollection : IDataModelCollection<IModel>
    {

    }
}
```

```csharp
namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a {Model2} object of SharePoint Online
    /// </summary>
    public interface IModel2 : IDataModel<IModel>
    {
         /// <summary>
        /// Collection of Model items in the current Model2 object
        /// </summary>
        public IModelCollection Items { get; }
    }
}
```

## Step 2: Define the internal model

Once you've defined the public interface the next step is defining the internal classes that implement the created interface(s). These internal classes live at the same level as your interface classes but then in the `Internal` folder instead of the `Public` folder. So if the interface lives in the `Model\SharePoint\Core\Public` folder then the respective internal class lives in the `Model\SharePoint\Core\Internal` folder.

At this moment the internal classes are split into 2 partial classes: there's a `model.cs` and a `model.gen.cs` class. Overtime we might opt to combine these classes into a single class, but for now it's recommended to implement 2 classes.

### Step 2.1: Create the model.gen.cs class

The `model.gen.cs` class contains the code that in the future could be generated, which are the properties. Key things to check are:

- It's an internal partial class, e.g. `internal partial class Web`
- The class name is aligned to the interface name (e.g. `IWeb` and `Web`)
- The namespace is the same as the one used for the interface
- The class implements the needed base classes and interfaces:
  - A model class typically implements `BaseDataModel<IModel>` and the public interface `IModel`
  - A collection class `BaseDataModelCollection<IModel>` and the public interface `IModelCollection`
- The used interface needs to be implemented, which means all the properties defined in the interface will be added:
  - Properties are public
  - Properties use the `GetValue<>` base class method for getting
  - Properties use the `SetValue` base class method for setting
  - Collection properties have specific get implementations, check [The PnP Core SDK model](readme.md)) for more details
- You've added the `Key` property and it points to the property that uniquely identifies your model instance. Ensure you've also updated the `KeyProperty` value to match how you've implemented the `Key` property:

```csharp
// Property that uniquely identifies this model instance is a guid named Id
public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

// Implement they Key property to use the guid ID:
[KeyProperty("Id")]
public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
```

### Step 2.2: Create the model.cs class

The `model.cs` contains the **custom** code, so code that could not be generated. Key things to check are:

- It's an internal partial class, e.g. `internal partial class Web`
- The class name is aligned to the interface name (e.g. `IWeb` and `Web`)
- The namespace is the same as the one used for the interface

## Step 3: Decorate the model to enable create/read/update/delete functionality

With step 1 and 2 done you've a model definition, but there's no behaviour yet. To bring the model to life, you'll need to **decorate** it with class and property attributes. Depending on whether you're targeting SharePoint REST API's or Microsoft Graph API's you would use different attributes to decorate the model:

- [Decorating the model for SharePoint REST API's](extending%20the%20model%20-%20SharePoint%20REST.md)
- [Decorating the model for Microsoft Graph API's](extending%20the%20model%20-%20Microsoft%20Graph.md)

> [!Important]
> If you're extending the SharePoint model and there's both a Microsoft Graph and a SharePoint REST API available then start with implementing the model using SharePoint REST. Once that works you can add additional Microsoft Graph based decoration.

## Step 4: Add additional functionality

At this point you should have a working model that can be used to read data and depending on the implementation also supports adding, updating and deleting. The power of the SDK however will not just be the model, but also the "rich" extensions on top of it. The extensions can be implemented as methods on the model, e.g. you could imagine an `UploadFile` method on the `ITeamChannel` model. For implementing this additional functions you quite often will need to execute API calls and optionally have the resulting json response loaded into the model. For doing that there are the `RequestAsync` and `Request` base class methods in case you want the resulting json be processed or the `RawRequestAsync` method in case you want to process the resulting json as part of your implementation. You'll find more details on this on the pages that explain the model decoration for Graph/REST:

- [Decorating the model for SharePoint REST API's](extending%20the%20model%20-%20SharePoint%20REST.md)
- [Decorating the model for Microsoft Graph API's](extending%20the%20model%20-%20Microsoft%20Graph.md)

## Step 5: Write test cases

Quality is key aspect of this library and test cases do help to guarantee quality. See the [Writing test cases](writing%20tests.md) article to learn more about how to do that.
