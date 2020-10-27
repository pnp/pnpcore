using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create PnPContext object instances
    /// </summary>
    public class PnPContextFactory : IPnPContextFactory
    {
        /// <summary>
        /// Default constructor for <see cref="PnPContextFactory"/>
        /// </summary>
        /// <param name="logger">Connected logger</param>
        /// <param name="sharePointRestClient">SharePoint REST http client to use</param>
        /// <param name="microsoftGraphClient">Microsoft Graph http client to use</param>
        /// <param name="contextOptions"><see cref="PnPContextFactory"/> options</param>
        /// <param name="globalOptions">Global options to use</param>
        /// <param name="telemetryClient">Connected Azure AppInsights telemetry client</param>
        public PnPContextFactory(
            ILogger<PnPContext> logger,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            IOptions<PnPContextFactoryOptions> contextOptions,
            IOptions<PnPGlobalSettingsOptions> globalOptions
#if !BLAZOR
            , TelemetryClient telemetryClient
#endif
            )
        {
            // Store logger and options locally
            Log = logger;
            SharePointRestClient = sharePointRestClient;
            MicrosoftGraphClient = microsoftGraphClient;
            ContextOptions = contextOptions?.Value;
            GlobalOptions = globalOptions?.Value;
#if !BLAZOR
            TelemetryClient = telemetryClient;
#else
            TelemetryClient = null;
#endif
        }

        /// <summary>
        /// Connected logger
        /// </summary>
        protected ILogger Log { get; private set; }

        /// <summary>
        /// Connected SharePoint REST http client
        /// </summary>
        protected SharePointRestClient SharePointRestClient { get; private set; }

        /// <summary>
        /// Connected Microsoft Graph http client
        /// </summary>
        protected MicrosoftGraphClient MicrosoftGraphClient { get; private set; }

        /// <summary>
        /// Connected Azure AppInsights telemetry client
        /// </summary>
        protected TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        /// Options used to configure this <see cref="PnPContext"/>
        /// </summary>
        protected PnPContextFactoryOptions ContextOptions { get; private set; }

        /// <summary>
        /// Options used to configure this <see cref="PnPContext"/>
        /// </summary>
        protected PnPGlobalSettingsOptions GlobalOptions { get; private set; }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name)
        {
            return CreateAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the SPOContext configuration to use</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name)
        {
            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            return await CreateAsync(configuration.SiteUrl, configuration.AuthenticationProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url)
        {
            return CreateAsync(url).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url)
        {
            // Use the default settings to create a new instance of SPOContext
            return await CreateAsync(url, ContextOptions.DefaultAuthenticationProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the SPOContext</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider)
        {
            return CreateAsync(url, authenticationProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of SPOContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the SPOContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the SPOContext</param>
        /// <returns>A SPOContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryClient)
            {
                Uri = url
            };

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            await ConfigureTelemetry(context).ConfigureAwait(false);

            return context;
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider)
        {
            return CreateAsync(groupId, authenticationProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryClient);

            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            await ConfigureTelemetry(context).ConfigureAwait(false);

            return context;
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId)
        {
            return CreateAsync(groupId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId)
        {
            return await CreateAsync(groupId, ContextOptions.DefaultAuthenticationProvider).ConfigureAwait(false);
        }

        internal static async Task ConfigureForGroup(PnPContext context, Guid groupId)
        {
            // Ensure the group is loaded, given we've received the group id we can populate the metadata of the group model upfront before loading it
            (context.Group as GraphGroup).Metadata.Add(PnPConstants.MetaDataGraphId, groupId.ToString());
            // Do the default group load, should load all properties
            await context.Group.GetAsync().ConfigureAwait(false);
            // If the group has a linked SharePoint site then WebUrl is populated
            context.Uri = context.Group.WebUrl;
        }

        internal async Task ConfigureTelemetry(PnPContext context)
        {
            // Populate the Azure AD tenant id
            if (TelemetryClient != null && GlobalOptions != null && !GlobalOptions.DisableTelemetry)
            {
                await context.SetAADTenantId().ConfigureAwait(false);
            }
        }
    }
}
