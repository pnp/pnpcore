# PnP Core SDK

## Intro

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying APIs being called. Currently the library is in **beta**, see our roadmap for more details. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

> [!Important]
> The PnP Core SDK is in beta: it's ready for developers to start using in real life scenarios. Between now and GA there might be small breaking changes, but we try to minimize those as much as possible, focus will be on finalizing our GA scope, stabilizing and documenting it.

## Roadmap

This is a community effort, hence we cannot guarantee below roadmap but rest assure, we're working hard to stick to plan 😀 If you want to join our team and help realize this, then checkout the [How can I help](https://pnp.github.io/pnpcore/#how-can-you-help) section in our docs.

- Preview 3: September 2020 (shipped)
- Beta 1: November 2020 (shipped)
- Beta 2: January 2021 (shipped)
- V1: February 2021

## Getting started using this library

Using the PnP Core SDK is simple, check out the [getting started](~/using-the-sdk/readme.md) guide.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore.

## How can you help?

The model implemented in the preview only covers a small part of what's possible and what's needed. We're asking the community to help us expand the model by:

- **Extending the model** via the creation of new model, complex type and collection classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Adding "functionalities"** to the model: after extending the model, the typical CRUD functionality is there, but for some parts of the model it makes sense to add functionalities by adding methods on top of the model classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Writing sample programs** that use this library: since this is a .Net Standard library, it doesn't target Windows only, but also macOS and Linux, as well as  mobile OS's like Android and iOS. With the rise of [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), this library can also be used to build .Net based apps running in the browser. Samples can be added in the [src\samples](https://github.com/pnp/pnpcore/tree/dev/src/samples) folder.
- **Working on documentation**: good documentation is critical to help developers work with this library, either by using it or by extending it. See the [Writing documentation](~/contributing/writing%20documentation.md) article for more details.

### Best practices when contributing

- This is all new and we're here to help, don't hesitate to reach out with your questions and feedback via [our issue list](https://github.com/pnp/pnpcore/issues)
- To avoid overlapping efforts in the startup phase please let us know if you plan to take on a particular chunk of work (e.g. let's say you want to extend the model to support Planner)
- If you are extending the model and you need to update the model base logic then please reach out, we want to align and agree on those changes first as these need to fit into our model approach

> [!Note]
> The best way to reach out is by creating an issue in the issue list: https://github.com/pnp/pnpcore/issues

**This community rocks, sharing is caring!**
