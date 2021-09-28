# Extensibility of PnP.Core.Transformation
This document provides information about how to extend and customize the default behavior of the PnP Transformation Framework.

...

## Transformation customization
The configuration

## Custom batch processing
Since a site can

## Extensibility
The extension method **AddPnPSharePointTransformation**  configures the base engine using SharePoint as the data source. You can customize each part of the engine, providing a custom implementation of the following interfaces:
- **ITargetPageUriResolver**: interface used to resolve the SharePoint target uri. Call *WithTargetPageUriResolver* on *IPnPTransformationBuilder* instance in order to customize;

- **ITransformationStateManager**: interface to handle the state of a transformation process managed by an implementation of *ITransformationExecutor*. Call *WithTransformationStateManager* on *IPnPTransformationBuilder* instance in order to customize;

- **ITransformationDistiller**: interface for a service that defines a list of pages to transform comparing source and target. Call *WithTransformationDistiller* on *IPnPTransformationBuilder* instance in order to customize;

- **IPageTransformator**: . Call *WithPageTransformator* on *IPnPTransformationBuilder* instance in order to customize;