using Microsoft.Extensions.Options;
using PnP.Core.Services.Builder.Configuration;
using System;

namespace PnP.Core.Auth.Services.Builder.Configuration
{
    /// <summary>
    /// Handles the configuration of PnPCoreOptions based on the AuthenticationOptions settings
    /// </summary>
    public class AuthenticationProvidersOptionsConfigurator :
        IConfigureOptions<PnPCoreOptions>
    {
        private readonly PnPCoreAuthenticationOptions authenticationOptions;
        private readonly IAuthenticationProviderFactory authenticationProviderFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="authenticationOptions"><see cref="PnPCoreAuthenticationOptions"/> authentication options</param>
        /// <param name="authenticationProviderFactory">Authentication factory</param>
        public AuthenticationProvidersOptionsConfigurator(IOptions<PnPCoreAuthenticationOptions> authenticationOptions,
            IAuthenticationProviderFactory authenticationProviderFactory)
        {
            this.authenticationOptions = authenticationOptions?.Value;
            this.authenticationProviderFactory = authenticationProviderFactory;
        }

        /// <summary>
        /// Process the <see cref="PnPCoreOptions"/> options
        /// </summary>
        /// <param name="options"></param>
        public void Configure(PnPCoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (authenticationOptions != null
                && authenticationOptions.Credentials != null
                && authenticationOptions.Credentials.Configurations.Count > 0)
            {
                // For each site configuration we create the configured Authentication Provider
                //foreach (var (siteKey, siteValue) in authenticationOptions.Sites)
                foreach (var siteOption in authenticationOptions.Sites)
                {
                    var siteKey = siteOption.Key;
                    var siteValue = siteOption.Value;

                    // We use the configuration provided by name, if any
                    if (!string.IsNullOrEmpty(siteValue.AuthenticationProviderName))
                    {
                        // and we set it into its settings
                        options.Sites[siteKey].AuthenticationProvider =
                            authenticationProviderFactory.Create(siteValue.AuthenticationProviderName);
                    }
                    // We use the default provider and configuration if a specific configuration is missing
                    else
                    {
                        options.Sites[siteKey].AuthenticationProvider =
                            authenticationProviderFactory.CreateDefault();
                    }
                }
            }

            if (authenticationOptions != null
                && authenticationOptions.Credentials != null
                && !string.IsNullOrEmpty(authenticationOptions.Credentials.DefaultConfiguration))
            {
                // Configure the default IAuthenticationProvider instance
                options.DefaultAuthenticationProvider = authenticationProviderFactory.CreateDefault();
            }
        }
    }
}
