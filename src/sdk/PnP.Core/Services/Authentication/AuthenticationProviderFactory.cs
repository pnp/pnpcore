using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Configuration;
using System.Linq;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create IAuthenticationProvider object instances
    /// </summary>

    public class AuthenticationProviderFactory : IAuthenticationProviderFactory
    {
        private readonly OAuthAuthenticationProviderOptions options;
        private readonly ILogger log;
        private readonly IServiceProvider serviceProvider;

        public AuthenticationProviderFactory(
            IOptionsMonitor<OAuthAuthenticationProviderOptions> options,
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
            this.log = logger;
            this.serviceProvider = serviceProvider;
            this.options = options.CurrentValue;
        }

        /// <summary>
        /// Creates the default instance of IAuthenticationProvider based on the configuration
        /// </summary>
        /// <returns>An object that implements IAuthenticationProvider based on the configuration</returns>
        public IAuthenticationProvider CreateDefault()
        {
            if (string.IsNullOrEmpty(this.options.DefaultConfiguration))
            {
                throw new ConfigurationException("Missing default configuration for Authentication Provider Factory");
            }

            // Return the Authentication Provider based on the default configuration
            return Create(this.options.DefaultConfiguration);
        }

        /// <summary>
        /// Creates a new instance of IAuthenticationProvider based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the IAuthenticationProvider configuration to use</param>
        /// <returns>An object that implements IAuthenticationProvider based on the provided configuration name</returns>
        public IAuthenticationProvider Create(string name)
        {
            // Search for the provided configuration
            var configuration = this.options.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new Exception($"Invalid configuration name '{name}' for IAuthenticationProvider creation!");
            }

            // Use the configuration to create a new instance of IAuthenticationProvider
            var provider = this.serviceProvider.GetService(configuration.AuthenticationProviderType) as IAuthenticationProvider;

            // Configure the Authentication Provider instance accordingly to the current configuration
            if (provider != null)
            {
                provider.Configure(configuration);
            }
            return provider;
        }
    }
}
