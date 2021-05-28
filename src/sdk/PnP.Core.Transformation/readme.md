# PnP.Core.Transformation


## Getting started
In order to setup PnP transformation engine you have to configure it using .NET dependency injection. Since library is built on top of **PnP Core** you have also to configure sites and authentication methods.
```c#
services.AddPnPCoreAuthentication();
services.AddPnPCore();
            
services.AddPnPSharePointTransformation();
```
The library supports the transformation of single page through an instance of type **IPageTransformator**. You can transform a SharePoint page quickly the extension method provided:
```c#
// Get required dependencies
var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
var pageTransformator = provider.GetRequiredService<IPageTransformator>();

// Create SharePoint contexts for source and target
var sourceContext = await pnpContextFactory.CreateAsync("source");
var targetContext = await pnpContextFactory.CreateAsync("target");
var sourceUri = new Uri("http://site/item");

// Transform the source page uri into the target context
Uri result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);
```
The function returns when the transformation is completed.
## Batch execution
If you want to process more than one page you can use the page transformator instance multiple times but library has the ability to enumerate pages of a site and transform each item available on the source.
Using an instance of type **ITransformationExecutor** and method **TransformSharePointAsync** you can transform a entire SharePoint site.
```c#
// Get required dependency
var transformationExecutor = provider.GetRequiredService<ITransformationExecutor>();

// Transform the entire site
TransformationProcessStatus result = await transformationExecutor.TransformSharePointAsync(sourceContext, targetContext);
```
The executor uses the default implementation which works in memory and returns final status. Optionally you can provide a **CancellationToken** in order to cancel the long running operation.

## Transformation customization
The configuration

## Custom batch processing
Since a site can

## Extensibility
The extension method **AddPnPSharePointTransformation**  configures the base engine and SharePoint as a source. You can customize each parts of the engine providing a custom implementation of the following interfaces:
- **ITargetPageUriResolver**: interface used to resolve the SharePoint target uri. Call *WithTargetPageUriResolver* on *IPnPTransformationBuilder* instance in order to customize;

- **ITransformationStateManager**: interface to handle the state of a transformation process managed by an implementation of *ITransformationExecutor*. Call *WithTransformationStateManager* on *IPnPTransformationBuilder* instance in order to customize;

- **ITransformationDistiller**: interface for a service that defines a list of pages to transform comparing source and target. Call *WithTransformationDistiller* on *IPnPTransformationBuilder* instance in order to customize;

- **IPageTransformator**: . Call *WithPageTransformator* on *IPnPTransformationBuilder* instance in order to customize;