# Implementing paging

If you want a model (e.g. `List`) to retrieved using paging then you'll need to explicitly enable paging and this article will explain how.


## Using ISupportPaging

To enable paging support you need to add the `ISupportPaging<>` interface on the public collection class interface of the model for which you want to use paging. If you for example want to enabled paged retrieval of `Lists` then you need to add the interface to the `ListCollection` class.

```csharp
public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>, ISupportPaging<IList>
{
}
```

Once you've done that the paging methods/attribute can be used as shown in this example:

```csharp
using (var context = pnpContextFactory.Create("SiteToWorkWith"))
{
    // Get a first page of lists of size 2
    await context.Web.Lists.GetPagedAsync(2, p => p.Title);

    // Do we have a pointer to the next page?
    if (context.Web.Lists.CanPage)
    {
        // Load the next page
        await context.Web.Lists.GetNextPageAsync();

        // Load all pages
        await context.Web.Lists.GetAllPagesAsync();
    }
}
```

> [!Important]
> Since paging depends on the ODATA $top operator you also need to make your class linq queriable (by implementing `IQueryable<>`, next to the default `IDataModelCollection<>`). If for some reason you cannnot make your collection class linq quariable then ensure that you've decorated your model class (`List` in this case) with `LinqGet` as shown below
>
>```csharp
>    [SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
>    [GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}", LinqGet = "sites/{Parent.GraphId}/lists")]
>    internal partial class List
>    {
>    }
>```
