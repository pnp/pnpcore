# PnP Core Samples

Samples are best way to learn how to use, hence we've some example PnP Core SDK sample applications for you to learn. Following samples are available:

## Azure functions

Sample | Description | .NET version | PnP Core version | Authentication Type | Function mode
-------|-------------|--------------|------------------|---------------------|--------------
[Out of process](Demo.AzureFunction.OutOfProcess.AppOnly/readme.md) | Azure v4 Function using PnP Core SDK via dependency injection showing how to create and configure a site collection | .NET 6 | v1.4.0 | Application permissions (`X509CertificateAuthenticationProvider`) | V4, isolated process
[V3 function](Demo.AzureFunction/README.md) | Azure v3 Function that shows how to use the PnP Core SDK via dependency injection | .NET Core 3.1 | v1.4.0 | Application permissions (`X509CertificateAuthenticationProvider`) | V3, in-process

> [!Important]
> Another very useful Azure Functions sample is described on Sergei Sergeev's blog [How to access SharePoint data from Azure Function with SPFx and PnP Core SDK](https://spblog.net/post/2020/12/10/how-to-access-sharepoint-data-from-azure-function-with-spfx-and-pnp-core). It shows how to setup an Azure Function using an on-behalf-of auth flow via the [OnBehalfOfAuthenticationProvider](https://pnp.github.io/pnpcore/api/PnP.Core.Auth.OnBehalfOfAuthenticationProvider.html) and then call this Azure Function from a [SharePoint Framework](https://docs.microsoft.com/en-us/sharepoint/dev/spfx/sharepoint-framework-overview) web part.

## Web applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[ASPNet Core site](Demo.ASPNetCore/README.md) | Demo application showing how use the PnP Core SDK from an ASP.Net Core application | .NET 5 | v1.4.0 | Custom authentication is used via the (`ExternalAuthenticationProvider`)
[ASPNET Blazor app](Demo.Blazor/README.md) | Sample Blazor WASM application that prototypes how the PnP Core SDK can be used in Blazor development | .NET 7 RC2 | v1.7.0 | Custom `IAuthenticationProvider` implementation

## Console applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[Minimal Console demo](Demo.Console.Minimal/readme.md) | Most simplistic console app that shows how to use the PnP Core SDK for working with Microsoft 365 data | .NET 6 | v1.4.0 | Interactive login (`InteractiveAuthenticationProvider`)
[Console demo](Demo.Console/README.md) | Demo console app that shows how to use the PnP Core SDK for working with Microsoft 365 data | .NET 6 | v1.5.0 | Interactive login (`InteractiveAuthenticationProvider`)
[Persistent TokenCache](Demo.PersistentTokenCache/README.md) | Demo console app that shows how to use the PnP Core SDK with a custom authentication provider that utilizes an MSAL token cache avoiding credential prompts after first login | .NET 5 | v1.4.0 | Custom `IAuthenticationProvider` implementation
[Pi demo](Demo.RPi/README.md) | Sample application running on [dotNet](https://dotnet.microsoft.com/download/dotnet-core/3.1) Core on the Raspberry Pi | .NET 5 | v1.4.0 | Username password login (`UsernamePasswordAuthenticationProvider`)

## Desktop applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[WPF app](Demo.WPF/README.md) | Sample WPF windows application showing how dependency injection and the PnP Core SDK can be used in WPF/XAML apps | .NET 5 | v1.4.0 | Interactive login (`InteractiveAuthenticationProvider`)
