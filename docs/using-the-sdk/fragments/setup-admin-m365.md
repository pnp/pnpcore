## Creating Context

In this article, you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](../readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for doing Microsoft 365 admin operations
}
```

## PnP.Core.Admin dependency

The functionality shown in this article depends on the [PnP.Core.Admin nuget package](https://www.nuget.org/packages/PnP.Core.Admin). Once the PnP.Core.Admin nuget package has been installed you can get to the Microsoft 365 admin features via using the `GetMicrosoft365Admin` extension method:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Use the GetMicrosoft365Admin extension method on any PnPContext 
    // to tap into the Microsoft 365 admin features
    var isMultiGeo = await context.GetMicrosoft365Admin().IsMultiGeoTenantAsync();
}
```
