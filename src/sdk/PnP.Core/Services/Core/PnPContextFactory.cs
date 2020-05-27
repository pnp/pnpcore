using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create PnPContext object instances
    /// </summary>
    public class PnPContextFactory : IPnPContextFactory
    {
        private readonly PnPContextFactoryOptions options;
        private readonly ILogger log;

        private readonly IAuthenticationProviderFactory authProviderFactory;
        private readonly SharePointRestClient restClient;
        private readonly MicrosoftGraphClient graphClient;
        private readonly TelemetryClient telemetry;
        private readonly ISettings settings;

        public PnPContextFactory(
            IOptionsMonitor<PnPContextFactoryOptions> options,
            ILogger<PnPContext> logger,
            IAuthenticationProviderFactory authenticationProviderFactory,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            ISettings settingsClient,
            TelemetryClient telemetryClient)
        {
            // We need the options
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Store logger and options locally
            this.log = logger;
            this.options = options.CurrentValue;
            authProviderFactory = authenticationProviderFactory;
            restClient = sharePointRestClient;
            graphClient = microsoftGraphClient;
            settings = settingsClient;
            telemetry = telemetryClient;
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name)
        {
            // Search for the provided configuration
            var configuration = this.options.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError, $"Invalid configuration name '{name}' for PnPContext creation!");
            }

            return Create(configuration.SiteUrl, configuration.AuthenticationProviderName);
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <param name="authenticationProviderName">The name of the Authentication Provider to use to authenticate within the SPOContext</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url)
        {
            // Use the default settings to create a new instance of SPOContext
            return Create(url, authProviderFactory.CreateDefault());
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <param name="authenticationProviderName">The name of the Authentication Provider to use to authenticate within the SPOContext</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, string authenticationProviderName)
        {
            // Create the Authentication Provider based on the provided configuration
            var authProvider = authProviderFactory.Create(authenticationProviderName);
            if (authProvider == null)
            {
                throw new ClientException(ErrorType.ConfigurationError, $"Invalid Authentication Provider name '{authenticationProviderName}' for site '{url.AbsoluteUri}' during PnPContext creation!");
            }

            // Use the provided settings to create a new instance of SPOContext
            return new PnPContext(url, log, authProvider, restClient, graphClient, settings, telemetry);
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the SPOContext</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of SPOContext
            return new PnPContext(url, log, authenticationProvider,
                restClient, graphClient, settings, telemetry);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication configuration name
        /// </summary>
        /// <param name="groupId">The id of an Office 365 group</param>
        /// <param name="authenticationProviderName">The name of the Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, string authenticationProviderName)
        {
            // Create the Authentication Provider based on the provided configuration
            var authProvider = authProviderFactory.Create(authenticationProviderName);
            if (authProvider == null)
            {
                throw new ClientException(ErrorType.ConfigurationError, $"Invalid Authentication Provider name '{authenticationProviderName}' for group '{groupId}' during PnPContext creation!");
            }

            // Use the provided settings to create a new instance of SPOContext
            return new PnPContext(groupId, log, authProvider, restClient, graphClient, settings, telemetry);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Office 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of SPOContext
            return new PnPContext(groupId, log, authenticationProvider,
                restClient, graphClient, settings, telemetry);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Office 365 group</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId)
        {
            return Create(groupId, authProviderFactory.CreateDefault());
        }
    }
}
