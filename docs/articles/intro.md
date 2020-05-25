# PnP Core SDK - preview

## Intro

The PnP Core SDK is an SDK designed to work against Microsoft 365. It provides a unified object model for working with SharePoint Online and the broader Microsoft 365 environment. Currently the library is an **early preview** in which the focus was on coding the basics so that in a next step the actual extending of the library can go smooth. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but obviously we're also open to extend this library further towards other Microsoft 365 components such as Mail, Planner, Yammer, etc.

> [!Important]
> The PnP Core SDK is in preview: it's targetting developers that want to either test or extend it. It's **not** yet ready to be used in production scenarios, mainly because it still lacks most of the model definitions.

## Getting started using this library

Using the PnP Core SDK is simple, check out out [getting started](consumer/readme.md) guide.

## Where is the code?

The PnP Core SDK is maintained in the PnP GitHub organization: https://github.com/pnp/pnpcore.

## How can you help?

The model implemented in the preview only covers a small part of what's possible and needed. We're looking for folks that want to help us expand the model by:

- **Extending the model** via the creation of new model, complex type and collection classes. Check out the [Extending the model](contributor/readme.md) article to learn how to do this
- **Adding "functionality"** to the model: after extending the model the typical CRUD functionality is there, but for some parts of the model it makes sense to add functionality by adding methods on top of the model classes. Check out the [Extending the model](contributor/readme.md) article to learn how to do this
- **Writing sample programs** that use this library: since this is .Net Standard library it can not only be used on Windows, but also on Macs and Linux machines as well as on mobile OS's like Android and iOS. With the rise of [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) this library can also be used to build .Net based apps running in the browser. Samples can be added in the `src\samples` folder.
- **Working on documentation**: good documentation is critical to help developers work with this library, either by using it or by extending it. See the [Writing documentation](contributor/writing%20documentation.md) article for more details

### Best practices when contributing

- This is all new and we're there to help, don't hesitate to reach out with your questions and feedback
- To avoid overlapping efforts in the startup phase please let us know if you plan to take on a particular chunk of work (e.g. let's say you want to extend the model to support Planner)
- If you are extending the model and you need to update the model base logic then please reach out, we want to align on those changes first as these need to fit into our model approach

> [!Note]
> The best way to reach out is by creating an issue in the issue list: https://github.com/pnp/pnpcore/issues

**This community rocks, sharing is caring!**