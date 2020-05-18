using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using System;
using System.Collections.Generic;

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
            IAuthenticationProviderFactory authenticationProviderFactory,
            SharePointRestClient sharePointRestClient,
            MicrosoftGraphClient microsoftGraphClient,
            ISettings settingsClient,
            TelemetryClient telemetryClient) : base(options, logger, authenticationProviderFactory, sharePointRestClient, microsoftGraphClient, settingsClient, telemetryClient)
        {
        }

        public override PnPContext Create(string name)
        {
            var context = base.Create(name);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Uri url, IAuthenticationProvider authenticationProvider)
        {
            var context = base.Create(url, authenticationProvider);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Uri url)
        {
            var context = base.Create(url);
            ConfigurePnPContextForTesting(ref context);
            return context;
        }

        public override PnPContext Create(Uri url, string authenticationProviderName)
        {
            var context = base.Create(url, authenticationProviderName);
            ConfigurePnPContextForTesting(ref context);
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
