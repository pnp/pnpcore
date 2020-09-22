using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Test.Services
{
    /// <summary>
    /// Test context factory, delivering PnPContext objects that can be used in testing (with Mocking/Recording enabled)
    /// </summary>
    public class TestPnPContextFactory : PnPContextFactory
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

        public TestPnPContextFactory(
            IOptionsMonitor<PnPContextFactoryOptions> options,
            ILogger<PnPContext> logger,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            IOptions<PnPContextFactoryOptions> contextOptions,
            IOptions<PnPGlobalSettingsOptions> globalOptions,
            TelemetryClient telemetryClient) : base(options, logger, sharePointRestClient, microsoftGraphClient, contextOptions, globalOptions, telemetryClient)
        {
        }

        public override PnPContext Create(string name)
        {
            return CreateAsync(name).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(string name)
        {
            var context = await base.CreateAsync(name).ConfigureAwait(false);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider)
        {
            return CreateAsync(url, authenticationProvider).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Uri url, IAuthenticationProvider authenticationProvider)
        {
            var context = await base.CreateAsync(url, authenticationProvider).ConfigureAwait(false);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Uri url)
        {
            return CreateAsync(url).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Uri url)
        {
            var context = await base.CreateAsync(url).ConfigureAwait(false);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Guid groupId, IAuthenticationProvider authenticationProvider)
        {
            return CreateAsync(groupId, authenticationProvider).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Guid groupId, IAuthenticationProvider authenticationProvider)
        {
            var context = new PnPContext(Log, authenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryClient);

            ConfigurePnPContextForTesting(ref context);

            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            return context;
        }

        public override PnPContext Create(Guid groupId)
        {
            return CreateAsync(groupId).GetAwaiter().GetResult();
        }

        public async override Task<PnPContext> CreateAsync(Guid groupId)
        {
            var context = new PnPContext(Log, Options.DefaultAuthenticationProvider, SharePointRestClient, MicrosoftGraphClient, ContextOptions, GlobalOptions, TelemetryClient);

            ConfigurePnPContextForTesting(ref context);

            await ConfigureForGroup(context, groupId).ConfigureAwait(false);

            return context;
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
    }
}
