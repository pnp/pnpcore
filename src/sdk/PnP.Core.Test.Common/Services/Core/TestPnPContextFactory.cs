using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using PnP.Core.Services.Core;
using PnP.Core.Test.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Common.Services
{
    /// <summary>
    /// Test context factory, delivering PnPContext objects that can be used in testing (with Mocking/Recording enabled)
    /// </summary>
    public class TestPnPContextFactory : PnPContextFactory, IPnPTestContextFactory
    {
        /// <summary>
        /// Generate a context configured for mocking mode or recording mode
        /// </summary>
        public bool Mocking { get; set; } = true;

        /// <summary>
        /// Context id, useful when a test contains multiple context creations
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the test
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Source file of the test, will be used to determine the path for storing test files
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Generate the .request and .debug files, can be handy for debugging
        /// </summary>
        public bool GenerateTestMockingDebugFiles { get; set; }

        /// <summary>
        /// Urls's used by the test cases
        /// </summary>
        public Dictionary<string, Uri> TestUris { get; set; }

        /// <summary>
        /// Run the tests using application permissions instead of delegated permissions
        /// </summary>
        public bool UseApplicationPermissions { get; set; }

        public TestPnPContextFactory(
            ILogger<PnPContext> logger,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            IOptions<PnPContextFactoryOptions> contextOptions,
            IOptions<PnPGlobalSettingsOptions> globalOptions,
            EventHub eventHub) : base(logger, sharePointRestClient, microsoftGraphClient, contextOptions, globalOptions, eventHub)
        {
            if (TelemetryManager != null && !TestCommonBase.RunningInGitHubWorkflow())
            {
                // Send telemetry to the test Azure AppInsights instance
#pragma warning disable CS0618 // Type or member is obsolete
                TelemetryManager.TelemetryClient.InstrumentationKey = "6073339d-9e70-4004-9ff7-1345316ade97";
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        public override PnPContext Create(string name, PnPContextOptions options = null)
        {
            return CreateAsync(name, options).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(string name, PnPContextOptions options = null)
        {
            name = UpdateConfigurationForApplicationPermissions(name);

            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            return await CreateAsync(configuration.SiteUrl, configuration.AuthenticationProvider, options).ConfigureAwait(false);
        }

        internal async Task<PnPContext> CreateWithTelemetryManagerAsync(string name, TelemetryManager telemetryManager)
        {
            name = UpdateConfigurationForApplicationPermissions(name);

            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, configuration.AuthenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, telemetryManager)
            {
                Uri = configuration.SiteUrl
            };

            ConfigurePnPContextForTesting(ref context);

            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, null).ConfigureAwait(false);

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            return context;

        }

        public async Task<PnPContext> CreateWithoutInitializationAsync(string name)
        {
            name = UpdateConfigurationForApplicationPermissions(name);

            // Search for the provided configuration
            var configuration = ContextOptions.Configurations.FirstOrDefault(c => c.Name == name);
            if (configuration == null)
            {
                throw new ClientException(ErrorType.ConfigurationError,
                    string.Format(PnPCoreResources.Exception_ConfigurationError_InvalidPnPContextConfigurationName, name));
            }

            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, configuration.AuthenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                Uri = configuration.SiteUrl
            };

            ConfigurePnPContextForTesting(ref context);

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            await ConfigureTelemetry(context).ConfigureAwait(false);

            return context;

        }

        public async Task<PnPContext> CreateLiveAsync(Uri url, IAuthenticationProvider authenticationProvider)
        {
            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                Uri = url
            };

            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, null).ConfigureAwait(false);

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            return context;

        }

        public override PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            return CreateAsync(url, authenticationProvider, options).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            // Use the provided settings to create a new instance of SPOContext
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager)
            {
                Uri = url
            };

            ConfigurePnPContextForTesting(ref context);

            await ConfigureTelemetry(context).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, options).ConfigureAwait(false);

            // Configure the global Microsoft Graph settings
            context.GraphFirst = ContextOptions.GraphFirst;
            context.GraphCanUseBeta = ContextOptions.GraphCanUseBeta;
            context.GraphAlwaysUseBeta = ContextOptions.GraphAlwaysUseBeta;

            return context;
        }

        public override PnPContext Create(Uri url, PnPContextOptions options = null)
        {
            return CreateAsync(url, options).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Uri url, PnPContextOptions options = null)
        {
            // Use the default settings to create a new instance of SPOContext
            return await CreateAsync(url, ContextOptions.DefaultAuthenticationProvider, options).ConfigureAwait(false);
        }

        public override PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, authenticationProvider, options).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider, PnPContextOptions options = null)
        {
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager);

            ConfigurePnPContextForTesting(ref context);

            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            return context;
        }

        public override PnPContext Create(Guid groupId, PnPContextOptions options = null)
        {
            return CreateAsync(groupId, options).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Guid groupId, PnPContextOptions options = null)
        {
            var context = new PnPContext(Log, ContextOptions.DefaultAuthenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryManager);

            ConfigurePnPContextForTesting(ref context);

            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            // Perform context initialization
            await InitializeContextAsync(context, options).ConfigureAwait(false);

            return context;
        }

        internal PnPGlobalSettingsOptions GetGlobalSettingsOptions()
        {
            return GlobalOptions;
        }

        internal void HookupTelemetryManager()
        {
            TelemetryManager = new TestTelemetryManager(GetGlobalSettingsOptions());
        }

        internal void RemoveTelemetryManager()
        {
            TelemetryManager = null;
        }

        internal TelemetryManager GetTelemetryManager()
        {
            return TelemetryManager;
        }

        private void ConfigurePnPContextForTesting(ref PnPContext context)
        {
            if (Mocking)
            {
                context.SetMockMode(Id, TestName, SourceFilePath, GenerateTestMockingDebugFiles, TestUris);
            }
            else
            {
                context.SetRecordingMode(Id, TestName, SourceFilePath, GenerateTestMockingDebugFiles, TestUris);
            }
        }

        private string UpdateConfigurationForApplicationPermissions(string configuration)
        {
            if (UseApplicationPermissions)
            {
                return $"{configuration}{Constants.ApplicationPermissionsSuffix}";
            }

            return configuration;
        }

    }
}
