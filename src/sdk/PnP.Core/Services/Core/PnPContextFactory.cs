using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Reflection;
#endif
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create PnPContext object instances
    /// </summary>
    public class PnPContextFactory : IPnPContextFactory
    {
        private static readonly Expression<Func<IWeb, object>>[] defaultWebProps = { w => w.Id, w => w.Url,
                                                                                     w => w.RegionalSettings.QueryProperties(r => r.TimeZone, r => r.All) };
        private static readonly Expression<Func<ISite, object>>[] defaultSiteProps = { s => s.Id, s => s.GroupId };

#if NET5_0_OR_GREATER
        private static readonly HttpClient httpClient = new HttpClient();
#endif
        private bool telemetryInitialized;

        /// <summary>
        /// Default constructor for <see cref="PnPContextFactory"/>
        /// </summary>
        /// <param name="logger">Connected logger</param>
        /// <param name="sharePointRestClient">SharePoint REST http client to use</param>
        /// <param name="microsoftGraphClient">Microsoft Graph http client to use</param>
        /// <param name="contextOptions"><see cref="PnPContextFactory"/> options</param>
        /// <param name="globalOptions">Global options to use</param>
        /// <param name="eventHub">EventHub instance</param>
        public PnPContextFactory(
            ILogger<PnPContext> logger,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            IOptions<PnPContextFactoryOptions> contextOptions,
            IOptions<PnPGlobalSettingsOptions> globalOptions,
            EventHub eventHub)
        {
            // Store logger and options locally
            Log = logger;
            SharePointRestClient = sharePointRestClient;
            MicrosoftGraphClient = microsoftGraphClient;
            ContextOptions = contextOptions?.Value;
            GlobalOptions = globalOptions?.Value;
            EventHub = eventHub;

            ConnectTelemetry();
        }

        /// <summary>
        /// Gets the EventHub instance, can be used to tap into events like request retry (due to throttling)
        /// </summary>
        public EventHub EventHub { get; }

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
        /// Connected Telemetry client
        /// </summary>
        internal TelemetryManager TelemetryManager { get; set; }

        /// <summary>
        /// Options used to configure this <see cref="PnPContext"/>
        /// </summary>
        protected PnPContextFactoryOptions ContextOptions { get; private set; }

        /// <summary>
        /// Options used to configure this <see cref="PnPContext"/>
        /// </summary>
        protected PnPGlobalSettingsOptions GlobalOptions { get; private set; }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the authentication provider</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, PnPContextOptions options = null)
        {
            return CreateAsync(name, initializeAuthenticationProvider, default, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the authentication provider</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(name, initializeAuthenticationProvider, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name, PnPContextOptions options = null)
        {
            return CreateAsync(name, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(name, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name, PnPContextOptions options = null)
        {
            return await CreateAsync(name, null, default, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return await CreateAsync(name, null, cancellationToken, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the authentication provider</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, PnPContextOptions options = null)
        { 
            return await CreateAsync(name, initializeAuthenticationProvider, default, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the authentication provider</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            // Process the Authentication Provider initialization code, if any
            initializeAuthenticationProvider?.Invoke(configuration.AuthenticationProvider);

            return await CreateAsync(configuration.SiteUrl, configuration.AuthenticationProvider, cancellationToken, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, PnPContextOptions options = null)
        {
            return CreateAsync(url, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(url, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, PnPContextOptions options = null)
        {
            // Use the default settings to create a new instance of PnPContext
            return await CreateAsync(url, ContextOptions.DefaultAuthenticationProvider, default, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            // Use the default settings to create a new instance of PnPContext
            return await CreateAsync(url, ContextOptions.DefaultAuthenticationProvider, cancellationToken, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            return CreateAsync(url, authenticationProvider, default, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(url, authenticationProvider, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            return await CreateAsync(url, authenticationProvider, default, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider), PnPCoreResources.Exception_CreatePnPContext_AuthenticationProvider);
            }

            // Use the provided settings to create a new instance of PnPContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                Uri = url,
                CancellationToken = cancellationToken,
            };

            // Configure telemetry, if enabled
            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, options).ConfigureAwait(false);

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            return context;
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, authenticationProvider, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, authenticationProvider, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        { 
            return await CreateAsync(groupId, authenticationProvider, default, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and Authentication Provider instance
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException(PnPCoreResources.Exception_CreatePnPContext_GroupId, nameof(groupId));
            }

            if (authenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(authenticationProvider), PnPCoreResources.Exception_CreatePnPContext_AuthenticationProvider);
            }

            // Use the provided settings to create a new instance of PnPContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                CancellationToken = cancellationToken,
            };

            // Configure telemetry, if enabled
            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Load the group to find out what the SharePoint site url is
            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, options).ConfigureAwait(false);

            return context;
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Guid groupId, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, cancellationToken, options).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId, PnPContextOptions options = null)
        {
            return await CreateAsync(groupId, ContextOptions.DefaultAuthenticationProvider, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided group and using the default Authentication Provider
        /// </summary>
        /// <param name="groupId">The id of an Microsoft 365 group</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation</param>
        /// <param name="options">Options used to configure the created context</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Guid groupId, CancellationToken cancellationToken, PnPContextOptions options = null)
        {
            return await CreateAsync(groupId, ContextOptions.DefaultAuthenticationProvider, cancellationToken, options).ConfigureAwait(false);
        }

        /// <summary>
        /// When using REST batch requests the URL needs to be correctly cased, so we're loading the web url while doing an interactive request.
        /// Also loading the default needed properties to save additional loads for missing key properties
        /// </summary>
        /// <param name="context">PnPContext being initialized</param>
        /// <param name="options">Options for the initialization of this context</param>
        /// <returns></returns>
        internal static async Task InitializeContextAsync(PnPContext context, PnPContextOptions options)
        {
            // Set environment if not yet set
            if (!context.Environment.HasValue)
            {
                context.Environment = CloudManager.GetEnvironmentFromUri(context.Uri);
                // Ensure the Microsoft Graph URL is set depending on the used cloud environment
                context.GraphClient.UpdateBaseAddress(CloudManager.GetMicrosoftGraphAuthority(context.Environment.Value));
            }

            // Store the provided options, needed for context cloning
            context.LocalContextOptions = options;

            // Add context properties (if any)
            SetPnPContextProperties(context, options);

            // IMPORTANT: if you change this logic by adding more initialization data you also need
            //            to update the CopyContextInitialization method!

            // Combine the default properties to load with optional additional properties
            var (siteProps, webProps) = GetDefaultPropertiesToLoad(options);

            if (options != null && options.SiteUriCasingIsCorrect)
            {
                var initializationBatch = context.NewBatch();
                await context.Web.LoadBatchAsync(initializationBatch, webProps.ToArray()).ConfigureAwait(false);
                await context.Site.LoadBatchAsync(initializationBatch, siteProps.ToArray()).ConfigureAwait(false);
                await context.ExecuteAsync(initializationBatch).ConfigureAwait(false);
            }
            else
            {
                // IMPORTANT: this first call is an interactive call by design as that allows us set the 
                //            web URL using the correct casing. Correct casing is required in REST batching

                // Use the query client to build the correct initialization query for the given Web properties 
                BaseDataModel<IWeb> concreteEntity = EntityManager.GetEntityConcreteInstance(typeof(IWeb), context.Web, context) as BaseDataModel<IWeb>;
                var entityInfo = EntityManager.GetClassInfo(concreteEntity.GetType(), concreteEntity, null, webProps.ToArray());
                var apiCallRequest = await QueryClient.BuildGetAPICallAsync(concreteEntity, entityInfo, new ApiCall($"_api/Web", ApiType.SPORest), true).ConfigureAwait(false);

                // Load required web properties
                var api = new ApiCall(apiCallRequest.ApiCall.Request, ApiType.SPORest)
                {
                    Interactive = true
                };
                await (context.Web as Web).RequestAsync(api, HttpMethod.Get, "Get").ConfigureAwait(false);

                // Replace the context URI with the value using the correct casing
                context.Uri = context.Web.Url;

                // Request the site properties
                await context.Site.LoadAsync(siteProps.ToArray()).ConfigureAwait(false);
            }

            // Ensure the Graph ID is set once and only once
            if (context.Web is IMetadataExtensible me)
            {
                if (!me.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                {
                    me.Metadata.Add(PnPConstants.MetaDataGraphId, $"{context.Uri.DnsSafeHost},{context.Site.Id},{context.Web.Id}");
                }
            }

            // If the GroupId is available ensure it's also correctly set on the Group metadata so that calls via that
            // model can work
            if (context.Site.IsPropertyAvailable(p => p.GroupId) && context.Site.GroupId != Guid.Empty)
            {
                if (context.Group is IMetadataExtensible groupMetaData)
                {
                    if (!groupMetaData.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        groupMetaData.Metadata.Add(PnPConstants.MetaDataGraphId, context.Site.GroupId.ToString());
                    }
                }
            }

        }

        internal static async Task CopyContextInitializationAsync(PnPContext source, PnPContext target)
        {
            if (source.Web.IsPropertyAvailable(p => p.Id) &&
                source.Web.IsPropertyAvailable(p => p.Url) &&
                source.Web.IsPropertyAvailable(p => p.RegionalSettings) &&
                source.Site.IsPropertyAvailable(p => p.Id) &&
                source.Site.IsPropertyAvailable(p => p.GroupId))
            {
                target.Web.SetSystemProperty(p => p.Id, source.Web.Id);
                target.Web.SetSystemProperty(p => p.Url, source.Web.Url);
                target.Web.Requested = true;
                (target.Web as Web).Metadata = new Dictionary<string, string>((source.Web as Web).Metadata);

                // Copy regional settings, important to trigger the creation of the regional settings model from the target web model
                var regionalSettings = target.Web.RegionalSettings;
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.AM)) { regionalSettings.SetSystemProperty(p => p.AM, source.Web.RegionalSettings.AM); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.CollationLCID)) { regionalSettings.SetSystemProperty(p => p.CollationLCID, source.Web.RegionalSettings.CollationLCID); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.DateFormat)) { regionalSettings.SetSystemProperty(p => p.DateFormat, source.Web.RegionalSettings.DateFormat); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.DateSeparator)) { regionalSettings.SetSystemProperty(p => p.DateSeparator, source.Web.RegionalSettings.DateSeparator); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.DecimalSeparator)) { regionalSettings.SetSystemProperty(p => p.DecimalSeparator, source.Web.RegionalSettings.DecimalSeparator); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.DigitGrouping)) { regionalSettings.SetSystemProperty(p => p.DigitGrouping, source.Web.RegionalSettings.DigitGrouping); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.FirstDayOfWeek)) { regionalSettings.SetSystemProperty(p => p.FirstDayOfWeek, source.Web.RegionalSettings.FirstDayOfWeek); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.IsEastAsia)) { regionalSettings.SetSystemProperty(p => p.IsEastAsia, source.Web.RegionalSettings.IsEastAsia); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.IsRightToLeft)) { regionalSettings.SetSystemProperty(p => p.IsRightToLeft, source.Web.RegionalSettings.IsRightToLeft); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.IsUIRightToLeft)) { regionalSettings.SetSystemProperty(p => p.IsUIRightToLeft, source.Web.RegionalSettings.IsUIRightToLeft); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.ListSeparator)) { regionalSettings.SetSystemProperty(p => p.ListSeparator, source.Web.RegionalSettings.ListSeparator); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.LocaleId)) { regionalSettings.SetSystemProperty(p => p.LocaleId, source.Web.RegionalSettings.LocaleId); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.NegativeSign)) { regionalSettings.SetSystemProperty(p => p.NegativeSign, source.Web.RegionalSettings.NegativeSign); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.NegNumberMode)) { regionalSettings.SetSystemProperty(p => p.NegNumberMode, source.Web.RegionalSettings.NegNumberMode); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.PM)) { regionalSettings.SetSystemProperty(p => p.PM, source.Web.RegionalSettings.PM); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.PositiveSign)) { regionalSettings.SetSystemProperty(p => p.PositiveSign, source.Web.RegionalSettings.PositiveSign); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.ShowWeeks)) { regionalSettings.SetSystemProperty(p => p.ShowWeeks, source.Web.RegionalSettings.ShowWeeks); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.ThousandSeparator)) { regionalSettings.SetSystemProperty(p => p.ThousandSeparator, source.Web.RegionalSettings.ThousandSeparator); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.Time24)) { regionalSettings.SetSystemProperty(p => p.Time24, source.Web.RegionalSettings.Time24); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeMarkerPosition)) { regionalSettings.SetSystemProperty(p => p.TimeMarkerPosition, source.Web.RegionalSettings.TimeMarkerPosition); };
                if (source.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeSeparator)) { regionalSettings.SetSystemProperty(p => p.TimeSeparator, source.Web.RegionalSettings.TimeSeparator); };
                regionalSettings.Requested = true;
                (regionalSettings as RegionalSettings).Metadata = new Dictionary<string, string>((source.Web.RegionalSettings as RegionalSettings).Metadata);

                // Copy timezone settings
                var timeZone = regionalSettings.TimeZone;
                if (source.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.Description)) { timeZone.SetSystemProperty(p => p.Description, source.Web.RegionalSettings.TimeZone.Description); };
                if (source.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.Id)) { timeZone.SetSystemProperty(p => p.Id, source.Web.RegionalSettings.TimeZone.Id); };
                if (source.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.Bias)) { timeZone.SetSystemProperty(p => p.Bias, source.Web.RegionalSettings.TimeZone.Bias); };
                if (source.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.DaylightBias)) { timeZone.SetSystemProperty(p => p.DaylightBias, source.Web.RegionalSettings.TimeZone.DaylightBias); };
                if (source.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.StandardBias)) { timeZone.SetSystemProperty(p => p.StandardBias, source.Web.RegionalSettings.TimeZone.StandardBias); };
                timeZone.Requested = true;
                (timeZone as Model.SharePoint.TimeZone).Metadata = new Dictionary<string, string>((source.Web.RegionalSettings.TimeZone as Model.SharePoint.TimeZone).Metadata);

                // Copy site settings                
                target.Site.SetSystemProperty(p => p.Id, source.Site.Id);
                target.Site.SetSystemProperty(p => p.GroupId, source.Site.GroupId);
                target.Site.Requested = true;
                (target.Site as Site).Metadata = new Dictionary<string, string>((source.Site as Site).Metadata);

                // Copy the additional initialization properties (if any)
                // Only "basic" properties are cloned, complex properties (e.g. Web.Lists) are not cloned as 
                // a common use case of using a cloned context is to start "clean", basic web/site properties do not change 
                // when cloning for the same site url, but one might want to load collections (= complex property)
                // using a different filter
                if (source.LocalContextOptions != null)
                {
                    List<Expression<Func<ISite, object>>> clonedSiteProperties = new List<Expression<Func<ISite, object>>>();
                    List<Expression<Func<IWeb, object>>> clonedWebProperties = new List<Expression<Func<IWeb, object>>>();

                    if (source.LocalContextOptions.AdditionalSitePropertiesOnCreate != null)
                    {

                        foreach (var prop in source.LocalContextOptions.AdditionalSitePropertiesOnCreate)
                        {
                            if (!prop.Body.Type.ImplementsInterface(typeof(IDataModelParent)) && !prop.Body.Type.ImplementsInterface(typeof(IQueryable)))
                            {
                                clonedSiteProperties.Add(prop);
                                target.Site.SetSystemProperty(prop, GetPropertyValue(source.Site, prop));
                            }
                        }
                    }

                    if (source.LocalContextOptions.AdditionalWebPropertiesOnCreate != null)
                    {
                        foreach (var prop in source.LocalContextOptions.AdditionalWebPropertiesOnCreate)
                        {
                            if (!prop.Body.Type.ImplementsInterface(typeof(IDataModelParent)) && !prop.Body.Type.ImplementsInterface(typeof(IQueryable)))
                            {
                                clonedWebProperties.Add(prop);
                                target.Web.SetSystemProperty(prop, GetPropertyValue(source.Web, prop));
                            }
                        }
                    }

                    if (clonedSiteProperties.Any() || clonedWebProperties.Any())
                    {
                        target.LocalContextOptions = new PnPContextOptions()
                        {
                            AdditionalSitePropertiesOnCreate = clonedSiteProperties.Any() ? clonedSiteProperties : null,
                            AdditionalWebPropertiesOnCreate = clonedWebProperties.Any() ? clonedWebProperties : null
                        };
                    }
                }
            }
            else
            {
                // seems the data in the source context was not what we expected, run the usual initialization to reset it, reuse the source context options 
                // if specified
                await InitializeContextAsync(target, source.LocalContextOptions).ConfigureAwait(false);
            }
        }

        private static TResult GetPropertyValue<TSource, TResult>(TSource source, Expression<Func<TSource, TResult>> expression)
        {
            var propertyName = expression.Body switch
            {
                MemberExpression m => m.Member.Name,
                UnaryExpression u when u.Operand is MemberExpression m => m.Member.Name,
                _ => throw new NotImplementedException(expression.GetType().ToString())
            };

            return (TResult)source.GetPublicInstancePropertyValue(propertyName);
        }

        private static (IEnumerable<Expression<Func<ISite, object>>>, IEnumerable<Expression<Func<IWeb, object>>>) GetDefaultPropertiesToLoad(PnPContextOptions options)
        {
            IEnumerable<Expression<Func<ISite, object>>> siteProps = defaultSiteProps;
            IEnumerable<Expression<Func<IWeb, object>>> webProps = defaultWebProps;

            if (options != null)
            {
                if (options.AdditionalSitePropertiesOnCreate != null)
                {
                    siteProps = siteProps.Union(options.AdditionalSitePropertiesOnCreate);
                }

                if (options.AdditionalWebPropertiesOnCreate != null)
                {
                    webProps = webProps.Union(options.AdditionalWebPropertiesOnCreate);
                }
            }

            return (siteProps, webProps);
        }

        private static void SetPnPContextProperties(PnPContext context, PnPContextOptions options)
        {
            if (options != null && options.Properties != null && options.Properties.Count > 0)
            {
                foreach (var property in options.Properties)
                {
                    context.Properties[property.Key] = property.Value;
                }
            }
        }

        internal static async Task ConfigureForGroup(PnPContext context, Guid groupId)
        {
            // Ensure the group is loaded, given we've received the group id we can populate the metadata of the group model upfront before loading it
            (context.Group as GraphGroup).Metadata.Add(PnPConstants.MetaDataGraphId, groupId.ToString());
            // Do the default group load, should load all properties
            await context.Group.LoadAsync().ConfigureAwait(false);
            // If the group has a linked SharePoint site then WebUrl is populated
            context.Uri = context.Group.WebUrl;
        }

        internal void ConnectTelemetry()
        {
            bool connectTelemetry = true;

            if (GlobalOptions != null && GlobalOptions.DisableTelemetry)
            {
                connectTelemetry = false;
            }

            // Second option to turn of telemetry is the PnP wide PNP_DISABLETELEMETRY environment variable
            if (connectTelemetry && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PNP_DISABLETELEMETRY")))
            {
                connectTelemetry = false;
            }

#if NET5_0_OR_GREATER
            if (RuntimeInformation.RuntimeIdentifier == "browser-wasm")
            {
                connectTelemetry = false;
            }
#endif

            if (connectTelemetry)
            {
                TelemetryManager = new TelemetryManager(GlobalOptions);
            }
        }

        internal async Task ConfigureTelemetry(PnPContext context)
        {
            // We only configure telemetry and send the init event once per created PnPContextFactory
            if (telemetryInitialized)
            {
                return;
            }

#if NET5_0_OR_GREATER
            if (RuntimeInformation.RuntimeIdentifier == "browser-wasm")
            {
                // Blazor WASM cannot handle the AppInsights NuGet package way of working,
                // hence we're sending just one event to track WASM usage via a manual post
                await SendBlazorInitEventAsync(context).ConfigureAwait(false);
                telemetryInitialized = true;
                return;
            }
#endif

            // Populate the Azure AD tenant id
            if (TelemetryManager != null && GlobalOptions != null && !GlobalOptions.DisableTelemetry)
            {
                await context.SetAADTenantId().ConfigureAwait(false);

                // Send a one time "Init" event
                TelemetryManager.LogInitRequest();
            }

            telemetryInitialized = true;
        }

#if NET5_0_OR_GREATER
        private async Task SendBlazorInitEventAsync(PnPContext context)
        {
            try
            {
                // Ensure the tenant id was fetched
                await context.SetAADTenantId(true).ConfigureAwait(false);

                using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://dc.services.visualstudio.com/v2/track"))
                {
                    Assembly coreAssembly = Assembly.GetExecutingAssembly();
                    // Build payload
                    var body = new
                    {
                        name = "AppEvents",
                        time = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffZ"),
                        iKey = TelemetryManager.InstrumentationKey,
                        data = new
                        {
                            baseType = "EventData",
                            baseData = new
                            {
                                ver = 2,
                                name = "PnPCoreInit",
                                properties = new
                                {
                                    PnPCoreSDKVersion = ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version,
                                    AADTenantId = GlobalOptions.AADTenantId.ToString(),
                                    OS = "WASM"
                                }
                            }
                        }
                    };

                    var jsonBody = JsonSerializer.Serialize(body, PnPConstants.JsonSerializer_IgnoreNullValues);

                    using (StringContent content = new StringContent(jsonBody))
                    {
                        // Remove the default Content-Type content header
                        if (content.Headers.Contains("Content-Type"))
                        {
                            content.Headers.Remove("Content-Type");
                        }
                        // Add the batch Content-Type header
                        content.Headers.Add($"Content-Type", "application/x-json-stream");

                        // Connect content to request
                        request.Content = content;

                        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            Log.LogDebug($"Blazor telemetry failure: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                        }
                    }
                }
            }
            // Never fail if telemetry is failing
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.LogDebug($"Blazor telemetry failure: {ex}");
            }
        }
#endif

    }
}
