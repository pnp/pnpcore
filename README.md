# PnP Core SDK

![Build and Test](https://github.com/pnp/pnpcore/workflows/Build%20and%20Test/badge.svg?branch=dev) ![Refresh documentation](https://github.com/pnp/pnpcore/workflows/Refresh%20documentation/badge.svg?branch=dev) ![Nightly nuget release](https://github.com/pnp/pnpcore/workflows/Nightly%20nuget%20release/badge.svg?branch=dev) 

Nuget package | Description | Latest development version
--------------|-------------|---------------------------
PnP.Core | The PnP Core SDK | [![PnP.Core Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.svg)](https://www.nuget.org/packages/PnP.Core/) 
PnP.Core.Auth | The authentication provider for the PnP Core SDK | [![PnP.Core.Auth Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/)

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying API's being called. Currently the library is in **preview**, see our roadmap for more details. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

> **Important**
> The PnP Core SDK is in preview: it's targeting developers that want to either test or extend it. It's **not yet** ready to be used in production scenarios, mainly because it still lacks some of the model definitions and the object model might still have breaking changes.

For more details on how to use this SDK and how to contribute checkout https://aka.ms/pnp/coresdk/docs.

## Roadmap

This is a community effort, hence we cannot guarantee below roadmap but rest assure, we're working hard to stick to plan :-) If you want to join our team and help realize this, then checkout the [How can I help](https://pnp.github.io/pnpcore/#how-can-you-help) section in our docs.

- Preview 3: September 2020 (shipped)
- Beta 1: November 2020
- Beta 2: December 2020
- V1: January 2021

## Frequently Asked Questions

### Will this work in modern .Net

Absolutely! One of the key reasons for building PnP Core SDK is to nicely fit into modern .Net development:

- We currently target .Net Standard 2.0, once [.Net 5.0](https://devblogs.microsoft.com/dotnet/introducing-net-5/) is available we'll also ship a [.Net 5.0](https://devblogs.microsoft.com/dotnet/introducing-net-5/) version
- This library will work cross platform (Windows, Linux, MacOS)
- This library will work in all places where .Net will work (see [our samples](src/samples) to learn more):
  - Backend: e.g. [Azure functions v3](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library)
  - Web: e.g. [ASP.Net core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1), [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
  - Browser (WebAssemby (WASM)): [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
  - Windows Client: [Windows Forms, WPF](https://docs.microsoft.com/en-us/dotnet/desktop/?view=netdesktop-5.0)
  - Mobile: [Xamarin](https://dotnet.microsoft.com/apps/xamarin)
  
- The library internally uses [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) and you can consume it via dependency injection in your applications

### What's the relationship with the existing PnP Sites Core / PnP Framework libraries

The [PnP Sites Core library](https://github.com/PnP/PnP-Sites-Core) is very popular library that extends SharePoint using mainly CSOM. This library contains the PnP Provisioning engine, tons of extension methods, a modern page API, etc...but this library has also organically grown into a complex and hard to maintain code base. One of the reasons why the PnP Core SDK development started is to provide a new clean basis for the PnP Sites Core library with a strong focus on quality (test coverage above 80%, automation). As this transition will take quite some time and effort we plan to gradually move things over from PnP Sites Core to the PnP Core SDK. The first step in this transition is releasing a .Net Standard 2.0 version of PnP Sites Core, called [PnP Framework](https://github.com/pnp/pnpframework). Going forward [PnP Framework](https://github.com/pnp/pnpframework) features will move to the PnP Core SDK in a phased approach. At this moment we've shipped our first [PnP Framework](https://github.com/pnp/pnpframework) preview version and preview 3 of the [PnP Core SDK](https://github.com/pnp/pnpcore).

![PnP dotnet roadmap](PnP%20dotnet%20Roadmap%20-%20October%20status.png)

### What underlying API's are used

The SDK provides an object model that's API agnostic, when you as a developer for example load a SharePoint List the SDK by default will use [Microsoft Graph](https://docs.microsoft.com/en-us/graph/). If however you're loading List properties that cannot be provided via [Microsoft Graph](https://docs.microsoft.com/en-us/graph/) the SDK wil issue a SharePoint REST call. Depending on the needs the SDK will use Microsoft Graph Beta calls (e.g. for Taxonomy support) and in some rare cases the CSOM endpoint (client.svc) is called. But the good thing is that all of this is transparent for you as developer! You'll have a consistent development experience regardless of the underlying API's being called.

As [Microsoft Graph](https://docs.microsoft.com/en-us/graph/) is our recommended API to work with Microsoft 365, the SDK will always use Microsoft Graph whenever possible, the "fall back" to SharePoint REST only happens when Microsoft Graph can't provide the needed data.

**Community rocks, sharing is caring!**

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
