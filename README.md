
<h1 align="center">
  <a href="https://pnp.github.io/pnpcore">
    <img alt="PnP Core SDK" src="./docs/pnp-core-sdk-red.svg" height="100">
  </a>
</h1>

The PnP Core SDK is an SDK designed to work for Microsoft 365. It provides a unified object model for working with SharePoint Online and Teams which is agnostic to the underlying APIs being called. The initial goal for this library will be to cover the needs of developers working with either SharePoint Online or Teams, but we're also open to extend this library further towards other Microsoft 365 workloads if there's community demand for doing so.

## Packages and status 🏭

![Build and Test](https://github.com/pnp/pnpcore/workflows/Build%20and%20Test/badge.svg?branch=dev) [![codecov](https://codecov.io/gh/jansenbe/pnpcore/branch/dev/graph/badge.svg?token=FL0EY8DRPQ)](https://codecov.io/gh/jansenbe/pnpcore) ![Refresh documentation](https://github.com/pnp/pnpcore/workflows/Refresh%20documentation/badge.svg?branch=dev) ![Nightly nuget release](https://github.com/pnp/pnpcore/workflows/Nightly%20nuget%20release/badge.svg?branch=dev)

Nuget package |  Downloads | Stable | Preview 
--------------|------------|--------|--------
[PnP.Core](https://pnp.github.io/pnpcore/using-the-sdk/readme.html) | [![Downloads](https://img.shields.io/nuget/dt/pnp.core.svg)](https://www.nuget.org/packages/PnP.Core/) | [![PnP.Core Nuget package](https://img.shields.io/nuget/v/PnP.Core.svg)](https://www.nuget.org/packages/PnP.Core/) | [![PnP.Core Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.svg)](https://www.nuget.org/packages/PnP.Core/)
[PnP.Core.Auth](https://pnp.github.io/pnpcore/using-the-sdk/configuring%20authentication.html) | [![Downloads](https://img.shields.io/nuget/dt/pnp.core.auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/) |[![PnP.Core.Auth Nuget package](https://img.shields.io/nuget/v/PnP.Core.Auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/) | [![PnP.Core.Auth Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Auth.svg)](https://www.nuget.org/packages/PnP.Core.Auth/)
[PnP.Core.Admin](https://pnp.github.io/pnpcore/using-the-sdk/admin-sharepoint-tenant.html) | [![Downloads](https://img.shields.io/nuget/dt/pnp.core.admin.svg)](https://www.nuget.org/packages/PnP.Core.Admin/) | [![PnP.Core.Admin Nuget package](https://img.shields.io/nuget/v/PnP.Core.Admin.svg)](https://www.nuget.org/packages/PnP.Core.Admin/) | [![PnP.Core.Admin Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Admin.svg)](https://www.nuget.org/packages/PnP.Core.Admin/)
[PnP.Core.Transformation](https://pnp.github.io/pnpcore/using-the-sdk/transformation-getting-started.html) | [![Downloads](https://img.shields.io/nuget/dt/pnp.core.transformation.svg)](https://www.nuget.org/packages/PnP.Core.Transformation/) | soon | [![PnP.Core.Transformation Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Transformation.svg)](https://www.nuget.org/packages/PnP.Core.Transformation/)
[PnP.Core.Transformation.SharePoint](https://pnp.github.io/pnpcore/using-the-sdk/transformation-getting-started.html) | [![Downloads](https://img.shields.io/nuget/dt/pnp.core.transformation.sharepoint.svg)](https://www.nuget.org/packages/PnP.Core.Transformation.sharepoint) | soon | [![PnP.Core.Transformation Nuget package](https://img.shields.io/nuget/vpre/PnP.Core.Transformation.sharepoint.svg)](https://www.nuget.org/packages/PnP.Core.Transformation.sharepoint/)
## Getting started 🚀

For more details on how to get started with the PnP.Core SDK checkout our [documentation](https://pnp.github.io/pnpcore/using-the-sdk/readme.html).

## I want to help 🙋‍♂️

If you want to join our team and help, then checkout the [How can I help](https://pnp.github.io/pnpcore/#how-can-you-help) section in our docs.

## Supportability and SLA 💁🏾‍♀️

This library is an open-source and community provided library backed by an active community supporting it. This is not a Microsoft provided library, so there's no SLA or direct support for this open-source component from Microsoft. Please report any issues using the [issues list](https://github.com/pnp/pnpcore/issues).

## Frequently Asked Questions ❓

### Will this work in modern .NET

Absolutely! One of the key reasons for building PnP Core SDK is to nicely fit into modern .NET development:

- We currently target .NET Standard 2.0, [.NET 5.0](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/), [.NET 6.0](https://devblogs.microsoft.com/dotnet/announcing-net-6/) and [.NET 7.0](https://devblogs.microsoft.com/dotnet/announcing-dotnet-7/). Use the .NET 6.0 (LTS) or .NET 7.0 builds if you're using a modern .NET version, use .NET Standard 2.0 for backwards compatibility with .NET Framework 4.6.1+. Note that .NET 5.0 support will be dropped with version 1.9 as it's an unsupported .NET version
- This library will work cross platform (Windows, Linux, MacOS)
- This library will work in all places where .NET will work (see [our samples](https://pnp.github.io/pnpcore/demos/README.html) to learn more):
  - Backend: e.g. [Azure functions v3/v4](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library)
  - Web: e.g. [ASP.NET core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-3.1), [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
  - Browser (WebAssembly (WASM)): [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor). We only support [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) with our .NET 5.0, .NET 6.0 and .NET 7.0 versions
  - .NET MAUI: [Windows, iOS, macOS, Android](https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui?view=net-maui-7.0)
  - Windows Client: [Windows Forms, WPF](https://docs.microsoft.com/en-us/dotnet/desktop/?view=netdesktop-5.0)
  - Mobile: [Xamarin](https://dotnet.microsoft.com/apps/xamarin)
  
- The library internally uses [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1) and you can consume it via dependency injection in your applications

### What underlying APIs are used

The SDK provides an object model that's API agnostic, when you as a developer for example load a model (List, Team, Web,...) the SDK will use the best possible API for loading that model being for the most part Microsoft Graph v1.0 and SharePoint REST. Depending on the needs the SDK will use Microsoft Graph Beta calls and in some cases the CSOM endpoint (client.svc) is called. But the good thing is that all of this is transparent for you as developer! You'll have a consistent development experience regardless of the underlying APIs being called.

As [Microsoft Graph](https://docs.microsoft.com/en-us/graph/) is our recommended API to work with Microsoft 365, the SDK will favor Microsoft Graph whenever it makes sense, the "fall back" to SharePoint REST only happens when Microsoft Graph can't provide the needed data or consistency.

### What's the relationship with the existing PnP Framework library

The [PnP Framework](https://github.com/pnp/pnpframework) library is very popular library that extends SharePoint using mainly CSOM. This library contains the PnP Provisioning engine, tons of extension methods, etc...but this library has also organically grown into a complex and hard to maintain code base. One of the reasons why the PnP Core SDK development started is to provide a new clean replacement for PnP Framework with a strong focus on quality (test coverage above 80%, automation). As this transition will take quite some time and effort, we plan to gradually move things over from PnP Framework to the PnP Core SDK. Going forward [PnP Framework](https://github.com/pnp/pnpframework) features will move to the PnP Core SDK in a phased approach.

**Community rocks, sharing is caring!**

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

<img src="https://m365-visitor-stats.azurewebsites.net/pnpcoresdk/readme?labelText=Visitors" />
