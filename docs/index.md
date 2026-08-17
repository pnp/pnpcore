# PnP Core SDK

## Intro

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying APIs being called. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

## Getting started using this library

Using the PnP Core SDK is simple, check out the [getting started](./using-the-sdk/readme.md) guide.

## How this project relates to PnP Framework

The PnP Framework is a .Net Framework based library that has been around for many years and has been used in many production scenarios. The PnP Core SDK is a new library that is designed to be used in modern .Net development. Currently PnP Core SDK is a dependency of the PnP Framework. In Future we plan to deprecate the PnP Framework and have the PnP Core SDK as the main library for working with Microsoft 365 workloads.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub repository: https://github.com/pnp/pnpcore.

## Supportability and SLA

This library is open-source and community provided component with active community providing support for it. This is not Microsoft provided component so there's no SLA or direct support for this open-source component from Microsoft. Please report any issues using the [issues list](https://github.com/pnp/pnpcore/issues).

## How can you help?

If you want to help, there are several ways to do so:

- **Extending the model** via the creation of new model, complex type and collection classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Adding "functionalities"** to the model: after extending the model, the typical CRUD functionality is there, but for some parts of the model it makes sense to add functionalities by adding methods on top of the model classes. Check out the [Extending the model](~/contributing/readme.md) article to learn how to do this.
- **Writing sample programs** that use this library: since this is a .Net Standard library, it doesn't target Windows only, but also macOS and Linux, as well as mobile OS's like Android and iOS. With the rise of [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), this library can also be used to build .Net based apps running in the browser. Samples can be added in the [samples](https://github.com/pnp/pnpcore/tree/dev/samples) folder.
- **Working on documentation**: good documentation is critical to help developers work with this library, either by using it or by extending it. See the [Writing documentation](~/contributing/writing%20documentation.md) article for more details.

### Best practices when contributing

We love to accept contributions. If you want to get involved in helping us grow the PnP Core SDK we would love to hear from you.

Before you submit your first PR, please follow the below best practices. We'd hate to see you work on something that someone else is already working on, something that we agreed not to do, or something that doesn't match the project.

We're here to help, don't hesitate to reach out with your questions and feedback via [our issue list](https://github.com/pnp/pnpcore/issues) or [our discussions](https://github.com/pnp/pnpcore/discussions).

To avoid overlapping efforts let us know if you plan to take on a particular chunk of work (e.g. let's say you want to extend the model to support Planner) If you are extending the model and you need to update the model base logic then please reach out, we want to align and agree on those changes first as these need to fit into our model approach

**DO's & DON'Ts**

- **DO** follow the same project and test structure as the existing project.
- **DO** include tests when adding new functionality and features. When fixing bugs, start by adding a test that highlights how the current behavior is broken.
- **DO** keep discussions focused. When a new or related topic comes up, it's often better to create a new issue than to side-track the conversation.
- **DO NOT** submit PRs for coding style changes. These changes can cause unnecessary conflicts and make the review process more complicated.
- **DO NOT** surprise us with big PRs. Instead, file an issue and start a discussion so we can agree on a direction before you invest a large amount of time.
- **DO NOT** commit code you didn't write. We encourage you to contribute your own work to maintain project quality and integrity.

> [!Note]
> The best way to reach out is by creating an issue in the issue list: https://github.com/pnp/pnpcore/issues

**This community rocks, sharing is caring!**
