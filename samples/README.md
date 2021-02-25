# PnP Core Samples

Samples are best way to learn how to use, hence we've some example PnP Core SDK sample applications for you to learn. Following samples are available:

Sample | Description | .Net version | PnP Core version | Authentication Type
-------|-------------|-----------------|-------------|----------------
[Demo.Console](Demo.Console/README.md) | Demo console app that shows how to use the PnP Core SDK for working with Microsoft 365 data | 5.0.0 | 0.6.1-beta3 | Interactive login (`InteractiveAuthenticationProvider`)
[Demo.ASPNetCore](Demo.ASPNetCore/README.md) | Demo application showing how use the PnP Core SDK from an ASP.Net Core application | | beta3 | Custom authentication is used via the (`ExternalAuthenticationProvider`)
[Demo.AzureFunction](Demo.AzureFunction/README.md) | Azure Function that shows how to use the PnP Core SDK via dependency injection | | | Application permissions (`X509CertificateAuthenticationProvider`)
[Demo.Blazor](Demo.Blazor/README.md) | Sample Blazor WASM application that prototypes how the PnP Core SDK can be used in Blazor development | 5.0.0 | beta3 | Custom `IAuthenticationProvider` implementation
[Demo.WPF](Demo.WPF/README.md) | Sample WPF windows application showing how dependency injection and the PnP Core SDK can be used in WPF/XAML apps | | | Interactive login (`InteractiveAuthenticationProvider`) 
[Demo.RPi](Demo.RPi/README.md) | Sample application running on [dotNet](https://dotnet.microsoft.com/download/dotnet-core/3.1) Core on the Raspberry Pi | | | Username password login (`UsernamePasswordAuthenticationProvider`)

Checkout the **README.md** file from each sample to learn more.
