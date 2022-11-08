# Working with taxonomy: advanced concepts

Whereas the introduction article got you started working with taxonomy data, this article will cover some additional scenarios like term set and term properties, pinning and reusing terms.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for adding, updating and deleting data
}
```

## Working with term set properties

A term set has a property bag implemented via the `Properties` property on an `ITermSet`. This property bag is of type `ITermSetPropertyCollection` and you can perform CRUD operations on it. To read the term set properties you can read all properties of a term set or use a LINQ query to load specific properties

```csharp
// Load a term set with all it's properties
var termSet = await myTermGroup.Sets.GetByIdAsync(termSet.Id, p => p.Properties);

foreach (var property in termSet.Properties.AsRequested())
{
    // Do something with the property
}

// Load a specific property of a term set
var property = termSet.Properties.FirstOrDefault(p => p.KeyField == "property1");
```

To add new properties you use one of the `AddProperty` methods on the `ITermSet`

```csharp
// Add a new property
await termSet.AddPropertyAsync("property2", "property 2 value");
// Persist the added property
await termSet.UpdateAsync();
```

Once you've loaded a property you can also update it using the same `AddPropertyAsync` method, the method will update the property if it already existed in the property bag or add it when it was not yet available.

```csharp
// Update property2 with a new value
await termSet.AddPropertyAsync("property2", "updated property 2 value");
// Persist the updated property
await termSet.UpdateAsync();
```

To delete properties you remove them from the `ITermSetPropertyCollection` and then use one of the `Update` methods.

```csharp
// Delete all properties
termSet.Properties.Clear();
// Persist the change
await termSet.UpdateAsync();
```

## Working with term properties

Working with term properties is identical to working with term set properties.

## Getting terms based upon their property values

To get one or more terms based upon their properties you can use one of the `GetTermsByCustomProperty` methods specifying the property and property value to filter the terms on:

```csharp
var terms = await termSet.GetTermsByCustomPropertyAsync("property2", "value2");
foreach(var term in terms)
{
    // do something with the terms
}
```

## Pinning and reusing a term

Pinning a term makes linked copies of the term and its children available at the destination. The children of a pinned term can only be created or edited at the source and the changes will reflect everywhere the term is used. Reusing a term makes linked copies of the term and its children available at the destination. Children can be created for a reused term anywhere it is used but will exist only in the term set they were created.

Both pinning a term or reusing a term comes down to adding a term relation of either type `TermRelationType.Pin` or `TermRelationType.Reuse`.

```csharp
// Pin term A in term set A under term B in term set B
await termA.Relations.AddAsync(TermRelationType.Pin, termSetB, termB);

// Reuse TermA under TermSetB
await termA.Relations.AddAsync(TermRelationType.Reuse, termSetB);
```
