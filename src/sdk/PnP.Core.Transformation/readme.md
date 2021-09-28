# PnP.Core.Transformation
The PnP.Core.Transformation library (aka Transformation Framework) is an add-on, built on top of the PnP.Core library, to transform content-based solutions into Microsoft SharePoint Online "modern" solutions.
It is the evolution of the PnP Modernization Framework that we've built and maintained in the last few years as a PnP Community project.
The basic idea of the Transformation Framework is to support customers and developers transforming content from whatever technology (including Microsoft SharePoint on-premises and Online) to modern pages in SharePoint Online. The architecture of the framework is really open and allows to plug into it any data source capable of providing "content" that can be transformed into the content of a modern page.

## General overview
At the very basis of the Transformation Framework architecture there is the idea of **PageTransformator**, which is an object capable of transforming a data source into a SharePoint Online modern page.
You can use a PageTransformator either in code or via PowerShell (PnP PowerShell), to trasform a content page into a modern page.

The data source can be any other kind of content, as long as it is somehow transformable into a SharePoint Online modern page.
Out of the box, the Transformation Framework provides the basic infrastructure to transform classic pages of SharePoint on-premises (2013, 2016, 2019) and SharePoint Online into modern pages of SharePoint Online.
However, anyone can implement a custom data source, which is nothing more than a set of .NET types implementing some interfaces and providing some custom logic, in order to support transforming content from external platforms to modern pages in SharePoint Online. 
For example, there could be a data source for Word Press, or a data source for other portal solutions.

With a PageTransformator you can transform a single content into a modern page.
However, we've seen that most of the customers are using this technology to transform a whole site or portal into a modern site in SharePoint Online.
Moreover, there are customers and partners building solutions for transforming content and feeding modern sites in SharePoint Online.

That's why we introduced the concept of **TransformationExecutor**, which is a more complex object capable of tranforming a set of content items from a data source into a set of SharePoint Online modern pages. Internally a TransformationExecutor relies on a **TransformationDistiller**, which creates the list of content items to transform. The goal of a TransformationDistiller is to determine the list of content items to process and transform, and it can be used for example to create delta-transformations to synchornize a target site with a data source. A TransformationExecutor also relies on **TransformationStateManager** object, which is able to keep track of the state of a complex transformation process. At the very end, a TransformationExecutor will use a set of PageTransformator instances to transform the set of pages in target.

This architecture allows to create highly scalable and decoupled solutions that can either process in-memory the transformation of content items, or can rely on a back-end asynchronous infrastructure (for example hosted on Microsoft Azure) to process high volume of transformation tasks.

## Transformation process
Internally, the transformation process relies on a a set of mapping providers. For every data source there is a basic mapping provider object, which implements the **IMappingProvider** interface, and there could be specialized mapping providers for the main contents that need to be transformed. Natively, you can find the following mapping providers:
- Html Mapping Provider: to map HTML content from the data source to SharePoint Online.
- Metadata Mapping Provider: to map metadata fields from the data source to SharePoint Online.
- Page Layout Mapping Provider: to map page layouts from the data source to SharePoint Online.
- Taxonomy Mapping Provider: to map taxonomy terms from the data source to SharePoint Online.
- Url Mapping Provider: to map URLs from the data source to SharePoint Online.
- User Mapping Provider: to map user accounts from the data source to SharePoint Online.
- Web Part Mapping Provider: to map content controls/components from the data source  into web parts of SharePoint Online.

The content transformation process goes through the source content and applies all the mappings to generate the transformed modern page.

## Getting started
In order to start using the Transformation Framework, you can read the ["Getting started"](./getting-started.md) guide. 