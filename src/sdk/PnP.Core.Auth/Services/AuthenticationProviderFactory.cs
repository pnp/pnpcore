using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using System;
using System.Configuration;

namespace PnP.Core.Auth.Services
{
    /// <summary>
    /// Public factory service to create IAuthenticationProvider object instances
    /// </summary>
    public class AuthenticationProviderFactory : IAuthenticationProviderFactory
    {
        private readonly PnPCoreAuthenticationOptions options;
        private readonly ILogger log;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// <see cref="AuthenticationProviderFactory"/> constructor
        /// </summary>
        /// <param name="options">Options to configure the <see cref="AuthenticationProviderFactory"/></param>
        /// <param name="logger">Logger for log output</param>
        /// <param name="serviceProvider">DI container service</param>
        public AuthenticationProviderFactory(
            IOptionsMonitor<PnPCoreAuthenticationOptions> options,
            ILogger<PnPContext> logger,
            IServiceProvider serviceProvider)
        {
            // We need the options
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // We need the serviceProvider
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            // Store logger and options locally
            log = logger;
            this.serviceProvider = serviceProvider;
            this.options = options.CurrentValue;
        }

        /// <summary>
        /// Creates the default instance of IAuthenticationProvider based on the configuration
        /// </summary>
        /// <returns>An object that implements IAuthenticationProvider based on the configuration</returns>
        public IAuthenticationProvider CreateDefault()
        {
            if (string.IsNullOrEmpty(options.Credentials.DefaultConfiguration))
            {
                throw new ConfigurationErrorsException(PnPCoreAuthResources.Exception_MissingDefaultAuthenticationProvider);
            }

            // Return the Authentication Provider based on the default configuration
            return Create(options.Credentials.DefaultConfiguration);
        }

        /// <summary>
        /// Creates a new instance of IAuthenticationProvider based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the IAuthenticationProvider configuration to use</param>
        /// <returns>An object that implements IAuthenticationProvider based on the provided configuration name</returns>
        public IAuthenticationProvider Create(string name)
        {
            // Search for the provided configuration
            if (!this.options.Credentials.Configurations.TryGetValue(name, out PnPCoreAuthenticationCredentialConfigurationOptions options))
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        PnPCoreAuthResources.InvalidConfigurationName, name));
            }
            else
            {
                if (!string.IsNullOrEmpty(this.options.Environment))
                {
                    if (Enum.TryParse(this.options.Environment, out Microsoft365Environment environment))
                    {
                        options.Environment = environment;

                        if (options.Environment == Microsoft365Environment.Custom)
                        {
                            options.AzureADLoginAuthority = this.options.AzureADLoginAuthority;
                        }
                    }
                }
            }

            Type providerType = ResolveAuthenticationProviderType(options);

            // Use the configuration to create a new instance of IAuthenticationProvider
            var provider = serviceProvider.GetService(providerType) as OAuthAuthenticationProvider;

            // Initialize the Authentication Provider instance accordingly to the current configuration
            if (provider != null)
            {
                // Configure the provider configuration name
                provider.ConfigurationName = name;

                // and initialize all the other configuration options
                provider.Init(options);
            }

            return provider;
        }

        /// <summary>
        /// Resolves the type of the Authentication Provider to create
        /// </summary>
        /// <param name="option">The configuration options for the target Authentication Provider</param>
        /// <returns>The type of the target Authentication Provider</returns>
        private static Type ResolveAuthenticationProviderType(PnPCoreAuthenticationCredentialConfigurationOptions option)
        {
            // Determine the Authentication Provider type to create
            Type providerType = null;

            if (option.X509Certificate != null)
            {
                providerType = typeof(X509CertificateAuthenticationProvider);
            }
            else if (option.CredentialManager != null)
            {
                providerType = typeof(CredentialManagerAuthenticationProvider);
            }
            else if (option.OnBehalfOf != null)
            {
                providerType = typeof(OnBehalfOfAuthenticationProvider);
            }
            else if (option.UsernamePassword != null)
            {
                providerType = typeof(UsernamePasswordAuthenticationProvider);
            }
            else if (option.DeviceCode != null)
            {
                providerType = typeof(DeviceCodeAuthenticationProvider);
            }
            else
            {
                providerType = typeof(InteractiveAuthenticationProvider);
            }

            return providerType;
        }

/*
        /// <summary>
        /// Initializes the configuration options for the target Authentication Provider
        /// </summary>
        /// <param name="provider">The target Authentication Provider</param>
        /// <param name="option">The configuration options for the target Authentication Provider</param>
        private static void InitProviderOptions(IAuthenticationProvider provider, PnPCoreAuthenticationCredentialConfigurationOptions option)
        {
            switch (provider)
            {
                case X509CertificateAuthenticationProvider x509Certificate:
                    x509Certificate.ClientId = option.ClientId;
                    x509Certificate.TenantId = option.TenantId;
                    x509Certificate.Certificate = X509CertificateUtility.LoadCertificate(
                        option.X509Certificate.StoreName,
                        option.X509Certificate.StoreLocation,
                        option.X509Certificate.Thumbprint);
                    break;
                case ExternalAuthenticationProvider aspNetCore:
                    aspNetCore.ClientId = option.ClientId;
                    aspNetCore.TenantId = option.TenantId;
                    break;
                case CredentialManagerAuthenticationProvider credentialManager:
                    credentialManager.ClientId = option.ClientId;
                    credentialManager.TenantId = option.TenantId;
                    credentialManager.CredentialManagerName = option.CredentialManager.CredentialManagerName;
                    break;
                case OnBehalfOfAuthenticationProvider onBehalfOf:
                    onBehalfOf.ClientId = option.ClientId;
                    onBehalfOf.TenantId = option.TenantId;
                    onBehalfOf.ClientSecret = option.OnBehalfOf.ClientSecret.ToSecureString();
                    break;
                case UsernamePasswordAuthenticationProvider usernamePassword:
                    usernamePassword.ClientId = option.ClientId;
                    usernamePassword.TenantId = option.TenantId;
                    usernamePassword.Username = option.UsernamePassword.Username;
                    usernamePassword.Password = option.UsernamePassword.Password.ToSecureString();
                    break;
                case InteractiveAuthenticationProvider interactive:
                    interactive.ClientId = option.ClientId;
                    interactive.TenantId = option.TenantId;
                    interactive.RedirectUri = option.Interactive.RedirectUri;
                    break;
                case DeviceCodeAuthenticationProvider deviceCode:
                    deviceCode.ClientId = option.ClientId;
                    deviceCode.TenantId = option.TenantId;
                    deviceCode.RedirectUri = option.Interactive.RedirectUri;
                    break;
            }
        }
*/
    }
}
