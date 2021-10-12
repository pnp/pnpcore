# PnP Transformation Framework - Getting Started

The PnP Transformation Framework is a library to transform content pages from any source platform into Microsoft SharePoint Online modern pages.

At the time of this writing, there is native support for transforming SharePoint classic pages from SharePoint 2013, 2016, 2019, and Online into SharePoint modern pages in SharePoint Online. However, the architecture of the framework is open and extensible
and can be used to create any custom data source for reading content pages from any third party platform.

You can use the PnP Transformation Framework either via [PnP PowerShell](https://github.com/pnp/powershell) or in your own .NET code as a referenced library.

## Transforming SharePoint pages via PnP PowerShell

In order to use the PnP Transformation Framewok in PnP PowerShell, you need to [install](https://pnp.github.io/powershell/#getting-up-and-running) the latest build of the PowerShell library. 

Once you have done that, you can simply invoke the *Invoke-PnPTransformation* cmdlet to trigger a page transformation.
Here you can see a sample code excerpt to transform a page from a SharePoint Online classic site into another SharePoint Online modern site.

```powershell
# Connect to the target site
$targetConnection = Connect-PnPOnline https://target-tenant.sharepoint.com/sites/TargetModernSite/ -ReturnConnection

# Connect to the source site
Connect-PnPOnline https://source-tenant.sharepoint.com/sites/SourceClassicSite/

# Trigger transformation
Invoke-PnPTransformation -Identity source-page.aspx -TargetConnection $targetConnection
```

The *Invoke-PnPTransformation* cmdlet supports a rich set of options to easily customize the transformation behavior. In the following example, you can see an invocation with additional settings levaraging the parameters splatting capability of PowerShell and reusing the same settings to transform multiple pages.

```powershell
# Connect to the target site
$targetConnection = Connect-PnPOnline https://target-tenant.sharepoint.com/sites/TargetModernSite/ -ReturnConnection

# Connect to the source site
Connect-PnPOnline https://source-tenant.sharepoint.com/sites/SourceClassicSite/

# Or you can use parameter splatting
$transformationParams = @{ 
    LocalStoragePath = "c:\temp"
    CopyPageMetadata = $true
    KeepPageCreationModificationInformation = $true
    KeepPageSpecificPermissions = $true
    Overwrite = $true
    SetAuthorInPageHeader = $true
    TargetPagePrefix = "Migrated_"
    RemoveEmptySectionsAndColumns = $true
    HandleWikiImagesAndVideos = $true
    AddTableListImageAsImageWebPart = $true
    IncludeTitleBarWebPart = $true
    SkipHiddenWebParts = $true
    TargetConnection = $targetConnection
}

Invoke-PnPTransformation -Identity first-page.aspx @transformationParams
Invoke-PnPTransformation -Identity second-page.aspx @transformationParams
```
Notice that the *Invoke-PnPTransformation* cmdlet supports SharePoint classic as the unique data source, so you cannot use it to plug into the PnP Transformation Framework a custom data source of your own. 

## Transforming SharePoint pages with custom .NET code

If you want to use the PnP Transformation Framework in your own custom developed solutions, or eventually leveraging a custom data source that you implemented, you can reference the [PnP Transformation Framework NuGet package](https://www.nuget.org/packages/PnP.Core.Transformation/) in your .NET solution. 

In particular, if you like to transform classic SharePoint content pages to SharePoint Online, you can reference the [PnP Transformation Framework library for SharePoint](https://www.nuget.org/packages/PnP.Core.Transformation.SharePoint/), which includes a dependency on the main PnP Transformation Framework NuGet package.

> [!Note]
> PnP Transformation Framework library for SharePoint relies on the SharePoint Client-Side Object Model (CSOM) to read the classic content pages from the source.

The PnP Transformation Framework is based on the PnP Core SDK and relies on dependency injection, so in order to start using it in your code, you need to setup a host context. In the following code excerpt, you can see an example of a console application to transform a SharePoint classic page.

```csharp
var host = Host.CreateDefaultBuilder()
// Configure logging
.ConfigureServices((hostingContext, services) =>
{
    // Add the PnP Core SDK library
    services.AddPnPCore(options => {
        options.PnPContext.GraphFirst = true;
        options.HttpRequests.UserAgent = "ISV|Contoso|ProductX";

        options.Sites.Add("TargetSite", new PnPCoreSiteOptions
        {
            SiteUrl = "https://target-tenant.sharepoint.com/sites/TargetModernSite/"
        });
    });

    services.AddPnPCoreAuthentication(
        options => {
            // Configure an Authentication Provider relying on the interactive authentication
            options.Credentials.Configurations.Add("interactive",
                new PnPCoreAuthenticationCredentialConfigurationOptions
                {
                    ClientId = "{your_client_id}",
                    TenantId = "{your_tenant_id}",
                    Interactive = new PnPCoreAuthenticationInteractiveOptions
                    {
                        RedirectUri = new Uri("http://localhost")
                    }
                });

            // Configure the default authentication provider
            options.Credentials.DefaultConfiguration = "interactive";

            // Map the site defined in AddPnPCore with the 
            // Authentication Provider configured in this action
            options.Sites.Add("TargetSite",
                new PnPCoreAuthenticationSiteOptions
                {
                    AuthenticationProviderName = "interactive"
                });
        }
    );

    // Register the transformation services for SharePoint as the data source
    services.AddPnPSharePointTransformation(
        pnpOptions => // Global settings
        {
            pnpOptions.DisableTelemetry = false;
            pnpOptions.PersistenceProviderConnectionString = @"c:\temp";
        },
        pageOptions => // Target modern page creation settings
        {
            pageOptions.CopyPageMetadata = true;
            pageOptions.KeepPageCreationModificationInformation = true;
            pageOptions.PostAsNews = false;
            pageOptions.PublishPage = false;
            pageOptions.DisablePageComments = false;
            pageOptions.KeepPageSpecificPermissions = true;
            pageOptions.Overwrite = true;
            pageOptions.ReplaceHomePageWithDefaultHomePage = true;
            pageOptions.SetAuthorInPageHeader = true;
            pageOptions.TargetPagePrefix = "Migrated_";
            pageOptions.TargetPageTakesSourcePageName = true;
        },
        spOptions => // SharePoint classic source settings
        {
            spOptions.RemoveEmptySectionsAndColumns = true;
            spOptions.ShouldMapUsers = true;
            spOptions.HandleWikiImagesAndVideos = true;
            spOptions.AddTableListImageAsImageWebPart = true;
            spOptions.IncludeTitleBarWebPart = true;
            spOptions.SkipHiddenWebParts = true;
            spOptions.SkipUrlRewrite = true;
        }
    );

})
// Let the builder know we're running in a console
.UseConsoleLifetime()
// Add services to the container
.Build();

// Start console host
await host.StartAsync();

using (var scope = host.Services.CreateScope())
{
    // Obtain a PnP Context factory
    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
    var pageTransformator = scope.ServiceProvider.GetRequiredService<IPageTransformator>();

    using (var sourceContext = new ClientContext("https://source-tenant.sharepoint.com/sites/SourceClassicSite/"))
    {
        var targetContext = await pnpContextFactory.CreateAsync("TargetSite");
        var sourceUri = new Uri("https://source-tenant.sharepoint.com/sites/SourceClassicSite/sitepages/source-page.aspx");

        var result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);

        Console.WriteLine(result.AbsoluteUri);
    }
}
```

Aside from the dependency injection plumbing and from all the configuration settings for the transformation framework, the real code is all about getting a reference to the data source, which is a CSOM *ClientContext* object, and the target, which is *PnPContext* of PnP Core SDK. Then, you simply invoke the *TransformSharePointAsync* method providing the source context (*ClientContext* of CSOM), the target context (*PnPContext* of PnP Core SDK), and the URL of the page to transform (as a *Uri* type instance). Under the cover the transformation will take place for you and will return you back the URL of the transformed page.

> [!Note]
> Supporting dependency injection and the service-oriented model makes really simple to use the PnP Transformation Framework in .NET Core and modern cloud-hosted solutions like Azure Functions, Containers, etc.
