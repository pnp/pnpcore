# PnP Core SDK

## Intro

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying APIs being called. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

> [!Important]
>
> - If you've used PnP Core SDK in a version before Beta3 then please check [these instructions](https://pnp.github.io/pnpcore/using-the-sdk/upgrade-to-beta3.html) on how to upgrade your code to the current version.

## Getting started using this library

Using the PnP Core SDK is simple, check out the [getting started](./using-the-sdk/readme.md) guide.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore.

## Supportability and SLA

This library is open-source and community provided component with active community providing support for it. This is not Microsoft provided component so there's no SLA or direct support for this open-source component from Microsoft. Please report any issues using the [issues list](https://github.com/pnp/pnpcore/issues).

## How can you help?

The model implemented in the preview only covers a small part of what's possible and what's needed. We're asking the community to help us expand the model by:

- **Extending the model** via the creation of new model, complex type and collection classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Adding "functionalities"** to the model: after extending the model, the typical CRUD functionality is there, but for some parts of the model it makes sense to add functionalities by adding methods on top of the model classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Writing sample programs** that use this library: since this is a .Net Standard library, it doesn't target Windows only, but also macOS and Linux, as well as  mobile OS's like Android and iOS. With the rise of [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), this library can also be used to build .Net based apps running in the browser. Samples can be added in the [samples](https://github.com/pnp/pnpcore/tree/dev/samples) folder.
- **Working on documentation**: good documentation is critical to help developers work with this library, either by using it or by extending it. See the [Writing documentation](~/contributing/writing%20documentation.md) article for more details.

### Best practices when contributing

- This is all new and we're here to help, don't hesitate to reach out with your questions and feedback via [our issue list](https://github.com/pnp/pnpcore/issues) or [our discussions](https://github.com/pnp/pnpcore/discussions).
- To avoid overlapping efforts in the startup phase please let us know if you plan to take on a particular chunk of work (e.g. let's say you want to extend the model to support Planner)
- If you are extending the model and you need to update the model base logic then please reach out, we want to align and agree on those changes first as these need to fit into our model approach

> [!Note]
> The best way to reach out is by creating an issue in the issue list: https://github.com/pnp/pnpcore/issues

**This community rocks, sharing is caring!**

<!-- <img src="https://m365-visitor-stats.azurewebsites.net/pnpcoresdk/docs" /> -->
