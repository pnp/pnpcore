using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using PnP.Core.Services;

namespace PnP.Core.Auth.Services.Builder.Configuration
{
    /// <summary>
    /// Options for configuring the PnP Core SDK Authentication Providers
    /// </summary>
    public class PnPCoreAuthenticationOptions
    {
        /// <summary>
        /// The Microsoft 365 cloud environment that's used
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Returns the Azure AD Login authority (e.g. login.microsoftonline.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string AzureADLoginAuthority { get; set; }

        /// <summary>
        /// The sites options
        /// </summary>
        public PnPCoreAuthenticationSitesOptions Sites { get; } = new PnPCoreAuthenticationSitesOptions();

        /// <summary>
        /// The credential options
        /// </summary>
        public PnPCoreAuthenticationCredentialsOptions Credentials { get; set; } = new PnPCoreAuthenticationCredentialsOptions();
    }

    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreAuthenticationSitesOptions : Dictionary<string, PnPCoreAuthenticationSiteOptions>
    {
    }

    /// <summary>
    /// Options for configuring a single site in PnP Core SDK
    /// </summary>
    public class PnPCoreAuthenticationSiteOptions
    {
        /// <summary>
        /// The Authentication Provider name
        /// </summary>
        public string AuthenticationProviderName { get; set; }
    }

    /// <summary>
    /// Defines a collection of credential settings for secure connections to the target resources
    /// </summary>
    public class PnPCoreAuthenticationCredentialsOptions
    {
        /// <summary>
        /// Defines the name of the default configuration
        /// </summary>
        public string DefaultConfiguration { get; set; }

        /// <summary>
        /// The credentials options
        /// </summary>
        public PnPCoreAuthenticationCredentialsConfigurationsOptions Configurations { get; } = new PnPCoreAuthenticationCredentialsConfigurationsOptions();
    }

    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreAuthenticationCredentialsConfigurationsOptions : Dictionary<string, PnPCoreAuthenticationCredentialConfigurationOptions>
    {
    }

    /// <summary>
    /// Options for one configured credentials
    /// </summary>
    public class PnPCoreAuthenticationCredentialConfigurationOptions
    {
        /// <summary>
        /// The ClientId of the application to use for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Tenant ID for the application, can be null
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The Microsoft 365 cloud environment that's used
        /// </summary>
        public Microsoft365Environment? Environment { get; set; }

        /// <summary>
        /// Returns the Azure AD Login authority (e.g. login.microsoftonline.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string AzureADLoginAuthority { get; set; }
        
        /// <summary>
        /// The options for the X509 Certificate Authentication Provider
        /// </summary>
        public PnPCoreAuthenticationX509CertificateOptions X509Certificate { get; set; }

        /// <summary>
        /// The options for the Credential Manager Authentication Provider
        /// </summary>
        public PnPCoreAuthenticationCredentialManagerOptions CredentialManager { get; set; }

        /// <summary>
        /// The options for the On Behalf Of Authentication Provider
        /// </summary>
        public PnPCoreAuthenticationOnBehalfOfOptions OnBehalfOf { get; set; }

        /// <summary>
        /// The options for the Username and Password Authentication Provider
        /// </summary>
        public PnPCoreAuthenticationUsernamePasswordOptions UsernamePassword { get; set; }

        /// <summary>
        /// The options for the Interactive Authentication Provider
        /// </summary>
        public PnPCoreAuthenticationInteractiveOptions Interactive { get; set; }

        /// <summary>
        /// The options for the Device Code Provider
        /// </summary>
        public PnPCoreAuthenticationDeviceCodeOptions DeviceCode { get; set; }
    }

    /// <summary>
    /// Options for the X509CertificateAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationX509CertificateOptions : PnPCoreAuthenticationBaseOptions
    {
        /// <summary>
        /// The name of the certificate store
        /// </summary>
        public StoreName StoreName { get; set; }

        /// <summary>
        /// The location in the certificate store
        /// </summary>
        public StoreLocation StoreLocation { get; set; }

        /// <summary>
        /// The thumbprint of the certificate
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// Certificate can be set directly, if required
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
    }

    /// <summary>
    /// Options for the CredentialManagerAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationCredentialManagerOptions : PnPCoreAuthenticationBaseOptions
    {
        /// <summary>
        /// The name of the Windows Credential Manager settings to use
        /// </summary>
        public string CredentialManagerName { get; set; }
    }

    /// <summary>
    /// Options for the OnBehalfOfAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationOnBehalfOfOptions : PnPCoreAuthenticationBaseOptions
    {
        /// <summary>
        /// The ClientSecret to authenticate the app with ClientId
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The name of the certificate store
        /// </summary>
        public StoreName StoreName { get; set; }

        /// <summary>
        /// The location in the certificate store
        /// </summary>
        public StoreLocation StoreLocation { get; set; }

        /// <summary>
        /// The thumbprint of the certificate
        /// </summary>
        public string Thumbprint { get; set; }
        
        /// <summary>
        /// Certificate can be set directly, if required
        /// </summary>
        public X509Certificate2 Certificate { get; set; }
    }

    /// <summary>
    /// Options for the UsernamePasswordAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationUsernamePasswordOptions : PnPCoreAuthenticationBaseOptions
    {
        /// <summary>
        /// The username for authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password for authentication
        /// </summary>
        public string Password { get; set; }
    }


    /// <summary>
    /// Options for the InteractiveAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationInteractiveOptions : PnPCoreAuthenticationBaseOptions { }

    /// <summary>
    /// Options for the DeviceCodeAuthenticationProvider
    /// </summary>
    public class PnPCoreAuthenticationDeviceCodeOptions : PnPCoreAuthenticationBaseOptions { }

    /// <summary>
    /// Options for the DeviceCodeAuthenticationProvider
    /// </summary>
    public abstract class PnPCoreAuthenticationBaseOptions
    {
        /// <summary>
        /// The Redirect URI for the authentication flow
        /// </summary>
        public Uri RedirectUri { get; set; }

        /// <summary>
        /// The Authority URI for the authentication flow
        /// </summary>
        public Uri AuthorityUri { get; set; }
    }
}
