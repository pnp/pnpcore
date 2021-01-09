## Creating Context

In this article, you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](../readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with Teams
}
```