using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Model;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using System;
using System.Linq;
using System.Net.Http;
#if NET5_0
using System.Runtime.InteropServices;
using System.Text.Json;
#endif
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Public factory service to create PnPContext object instances
    /// </summary>
    public class PnPContextFactory : IPnPContextFactory
    {
#if NET5_0
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
        public PnPContextFactory(
            ILogger<PnPContext> logger,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            IOptions<PnPContextFactoryOptions> contextOptions,
            IOptions<PnPGlobalSettingsOptions> globalOptions)
        {
            // Store logger and options locally
            Log = logger;
            SharePointRestClient = sharePointRestClient;
            MicrosoftGraphClient = microsoftGraphClient;
            ContextOptions = contextOptions?.Value;
            GlobalOptions = globalOptions?.Value;

            ConnectTelemetry();            
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
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider)
        {
            return CreateAsync(name, initializeAuthenticationProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(string name)
        {
            return CreateAsync(name).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name)
        {
            return await CreateAsync(name, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="name">The name of the PnPContext configuration to use</param>
        /// <param name="initializeAuthenticationProvider">The function to initialize the authentication provider</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(string name, Action<IAuthenticationProvider> initializeAuthenticationProvider)
        {
            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            // Process the Authentication Provider initialization code, if any
            if (initializeAuthenticationProvider != null)
            {
                initializeAuthenticationProvider(configuration.AuthenticationProvider);
            }

            return await CreateAsync(configuration.SiteUrl, configuration.AuthenticationProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url)
        {
            return CreateAsync(url).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url)
        {
            // Use the default settings to create a new instance of PnPContext
            return await CreateAsync(url, ContextOptions.DefaultAuthenticationProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public virtual PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider)
        {
            return CreateAsync(url, authenticationProvider).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a new instance of PnPContext based on a provided configuration name
        /// </summary>
        /// <param name="url">The URL of the PnPContext as a URI</param>
        /// <param name="authenticationProvider">The Authentication Provider to use to authenticate within the PnPContext</param>
        /// <returns>A PnPContext object based on the provided configuration name</returns>
        public async virtual Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of PnPContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                Uri = url
            };

            // Configure telemetry, if enabled
            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context).ConfigureAwait(false);

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
            // Use the provided settings to create a new instance of PnPContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager);

            // Configure telemetry, if enabled
            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Load the group to find out what the SharePoint site url is
            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context).ConfigureAwait(false);

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

        /// <summary>
        /// When using REST batch requests the URL needs to be correctly cased, so we're loading the web url while doing an interactive request.
        /// Also loading the default needed properties to save additional loads for missing key properties
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static async Task InitializeContextAsync(PnPContext context)
        {
            // Load required web properties
            var api = new ApiCall($"{context.Uri}/_api/Web?$select=Id,Url,RegionalSettings&$expand=RegionalSettings/TimeZone", ApiType.SPORest)
            {
                Interactive = true
            };
            await (context.Web as Web).RequestAsync(api, HttpMethod.Get, "Get").ConfigureAwait(false);
            
            // Replace the context URI with the value using the correct casing
            context.Uri = context.Web.Url;

            // Request the Site Id
            await context.Site.GetAsync(p => p.Id, p=>p.GroupId).ConfigureAwait(false);

            // Ensure the Graph ID is set
            (context.Web as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataGraphId, $"{context.Uri.DnsSafeHost},{context.Site.Id},{context.Web.Id}");
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

        internal void ConnectTelemetry()
        {
            bool connectTelemetry = true;

            if (GlobalOptions != null && GlobalOptions.DisableTelemetry)
            {
                connectTelemetry = false;
            }

#if NET5_0
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

#if NET5_0
            if (RuntimeInformation.RuntimeIdentifier == "browser-wasm")
            {
                // Blazor WASM cannot handle the AppInsights NuGet package way of working,
                // hence we're sending just one event to track WASM usage via a manual post
                await SendBlazorInitEventAsync(context).ConfigureAwait(false);                 
                telemetryInitialized = true;
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

#if NET5_0
        private async Task SendBlazorInitEventAsync(PnPContext context)
        {
            try
            {
                // Ensure the tenant id was fetched
                await context.SetAADTenantId().ConfigureAwait(false);

                using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://dc.services.visualstudio.com/v2/track"))
                {

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
                                    PnPCoreSDKVersion = "1.0.0.0",
                                    AADTenantId = GlobalOptions.AADTenantId.ToString(),
                                    OS = "WASM"
                                }
                            }
                        }
                    };

                    var jsonBody = JsonSerializer.Serialize(body, new JsonSerializerOptions { IgnoreNullValues = true });

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
