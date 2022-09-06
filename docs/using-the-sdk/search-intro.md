# Using search

Using the search features of PnP Core SDK you can issue queries and process the returned search results.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with files
}
```

## Making a basic search request

Let's start with making a simple search request by using one of the `Search` methods on `IWeb`. When making a search request you specify the search query and related options via a `SearchOptions` object as shown in below snippet. The key input for a search request is the actual search query (`contenttypeid:"0x010100*"` in below sample) and you can learn more about how to construct that query from [here](https://docs.microsoft.com/en-us/sharepoint/dev/general-development/keyword-query-language-kql-syntax-reference).

The returned search result rows are in form of a `Dictionary<string, object>` with the search result property name as key and the search result property value as value.

```csharp
// Let's search for all lists using a particular content type and
// for the found rows return the listed "select" properties
SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
{
    TrimDuplicates = false,
    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
};

var searchResult = await context.Web.SearchAsync(searchOptions);

foreach (var result in searchResult.Rows)
{
    if (result["Title"] == "abc")
    {
        // Do something
    }
}
```

## Sorting the search results

Often you want to also sort the search results on one or more properties and this is supported via the `SortProperties` attribute of the `SearchOptions` class. Checkout this [documentation](https://docs.microsoft.com/en-us/sharepoint/dev/general-development/sorting-search-results-in-sharepoint) to learn more about the sort options offered by search.

```csharp
SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
{
    TrimDuplicates = false,
    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
    // Define the properties to use for sorting the results, sorting on DocId a best practice
    // to increase search query performance
    SortProperties = new System.Collections.Generic.List<SortOption>()
    {
        new SortOption("DocId"),
        new SortOption("ModifiedBy", SortDirection.Ascending)
    },
};

var searchResult = await context.Web.SearchAsync(searchOptions);
```

## Getting refiners for the returned search results

When making a search request you can also ask the search engine to refine the search results on one or more refiners. Refiners are managed properties that are marked as refinable in the search schema and when used you'll get next to the actual search results also a result set per refiner showing you how the returned search results are grouped based upon the refiner. Checkout this [documentation](https://docs.microsoft.com/en-us/sharepoint/dev/general-development/query-refinement-in-sharepoint) to learn more about refiners. An example will provide more clarity: using below code we'll query all document libraries in the tenant and refine on `contenttypeid`. As a result you'll get a list of all document libraries in the search results and in the refinements you'll get a break down per `contenttypeid`, you'll see that for `contenttypeid` x there are y libraries using it, for `contenttypeid` Z there are n libraries using it, ...

```csharp
SearchOptions searchOptions = new SearchOptions("contentclass:STS_ListItem_DocumentLibrary")
{
    TrimDuplicates = false,
    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
    SortProperties = new System.Collections.Generic.List<SortOption>() { new SortOption("DocId") },
    RefineProperties = new System.Collections.Generic.List<string>() { "ContentTypeId" }
};

var searchResult = await context.Web.SearchAsync(searchOptions);

// Process the search results
foreach (var result in searchResult.Rows)
{
    if (result["Title"] == "abc")
    {
        // Do something
    }
}

// Process the refiner results
foreach (var refiner in searchResult.Refinements)
{
    foreach (var refinementResult in refiner.Value)
    {
        // refinementResult.Value is a possible refinement value
        // refinementResult.Count will provide the number of counts for the refinement value
    }
}
```

## Refining search results using refinement filters

The following example shows how to create a search request by passing the first refinement option obtained from the previous example search result. Set the `RefinementFilters` property to a list of KQL queries, which can be built up using the `Token` from previous refinement results.

```csharp
// Retrieve the refinement
var refinementResults = searchResult.Refinements["ContentTypeId"];
var refinementToken = refinementResults[0].Token;

SearchOptions searchOptions = new SearchOptions("contentclass:STS_ListItem_DocumentLibrary")
{
    RefinementFilters = new System.Collections.Generic.List<string>() { $"ContentTypeId:{refinementToken}" }
};

// Issue the search query
var searchResult = await context.Web.SearchAsync(searchOptions);

```

## Paging the search results

By default search results are returned in pages of 500, something you can change via the `RowsPerPage` attribute of the `SearchOptions` class. When a search query returns you'll be informed of the total amount of search results there are via the `TotalRows` and `TotalRowsIncludingDuplicates` properties of the `ISearchResult` response. Using these properties in concert with the `StartRow` property of the `SearchOptions` class you retrieve all the search result pages. Below sample shows how this can be done.

```csharp
List<Dictionary<string, object>> searchResults = new List<Dictionary<string, object>>();

bool paging = true;
int startRow = 0;

while (paging)
{
    SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
    {
        StartRow = startRow,
        TrimDuplicates = false,
        SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
        SortProperties = new System.Collections.Generic.List<SortOption>()
        {
            new SortOption("DocId"),
            new SortOption("ModifiedBy", SortDirection.Ascending)
        },
    };

    // Issue the search query
    var searchResult = await context.Web.SearchAsync(searchOptions);

    // Add the returned page of results to our search results list
    searchResults.AddRange(searchResult.Rows);

    // If we're not done yet update the start row and issue a query to retrieve the next page
    if (searchResults.Count < searchResult.TotalRows)
    {
        startRow = searchResults.Count;
    }
    else
    {
        // We're done!
        paging = false;
    }
}

// Process the total search result set
foreach (var result in searchResults)
{
    if (result["Title"] == "abc")
    {
        // Do something
    }
}
```

## Batching search queries

PnP Core SDK also allows you to batch multiple search queries and send them in one operation to the server, this can be done by using one of the `SearchBatch` methods.

```csharp
var batch = context.NewBatch();
Dictionary<Guid, IBatchSingleResult<ISearchResult>> batchResults =
    new Dictionary<Guid, IBatchSingleResult<ISearchResult>>();

List<Guid> uniqueListIds = new List<Guid>();
// Imagine the uniqueListIds contains a series of list id's that you want to issue a search query for

foreach (var listId in uniqueListIds)
{
    // Issue a search query with a refinement on `contenttypeid`, we don't need the
    // result rows, so `RowLimit` is set to 0
    var request = await context.Web.SearchBatchAsync(batch, new SearchOptions($"listid:{listId}")
    {
        RowLimit = 0,
        SortProperties = new List<SortOption>() { new SortOption("DocId") },
        RefineProperties = new List<string> { "contenttypeid" },
    });
    // Track the search query batch result objects
    batchResults.Add(listId, request);
}

// Execute the batch
await context.ExecuteAsync(batch);

// Process the search results
foreach (var batchResult in batchResults)
{
    // Check the IsAvailable attribute to ensure the search request was executed
    if (batchResult.Value.IsAvailable)
    {
        // The Result property is of type ISearchResult and can be used to process the search results
        if (batchResult.Value.Result.Refinements.Count > 0 &&
            batchResult.Value.Result.Refinements.ContainsKey("contenttypeid"))
        {
            foreach (var refinementResult in batchResult.Value.Result.Refinements["contenttypeid"])
            {
                // Process the contenttypeid refiner results
            }
        }
    }
}
```

## Re-indexing webs and lists

When you make [changes to the search schema (e.g. updated managed properties)](https://docs.microsoft.com/en-us/sharepoint/crawl-site-content) then you'll need to re-index the relevant content, which can be done at [list level](lists-intro.md#re-indexing-a-list) or [web level](webs-intro.md#re-indexing-a-web).
