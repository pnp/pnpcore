# Interop with PnP Framework

The PnP Core SDK is always used when PnP Framework is used as it's a dependency of PnP Framework, e.g. the Pages API implementation of PnP Core SDK is used in PnP Framework. When you're code is using PnP Framework you might also want to use additional PnP Core SDK features, or vice versa when you're using PnP Core SDK you might want to use SharePoint CSOM. Both options are discussed in this article.

## Using PnP Core SDK when PnP Framework was already configured

In PnP Core SDK a PnPContext is used while in PnP Framework a CSOM context is used. When you have a CSOM context you can create a PnPContext using the `PnPCoreSDK.Instance.GetPnPContext()` methods.

```csharp
var authManager = new AuthenticationManager("<Azure AD client id>", "joe@contoso.onmicrosoft.com", "Pwd as SecureString");

using (var csomContext = authManager.GetContext("https://contoso.sharepoint.com"))
{
    // Use CSOM to load the web title
    csomContext.Load(csomContext.Web, p => p.Title);
    csomContext.ExecuteQueryRetry();

    using (PnPContext pnpCoreContext = PnPCoreSdk.Instance.GetPnPContext(csomContext))
    {
        // Use PnP Core SDK (Microsoft Graph / SPO Rest) to load the web title
        var web = pnpCoreContext.Web.Get(p => p.Title);
    }
}
```

Above approach will trigger the creation of an `IPnPContextFactory` which will then be used to create the `PnPContext`. This `IPnPContextFactory` will be initialized using default settings, if you want to use a different configuration (e.g. because you're using another cloud environment or have need to use other non-default PnP Core settings) then you can specify the `IPnPContextFactory` while calling the `PnPCoreSDK.Instance.GetPnPContext()` methods.

> [!Important]
> Once you pass in your custom `IPnPContextFactory` by calling the `PnPCoreSDK.Instance.GetPnPContext()` methods then this `IPnPContextFactory` will also be used by all PnP Framework internal calls to `PnPCoreSDK.Instance.GetPnPContext()`.

```csharp
using (PnPContext pnpCoreContext = PnPCoreSdk.Instance.GetPnPContext(csomContext, pnpContextFactory))
{
    // Use PnP Core SDK (Microsoft Graph / SPO Rest) to load the web title
    var web = pnpCoreContext.Web.Get(p => p.Title);

    // After the first time passing in the IPnPContextFactory it will also be used by the implicit 
    // GetPnPContext calls from within PnP Framework
    // E.g. Calling the AddClientSidePage extension method will under the covers create a PnPContext 
    // using the passed IPnPContextFactory
    var page = csomContext.Web.AddClientSidePage("demo.aspx", true);
}
```

## Using PnP Framework when the PnP Core SDK was already configured

If you already have a PnP Core SDK context and you want also use PnP Framework and/or SharePoint CSOM then you can create a CSOM ClientContext from your PnP Context via the `PnPCoreSDK.Instance.GetClientContext()` method.

```csharp
using (var pnpCoreContext = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // Use PnP Core SDK (Microsoft Graph / SPO Rest) to load the web title
    var web = pnpCoreContext.Web.Get(p => p.Title);

    using (ClientContext csomContext = PnPCoreSdk.Instance.GetClientContext(pnpCoreContext))
    {
        // Use CSOM to load the web title
        csomContext.Load(csomContext.Web, p => p.Title);
        csomContext.ExecuteQueryRetry();
    }    
}
```
