# PnP Core SDK

![Build and Test](https://github.com/pnp/pnpcore/workflows/Build%20and%20Test/badge.svg?branch=dev) [![codecov](https://codecov.io/gh/jansenbe/pnpcore/branch/dev/graph/badge.svg?token=FL0EY8DRPQ)](https://codecov.io/gh/jansenbe/pnpcore) ![Refresh documentation](https://github.com/pnp/pnpcore/workflows/Refresh%20documentation/badge.svg?branch=dev) ![Nightly nuget release](https://github.com/pnp/pnpcore/workflows/Nightly%20nuget%20release/badge.svg?branch=dev)

Nuget package | Description | Latest version | Latest nightly development version
--------------|-------------|----------------|------------------------------------
PnP.Core | The PnP Core SDK | [![PnP.Core Nuget package](https://img.shields.io/nuget/v/PnP.Core.svg)](https://www.nuget.org/packages/PnP.Core/) | [![PnP.Core Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.svg)](https://www.nuget.org/packages/PnP.Core/)
PnP.Core.Auth | The authentication provider for the PnP Core SDK | [![PnP.Core.Auth Nuget package](https://img.shields.io/nuget/v/PnP.Core.Auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/) | [![PnP.Core.Auth Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/)
PnP.Core.Admin | Microsoft 365 admin features for the PnP Core SDK | soon | [![PnP.Core.Admin Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Admin.svg)](https://www.nuget.org/packages/PnP.Core.Admin/)

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying APIs being called. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

> **Important**
>
> - If you're upgrading from PnP Core SDK Beta 2 or before then please check [these instructions](https://pnp.github.io/pnpcore/using-the-sdk/upgrade-to-beta3.html) on how to upgrade your code to the current version.

For more details on how to use this SDK and how to contribute checkout https://aka.ms/pnp/coresdk/docs.

## I want to help 🙋‍♂️

If you want to join our team and help, then checkout the [How can I help](https://pnp.github.io/pnpcore/#how-can-you-help) section in our docs.

## Frequently Asked Questions ❓

### Will this work in modern .NET

Absolutely! One of the key reasons for building PnP Core SDK is to nicely fit into modern .NET development:

- We currently target .NET Standard 2.0 and [.NET 5.0](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/). Use the .NET 5.0 build if you're using a modern .NET version, use .NET Standard 2.0 for backwards compatibility with .NET Framework 4.6.1+
- This library will work cross platform (Windows, Linux, MacOS)
- This library will work in all places where .NET will work (see [our samples](src/samples) to learn more):
  - Backend: e.g. [Azure functions v3](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library)
  - Web: e.g. [ASP.NET core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1), [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
  - Browser (WebAssembly (WASM)): [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). We only support [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) with our .NET 5.0 version
  - Windows Client: [Windows Forms, WPF](https://docs.microsoft.com/en-us/dotnet/desktop/?view=netdesktop-5.0)
  - Mobile: [Xamarin](https://dotnet.microsoft.com/apps/xamarin)
  
- The library internally uses [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) and you can consume it via dependency injection in your applications

### What's the relationship with the existing PnP Sites Core / PnP Framework libraries

The [PnP Sites Core library](https://github.com/PnP/PnP-Sites-Core) is very popular library that extends SharePoint using mainly CSOM. This library contains the PnP Provisioning engine, tons of extension methods, a modern page API, etc...but this library has also organically grown into a complex and hard to maintain code base. One of the reasons why the PnP Core SDK development started is to provide a new clean basis for the PnP Sites Core library with a strong focus on quality (test coverage above 80%, automation). As this transition will take quite some time and effort we plan to gradually move things over from PnP Sites Core to the PnP Core SDK. The first step in this transition was releasing a .NET Standard 2.0 / .NET 5.0 version of PnP Sites Core, called [PnP Framework](https://github.com/pnp/pnpframework). Going forward [PnP Framework](https://github.com/pnp/pnpframework) features will move to the PnP Core SDK in a phased approach.

### What underlying APIs are used

The SDK provides an object model that's API agnostic, when you as a developer for example load a model (List, Team, Web,...) the SDK will use the best possible API for loading that model being for the most part Microsoft Graph v1.0 and SharePoint REST. Depending on the needs the SDK will use Microsoft Graph Beta calls (e.g. for Taxonomy support) and in some rare cases the CSOM endpoint (client.svc) is called. But the good thing is that all of this is transparent for you as developer! You'll have a consistent development experience regardless of the underlying APIs being called.

As [Microsoft Graph](https://docs.microsoft.com/en-us/graph/) is our recommended API to work with Microsoft 365, the SDK will favor Microsoft Graph whenever it makes sense, the "fall back" to SharePoint REST only happens when Microsoft Graph can't provide the needed data or consistency.

**Community rocks, sharing is caring!**

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
