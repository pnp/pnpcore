# Working with SharePoint Syntex

[SharePoint Syntex](https://www.microsoft.com/en-ww/microsoft-365/enterprise/sharepoint-syntex-overview) provides content services to capture and scale your expertise. [SharePoint Syntex](https://www.microsoft.com/en-ww/microsoft-365/enterprise/sharepoint-syntex-overview) uses advanced AI and machine teaching to amplify human expertise, automate content processing, and transform content into knowledge. PnP Core SDK does provide support to help you with content models: you can list content models and deploy them inside your tenant using the provided support.

In the remainder of this article you'll see a lot of `context` use: in this case this is a `PnPContext` which was obtained via the `PnPContextFactory` as explained in the [overview article](readme.md) and show below:

```csharp
using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
{
    // See next chapter on how to use the PnPContext for working with Syntex Models
}
```

## Connecting to a Syntex Content Center site

Connecting to a SharePoint Syntex Content Center site is an essential step when you're using the PnP Core SDK Syntex support. The SharePoint Syntex Content Center site is special type of site that contains content models: via a content model you teach SharePoint Syntex to read your content the way you would using machine teaching to build AI models with no code. SharePoint Syntex can automatically suggest or create metadata, invoke custom Power Automate workflows, and attach compliance labels to enforce retention or record management policies.

To verify if a site is a Syntex Content Center site you can use the [IsSyntexContentCenterAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html?q=IsSyntexContentCenterAsync) method on your web. When you want to work with a SharePoint Syntex Content Center site you need to first load it, this can be done using the [AsSyntexContentCenterAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IWeb.html#collapsible-PnP_Core_Model_SharePoint_IWeb_AsSyntexContentCenterAsync) method:

```csharp

// is the site I'm connected to a Syntex Content Center site?
bool isContentCenter = await context.Web.IsSyntexContentCenterAsync()

// Load the site as content center model
var contentCenter = await context.Web.AsSyntexContentCenterAsync();

if (contentCenter != null)
{
  // Use the content center site
}
```

## Working with Syntex Content understanding models

Once you've loaded a Syntex Content Center site you can work with the content understanding models defined in that Syntex Content Center site: you can publish a model to one or more libraries, you can unpublish a model from a library and you can list the libraries to which a model was published.

### Publishing a model to a library

Syntex content understanding models extract metadata from unstructured content (documents) and therefore to use them you need to publish a model to a document library. Once the model is published to a document library and a new document is added to the library the model will process the added document and will populate the defined metadata. To publish a model via PnP Core SDK you do have several option you can chose to publish to a single library or to multiple libraries in one go. For both publish options you have the choice to provide either an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html) or to define [SyntexModelPublishOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.SyntexModelPublishOptions.html) as shown in below code snippet.

```csharp
var cc = await context.Web.AsSyntexContentCenterAsync();
var models = await cc.GetSyntexModelsAsync();
// let's work with the first Syntex model
var modelToRegister = models.First();

// Get a list reference from a context created for the site hosting the list
var documents = await contextForSiteWithData.Web.Lists.GetByTitleAsync("Documents");
var invoices  = await contextForSiteWithData.Web.Lists.GetByTitleAsync("Invoices");

// Option A: publish a model to a single library
var result = await modelToRegister.PublishModelAsync(documents, MachineLearningPublicationViewOption.NewViewAsDefault);

// Option B: publish a model to multiple libraries
List<IList> libraries = new();
libraries.Add(documents);
libraries.Add(invoices);

var result = await modelToRegister.PublishModelAsync(libraries);

// Option C: publish to a single library via SyntexModelPublishOptions
var result = await modelToRegister.PublishModelAsync(
                new SyntexModelPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/documents",
                    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
                    TargetWebServerRelativeUrl = "/sites/contosoHR",
                });

// Option D: publish a model to multiple libraries via SyntexModelPublishOptions
List<SyntexModelPublishOptions> publications = new();
publications.Add(new SyntexModelPublishOptions()
{
    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/documents",
    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
    TargetWebServerRelativeUrl = "/sites/contosoHR",
    ViewOption = MachineLearningPublicationViewOption.NewViewAsDefault
});
publications.Add(new SyntexModelPublicationOptions()
{
    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/invoices",
    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
    TargetWebServerRelativeUrl = "/sites/contosoHR",
    ViewOption = MachineLearningPublicationViewOption.NoNewView
});
var result = await modelToRegister.PublishModelAsync(publications);
```

### Unpublish a model from a library

Unpushing models from a library follows the same pattern as publishing them: you can either use an [IList](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.IList.html) or define [SyntexModelUnPublishOptions](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.SyntexModelUnPublishOptions.html) and then unpublish a model from a single library or from multiple.

```csharp
var cc = await context.Web.AsSyntexContentCenterAsync();
var models = await cc.GetSyntexModelsAsync();
// let's work with the first Syntex model
var modelToRegister = models.First();

// Get a list reference from a context created for the site hosting the list
var documents = await contextForSiteWithData.Web.Lists.GetByTitleAsync("Documents");
var invoices  = await contextForSiteWithData.Web.Lists.GetByTitleAsync("Invoices");

// Option A: unpublish a model from a single library
var result = await modelToRegister.UnPublishModelAsync(documents);

// Option B: unpublish a model from multiple libraries
List<IList> libraries = new();
libraries.Add(documents);
libraries.Add(invoices);

var result = await modelToRegister.UnPublishModelAsync(libraries);

// Option C: unpublish from a single library via SyntexModelUnPublishOptions
var result = await modelToRegister.UnPublishModelAsync(
                new SyntexModelUnPublishOptions()
                {
                    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/documents",
                    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
                    TargetWebServerRelativeUrl = "/sites/contosoHR",
                });

// Option D: unpublish a model from multiple libraries via SyntexModelUnPublicationOptions
List<SyntexModelUnPublishOptions> publications = new();
publications.Add(new SyntexModelUnPublishOptions()
{
    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/documents",
    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
    TargetWebServerRelativeUrl = "/sites/contosoHR",
    ViewOption = MachineLearningPublicationViewOption.NewViewAsDefault
});
publications.Add(new SyntexModelUnPublishOptions()
{
    TargetLibraryServerRelativeUrl = $"/sites/contosoHR/invoices",
    TargetSiteUrl = "https://contoso-sharepoint.com/sites/contosoHR",
    TargetWebServerRelativeUrl = "/sites/contosoHR",
    ViewOption = MachineLearningPublicationViewOption.NoNewView
});
var result = await modelToRegister.UnPublishModelAsync(publications);
```

### List the libraries to which a model was published

If you want to know to which libraries a Syntex model was deployed then you can use the [GetModelPublicationsAsync](https://pnp.github.io/pnpcore/api/PnP.Core.Model.SharePoint.ISyntexModel.html#collapsible-PnP_Core_Model_SharePoint_ISyntexModel_GetModelPublicationsAsync) method to get a list of libraries to which the model was published.

```csharp
var cc = await context.Web.AsSyntexContentCenterAsync();
var models = await cc.GetSyntexModelsAsync();
// let's work with the first Syntex model
var modelToRegister = models.First();


// Get libraries to which this model was published
var libraries = await modelToRegister.GetModelPublicationsAsync();

foreach(var library in libraries)
{
    // Do something with the library publication
}
```
