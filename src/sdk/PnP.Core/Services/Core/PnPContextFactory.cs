using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.AzureActiveDirectory;
using System;
using System.Linq;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create PnPContext object instances
    /// </summary>
    public class PnPContextFactory : IPnPContextFactory
    {
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
            Log = logger;
            Options = options.CurrentValue;
            AuthenticationProviderFactory = authenticationProviderFactory;
            SharePointRestClient = sharePointRestClient;
            MicrosoftGraphClient = microsoftGraphClient;
            SettingsClient = settingsClient;
            TelemetryClient = telemetryClient;
        }

        internal PnPContextFactoryOptions Options { get; private set; }
        internal IAuthenticationProviderFactory AuthenticationProviderFactory { get; private set; }
        internal ILogger Log { get; private set; }
        internal SharePointRestClient SharePointRestClient { get; private set; }
        internal MicrosoftGraphClient MicrosoftGraphClient { get; private set; }
        internal TelemetryClient TelemetryClient { get; private set; }
        internal ISettings SettingsClient { get; private set; }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name)
        {
            // Search for the provided configuration
            var configuration = Options.Configurations.FirstOrDefault(c => c.Name == name);
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
            return Create(url, AuthenticationProviderFactory.CreateDefault());
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
            var authProvider = AuthenticationProviderFactory.Create(authenticationProviderName);
            if (authProvider == null)
            {
                throw new ClientException(ErrorType.ConfigurationError, $"Invalid Authentication Provider name '{authenticationProviderName}' for site '{url.AbsoluteUri}' during PnPContext creation!");
            }

            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, authProvider, SharePointRestClient, MicrosoftGraphClient, SettingsClient, TelemetryClient)
            {
                Uri = url
            };

            ConfigureTelemetry(context);

            return context;
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
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, SettingsClient, TelemetryClient)
            {
                Uri = url
            };

            ConfigureTelemetry(context);

            return context;
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
            var authProvider = AuthenticationProviderFactory.Create(authenticationProviderName);
            if (authProvider == null)
            {
                throw new ClientException(ErrorType.ConfigurationError, $"Invalid Authentication Provider name '{authenticationProviderName}' for group '{groupId}' during PnPContext creation!");
            }

            // Use the provided settings to create a new instance of SPOContext
            var context =  new PnPContext(Log, authProvider, SharePointRestClient, MicrosoftGraphClient, SettingsClient, TelemetryClient);

            ConfigureForGroup(context, groupId);

            ConfigureTelemetry(context);

            return context;
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
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, SettingsClient, TelemetryClient);

            ConfigureForGroup(context, groupId);

            ConfigureTelemetry(context);

            return context;
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Office 365 group</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId)
        {
            return Create(groupId, AuthenticationProviderFactory.CreateDefault());
        }

        internal static void ConfigureForGroup(PnPContext context, Guid groupId)
        {
            // Ensure the group is loaded, given we've received the group id we can populate the metadata of the group model upfront before loading it
            (context.Group as Group).Metadata.Add(PnPConstants.MetaDataGraphId, groupId.ToString());
            // Do the default group load, should load all properties
            context.Group.GetAsync().GetAwaiter().GetResult();
            // If the group has a linked SharePoint site then WebUrl is populated
            context.Uri = context.Group.WebUrl;
        }

        internal void ConfigureTelemetry(PnPContext context)
        {
            // Populate the Azure AD tenant id
            if (SettingsClient != null && !SettingsClient.DisableTelemetry)
            {
                context.SetAADTenantId();
            }
        }
    }
}
