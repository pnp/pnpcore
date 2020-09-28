# PnP Core Samples

Samples are best way to learn how to use, hence we've some simple PnP Core SDK sample applications for you to learn. Following samples are available:

Sample | Description | Authentication
-------|-------------|----------------
Demo.Console | Demo console app that shows how to sue the PnP Core SDK for working with Microsoft 365 data | Interactive login (`InteractiveAuthenticationProvider`)
Demo.ASPNetCore | Demo application showing how use the PnP Core SDK from an ASP.Net Core application | Credential manager based auth (`CredentialManagerAuthenticationProvider`)
Demo.AzureFunction | Azure Function that shows how to use the PnP Core SDK via dependency injection | Application permissions (`X509CertificateAuthenticationProvider`)
Demo.Blazor | Sample Blazor WASM application that prototypes how the PnP Core SDK can be used in Blazor development | Custom `IAuthenticationProvider` implementation
Demo.WPF | Sample WPF windows application showing how dependency injection and the PnP Core SDK can be used in WPF/XAML apps | Interactive login (`InteractiveAuthenticationProvider`) 
Demo.RPi | Sample application running on [dotNet](https://dotnet.microsoft.com/download/dotnet-core/3.1) Core on the Raspberry Pi | Username password login (`UsernamePasswordAuthenticationProvider`)

Checkout the **readme.md** file from each sample to learn more.
