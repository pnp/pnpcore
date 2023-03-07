# Working with taxonomy data: an introduction

The application that create might need taxonomy data or you might need to create taxonomy data like term groups, term sets and terms. All of this is possible using PnP Core SDK as explained in this article.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for adding, updating and deleting data
}
```

## Working the term store

All taxonomy objects like term groups, term sets and terms live in a term store. This term store can be access via the `TermStore` property of the `PnPContext` you're using:

```csharp
var termStore = await context.TermStore.GetAsync();
```

The `TermStore` property is the entry point to all the taxonomy data, but it can also be configured. Typical configurations on a term store are adding additional languages or updating the default term store language.

```csharp
var termStore = await context.TermStore.GetAsync();

if (!termStore.Languages.Contains("nl-NL"))
{
    // Add a new language
    termStore.Languages.Add("nl-NL");
    await termStore.UpdateAsync();
}
```

Removing a term language can be done as well:

```csharp
var termStore = await context.TermStore.GetAsync();

if (termStore.Languages.Contains("nl-NL"))
{
    // Remove a language
    termStore.Languages.Remove("nl-NL");
    await termStore.UpdateAsync();
}
```

> [!Important]
> If your application is using application permissions when working with taxonomy you'll need to ensure you've added SharePoint principal as termstore administrator. Navigate to https://contoso-admin.sharepoint.com/_layouts/15/TermStoreManager.aspx (update to match your tenant name) and add `i:0i.t|00000003-0000-0ff1-ce00-000000000000|app@sharepoint` to the list of Term Store Administrators. If that page is not available anymore in your tenant you can use PnP PowerShell to add the SharePoint Principal: below code will replace the existing admins with the app principal one, so first take note of the added admins or alternatively update the payload of the request:
> ```powershell
> $body = "{`"administrators`":[{`"user`": { `"email`": `"app@sharepoint`", `"userPrincipalName`": `"i:0i.t|00000003-0000-0ff1-ce00-000000000000|app@sharepoint`", displayName: `"SharePoint App`" }}]}"
> Invoke-PnPSPRestMethod -Method PATCH -Url "/_api/v2.1/termStore?select=*,administrators" -Raw -Content $body
> ```

## Working with term groups

Term groups are used to organize term sets and define permissions on who can manage or author new term sets and terms in the group. To get term groups you can either load all term groups, use a LINQ query to load specific groups or get a term group by id or name:

```csharp
// Load all term groups
await context.TermStore.LoadAsync(p => p.Groups);

// Load specific term groups using a LINQ query
var myTermGroup = await context.TermStore.Groups.Where(p => p.Name == "MyTermSets").FirstOrDefaultAsync();

// Get a term group by name (equivalent to previous LINQ query)
var myTermGroup = await context.TermStore.Groups.GetByNameAsync("MyTermSets");

// Get a term group by id
var myTermGroup = await context.TermStore.Groups.GetByIdAsync("0e8f395e-ff58-4d45-9ff7-e331ab728beb");
```

Adding a term group is done using the `Add` methods on the `ITermGroupCollection`:

```csharp
var myNewGroup = await context.TermStore.Groups.AddAsync("My New Group", "Optional group description");
```

Once you've a reference to a term group you can also update that term group:

```csharp
myNewGroup.Name = "Updated name!";
await myNewGroup.UpdateAsync();
```

Deleting a term group can be done using the `Delete` methods:

```csharp
await myNewGroup.DeleteAsync();
```

## Working with term sets

Term sets are the container for the terms and term sets themselves always are part of a term group. So to work with term sets you typically go via the term group. You can opt to load all term sets in a term group, write a LINQ query or get a term set by id:

```csharp
// Get the term group hosting the needed term set 
var myTermGroup = await context.TermStore.Groups.GetByIdAsync("0e8f395e-ff58-4d45-9ff7-e331ab728beb");

// Load all term sets in the group
await myTermGroup.LoadAsync(p => p.Sets);
foreach(var termSet in myTermGroup.Sets.AsRequested())
{
    // Use the term set
}

// Write a LINQ query to load a term set
var termSet = await myTermGroup.Sets.Where(p => p.Id == "2374aacb-8c25-4991-aa94-7585bcedf38d").FirstOrDefaultAsync();

// Get a term set by id, identical to above LINQ approach
var termSet = await myTermGroup.Sets.GetByIdAsync("2374aacb-8c25-4991-aa94-7585bcedf38d");
```

When you know the term set id you an directly get the term set via the `GetTermSetById` methods on the `ITermStore`:

```csharp
var termSet = await context.TermStore.GetTermSetByIdAsync("2374aacb-8c25-4991-aa94-7585bcedf38d", p => p.Description, p => p.Group);
```

Adding a term set to a term group is done using the `Add` methods on the `ITermSetCollection`:

```csharp
var termSet = await myTermGroup.Sets.AddAsync("MyTermSet", "Optional set description");
```

Once you've a reference to a term set you can also update that term set:

```csharp
// Update the term set description
termSet.Description = "updated description";
// Add a new localized label for the term set
(termSet.LocalizedNames as TermSetLocalizedNameCollection).Add(new TermSetLocalizedName() { LanguageTag = "nl-NL", Name = "Dutch name" });
// Send the updates to the server
await termSet.UpdateAsync();
```

Deleting a term set can be done using the `Delete` methods:

```csharp
await termSet.DeleteAsync();
```

## Working with terms

A term set can hold one or more terms and each term on it's own can hold other terms...so you can have a term hierarchy. As terms live in a term set working with terms means first getting the term set, like shown in previous chapter. If you want to work with terms you first need to load them and this can be done by loading all terms, by writing a LINQ query or by getting a term by id:

```csharp
// Load all terms in a term set that have the term set as parent
await termSet.LoadAsync(p => p.Terms);

foreach (var term in termSet.Terms.AsRequested())
{
    // Load the child terms of this term
    await term.LoadAsync(p => p.Terms);
    foreach (var childTerm in term.Terms.AsRequested())
    {
        // Do something with the term
    }

    // Do something with the term
}

// Load terms via a LINQ query
var term = await termSet.Terms.Where(p => p.Id == "6b39335d-1975-4fd7-9696-b40d57c9bde7").FirstOrDefaultAsync();

// Get a term by id from a term set
var term = await termSet.Terms.GetByIdAsync("6b39335d-1975-4fd7-9696-b40d57c9bde7");

// Get a term by id from another term
var childTerm = await term.Terms.GetByIdAsync("2dd726ce-1f14-4113-be57-5e0bc2d28914");
```

When you know the term set id and term id you an directly get the term via the `GetTermById` methods on the `ITermStore`:

```csharp
var term = await context.TermStore.GetTermByIdAsync("2374aacb-8c25-4991-aa94-7585bcedf38d", "6b39335d-1975-4fd7-9696-b40d57c9bde7", p => p.Descriptions, p => p.Set);
```

If you want to enumerate all terms in a hierarchical termset, then below code snippet shows how to combine batching and recursive code to load all the terms in the most efficient manner:

```csharp
var termset = context.TermStore.GetTermSetById("4b000117-03c4-4b2b-81f4-21e2ab26d6be", p => p.Description, p => p.Terms);

// recursively load the terms in the termset
await LoadTermsAsync(termset.Terms);

private async Task LoadTermsAsync(ITermCollection terms)
{
    var batch = terms.PnPContext.NewBatch();

    foreach (var term in terms.AsRequested())
    {
        await term.LoadBatchAsync(batch, p => p.Labels, p => p.Terms);
    }

    await terms.PnPContext.ExecuteAsync(batch);

    foreach (var term in terms.AsRequested())
    {
        if (term.Terms.AsRequested().Count() > 0)
        {
            // Load the possible child terms
            await LoadTermsAsync(term.Terms);
        }
    }
}
```

Adding a term to a term set or another term is done using the `Add` methods on the `ITermCollection`:

```csharp
// Add term at term set level, the default term store language will be assumed for the language of the name/description
var newTerm = await termSet.Terms.AddAsync("MyTerm", "Optional term description");

// Add child term to another term, the default term store language will be assumed for the language of the name/description
var childTerm = await newTerm.Terms.AddAsync("MyChildTerm", "Optional term description");
```

Once you've a reference to a term you can also update that term:

```csharp
// Add a new term label for language fr-FR
// Note: fr-FR must be a language allowed in the term store
newTerm.AddLabelAndDescription("French label", "fr-FR", false, "Optional term description");
await newTerm.UpdateAsync();
```

Deleting a term can be done using the `Delete` methods:

```csharp
await term.DeleteAsync();
```
