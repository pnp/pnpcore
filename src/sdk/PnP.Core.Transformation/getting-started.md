# Getting Started with PnP.Core.Transformation
This document provides the basic information about how to get started developing solutions that rely on the PnP Transformation Framework, to transform SharePoint Online classic pages into SharePoint Online modern pages.

## Transforming a single page
In order to setup the PnP Transformation Framework you have to configure it using .NET dependency injection. Since the library is built on top of the **PnP Core SDK** you have also to configure it.

> You can find further details about the PnP Core SDK and about how to setup it up in your solution by reading the official documentation available here: [Getting started with the PnP Core SDK](https://pnp.github.io/pnpcore/using-the-sdk/readme.html). 

Here you can see a code excerpt about how to configure dependency injection to use the PnP Core SDK and the PnP Transformation Framework. The source code refers to a couple of sites named "source" and "target" that should be configured in the settings of your application, accordingly to the configuration syntax of the PnP Core SDK.

```c#
services.AddPnPCoreAuthentication();
services.AddPnPCore();
            
services.AddPnPSharePointTransformation();
```
The PnP Transformation Framework supports the transformation of a single page through an implementation of type **IPageTransformator**. You can easily transform a SharePoint Online classic page using the following syntax:

```c#
// Get required dependencies
var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
var pageTransformator = provider.GetRequiredService<IPageTransformator>();

// Create SharePoint contexts for source and target
var sourceContext = await pnpContextFactory.CreateAsync("source");
var targetContext = await pnpContextFactory.CreateAsync("target");
var sourceUri = new Uri("https://tenant.sharepoint.com/sites/SourceSite/item.aspx");

// Transform the source page uri into the target context
Uri result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);
```

When the transformation is completed, the function returns the URL of the transformed page.

## Batch transformation
If you want to transform more than one page, you can use the IPageTransformator implementation multiple times.
However, the Transformation Framework provides the capability to enumerate all the source pages of a site and transform each of them into a modern page.
Using an implementation of type **ITransformationExecutor** and the method **TransformSharePointAsync** you can easily transform a whole SharePoint site from classic to modern.

```c#
// Get required dependency
var transformationExecutor = provider.GetRequiredService<ITransformationExecutor>();

// Transform the entire site
TransformationProcessStatus result = await transformationExecutor.TransformSharePointAsync(sourceContext, targetContext);
```

Out of the box and by default, with just two lines of code, you can transform a whole site.
Of course, you can specify additional settings to customize this default behavior.

The executor uses a default implementation, which works in memory and returns a final status as the result of the transformation process. Optionally you can provide a **CancellationToken** in order to cancel the long running operation.

## Transformation Framework Extensibility
If you like, you can heavily extend and customize the out of the box behavior of the Transformation Framework. You can read the document [Extensibility of PnP.Core.Transformation](./extensibility.md) to understand how to do that.
