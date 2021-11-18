# PnP Core Samples

Samples are best way to learn how to use, hence we've some example PnP Core SDK sample applications for you to learn. Following samples are available:

## Azure function samples

Sample | Description | .NET version | PnP Core version | Authentication Type | Function mode
-------|-------------|--------------|------------------|---------------------|--------------
[Demo.AzureFunction.OutOfProcess.AppOnly](Demo.AzureFunction.OutOfProcess.AppOnly/README.md) | Azure v4 Function using PnP Core SDK via dependency injection showing how to create and configure a site collection | .NET 6.0 | v1.4.0 | Application permissions (`X509CertificateAuthenticationProvider`) | V4, isolated process
[Demo.AzureFunction](Demo.AzureFunction/README.md) | Azure v3 Function that shows how to use the PnP Core SDK via dependency injection | .NET Core 3.1 | v1.4.0 | Application permissions (`X509CertificateAuthenticationProvider`) | V3, in-process

Checkout the **README.md** file from each sample to learn more.

## Console applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[Demo.Console](Demo.Console/README.md) | Demo console app that shows how to use the PnP Core SDK for working with Microsoft 365 data | .NET 5.0.0 | v1.4.0 | Interactive login (`InteractiveAuthenticationProvider`)
[Demo.PersistentTokenCache](Demo.PersistentTokenCache/README.md) | Demo console app that shows how to use the PnP Core SDK with a custom authentication provider that utilizes an MSAL token cache avoiding credential prompts after first login | 5.0.0 | v1.4.0 | Custom `IAuthenticationProvider` implementation
[Demo.RPi](Demo.RPi/README.md) | Sample application running on [dotNet](https://dotnet.microsoft.com/download/dotnet-core/3.1) Core on the Raspberry Pi | .NET 5.0.0 | v1.4.0 | Username password login (`UsernamePasswordAuthenticationProvider`)

Checkout the **README.md** file from each sample to learn more.

## Web applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[Demo.ASPNetCore](Demo.ASPNetCore/README.md) | Demo application showing how use the PnP Core SDK from an ASP.Net Core application | .NET 5.0.0 | v1.4.0 | Custom authentication is used via the (`ExternalAuthenticationProvider`)
[Demo.Blazor](Demo.Blazor/README.md) | Sample Blazor WASM application that prototypes how the PnP Core SDK can be used in Blazor development | .NET 5.0.0 | v1.4.0 | Custom `IAuthenticationProvider` implementation

Checkout the **README.md** file from each sample to learn more.

## Desktop applications

Sample | Description | .NET version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[Demo.WPF](Demo.WPF/README.md) | Sample WPF windows application showing how dependency injection and the PnP Core SDK can be used in WPF/XAML apps | .NET 5.0.0 | v1.4.0 | Interactive login (`InteractiveAuthenticationProvider`)

Checkout the **README.md** file from each sample to learn more.
