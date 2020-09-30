using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Test.Services;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;

namespace PnP.Core.Test.Utilities
{
    public sealed class TestCommon
    {
        private const string AsyncSuffix = "_Async";
        private static readonly Lazy<TestCommon> _lazyInstance = new Lazy<TestCommon>(() => new TestCommon(), true);
        private IPnPContextFactory pnpContextFactoryCache;
        private static readonly SemaphoreSlim semaphoreSlimFactory = new SemaphoreSlim(1);

        /// <summary>
        /// Gets the single TestCommon instance, singleton pattern
        /// </summary>
        internal static TestCommon Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Name of the default test site configuration
        /// </summary>
        internal static string TestSite { get { return "TestSite"; } }

        /// <summary>
        /// Name of the default test sub site configuration
        /// </summary>
        internal static string TestSubSite { get { return "TestSubSite"; } }

        /// <summary>
        /// Name of the default no group test site configuration
        /// </summary>
        internal static string NoGroupTestSite { get { return "NoGroupTestSite"; } }

        /// <summary>
        /// Name of the default test site confguration when using an access token to authenticate
        /// </summary>
        internal static string TestSiteAccessToken { get { return "TestSiteAccessToken"; } }

        /// <summary>
        /// Set Mocking to false to switch the test system in recording mode for all contexts being created
        /// </summary>
        public bool Mocking { get; set; } = true;

        /// <summary>
        /// Generate the .request and .debug files that can be useful to debug the test mocking system, these files
        /// are not needed to run the actual tests, hence the default = false
        /// </summary>
        public bool GenerateMockingDebugFiles { get; set; } = false;

        /// <summary>
        /// Urls's used by the test cases
        /// </summary>
        public Dictionary<string, Uri> TestUris { get; set; }

        /// <summary>
        /// Private constructor since this is a singleton
        /// </summary>
        private TestCommon()
        {

        }

        public PnPContext GetContext(string configurationName, int id = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            return GetContextAsync(configurationName, id, testName, sourceFilePath).GetAwaiter().GetResult();
        }

        public async Task<PnPContext> GetContextAsync(string configurationName, int id = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            // Obtain factory (cached)
            var factory = BuildContextFactory();

            // Remove Async suffix
            if (testName.EndsWith(AsyncSuffix))
            {
                testName = testName.Substring(0, testName.Length - AsyncSuffix.Length);
            }

            // Configure the factory for our testing mode
            var testPnPContextFactory = factory as TestPnPContextFactory;
            testPnPContextFactory.Mocking = Mocking;
            testPnPContextFactory.Id = id;
            testPnPContextFactory.TestName = testName;
            testPnPContextFactory.SourceFilePath = sourceFilePath;
            testPnPContextFactory.GenerateTestMockingDebugFiles = GenerateMockingDebugFiles;
            testPnPContextFactory.TestUris = TestUris;

            return await factory.CreateAsync(configurationName).ConfigureAwait(false);
        }



        public PnPContext GetContext(Guid groupId, int id = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            return GetContextAsync(groupId, id, testName, sourceFilePath).GetAwaiter().GetResult();
        }

        public async Task<PnPContext> GetContextAsync(Guid groupId, int id = 0,
            [System.Runtime.CompilerServices.CallerMemberName] string testName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            // Obtain factory (cached)
            var factory = BuildContextFactory();

            // Configure the factory for our testing mode
            (factory as TestPnPContextFactory).Mocking = Mocking;
            (factory as TestPnPContextFactory).Id = id;
            (factory as TestPnPContextFactory).TestName = testName;
            (factory as TestPnPContextFactory).SourceFilePath = sourceFilePath;
            (factory as TestPnPContextFactory).GenerateTestMockingDebugFiles = GenerateMockingDebugFiles;
            (factory as TestPnPContextFactory).TestUris = TestUris;

            return await factory.CreateAsync(groupId).ConfigureAwait(false);
        }

        public PnPContext Clone(PnPContext source, Uri uri, int id)
        {
            var clonedContext = source.Clone(uri);
            if (source.Mode == TestMode.Mock)
            {
                clonedContext.SetMockMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }
            else
            {
                clonedContext.SetRecordingMode(id, source.TestName, source.TestFilePath, source.GenerateTestMockingDebugFiles, source.TestUris);
            }
            return clonedContext;
        }

        public static IConfigurationRoot GetConfigurationSettings()
        {
            // Define the test environment by: 
            // - Copying env.sample to env.txt  
            // - Putting the test environment name in env.txt ==> this should be same name as used in your settings file:
            //   When using appsettings.mine.json then you need to put mine as content in env.txt
            var environmentName = LoadTestEnvironment();

            if (string.IsNullOrEmpty(environmentName))
            {
                throw new Exception("Please ensure you've a env.txt file in the root of the test project. This file should contain the name of the test environment you want to use.");
            }

            // The settings file is stored in the root of the test project, no need to configure the file to be copied over the bin folder
            var jsonSettingsFile = Path.GetFullPath($"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}appsettings.{environmentName}.json");
            
            var configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonSettingsFile, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            return configuration;
        }

        public IPnPContextFactory BuildContextFactory()
        {
            try
            {
                // If a test case is already initializing the factory then let's wait
                semaphoreSlimFactory.Wait();

                if (pnpContextFactoryCache != null)
                {
                    return pnpContextFactoryCache;
                }

                var configuration = GetConfigurationSettings();

                string targetSiteUrl = configuration.GetValue<string>("PnPCore:Sites:TestSite:SiteUrl");
                string targetSubSiteUrl = configuration.GetValue<string>("PnPCore:Sites:TestSubSite:SiteUrl");
                string noGroupSiteUrl = configuration.GetValue<string>("PnPCore:Sites:NoGroupTestSite:SiteUrl");

                if (RunningInGitHubWorkflow())
                {
                    targetSiteUrl = "https://bertonline.sharepoint.com/sites/prov-1";
                    targetSubSiteUrl = "https://bertonline.sharepoint.com/sites/prov-1/testsub1";
                    noGroupSiteUrl = "https://bertonline.sharepoint.com/sites/modern";
                }

                var serviceProvider = new ServiceCollection()
                    // Configuration
                    .AddScoped<IConfiguration>(_ => configuration)
                    // Logging service, get config from appsettings + add debug output handler
                    .AddLogging(configure =>
                    {
                        configure.AddConfiguration(configuration.GetSection("Logging"));
                        configure.AddDebug();
                    })
                    // Add the PnP Core SDK library services for test
                    .AddTestPnPContextFactory()
                    // Add the PnP Core SDK library services configuration from the appsettings.json file
                    .Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"))
                    // Add the PnP Core SDK Authentication Providers
                    .AddPnPCoreAuthentication()
                    .Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"))
                .BuildServiceProvider();

                TestUris = new Dictionary<string, Uri>
                {
                    { TestSite, new Uri(targetSiteUrl) },
                    { TestSubSite, new Uri(targetSubSiteUrl) },
                    { NoGroupTestSite, new Uri(noGroupSiteUrl) }
                };

                var pnpContextFactory = serviceProvider.GetRequiredService<IPnPContextFactory>();

                if (pnpContextFactoryCache == null)
                {
                    pnpContextFactoryCache = pnpContextFactory;
                }

                return pnpContextFactory;
            }
            finally
            {
                semaphoreSlimFactory.Release();
            }
        }

        private static string LoadTestEnvironment()
        {
            // Detect if we're running in a github workflow            
            if (RunningInGitHubWorkflow())
            {
                return "ci";
            }
            else
            {
                string testEnvironmentFile = "..\\..\\..\\env.txt";
                if (File.Exists(testEnvironmentFile))
                {
                    string content = File.ReadAllText(testEnvironmentFile);
                    if (!string.IsNullOrEmpty(content))
                    {
                        return content.Trim();
                    }
                }

                return null;
            }
        }


        internal static bool RunningInGitHubWorkflow()
        {
            var runningInCI = Environment.GetEnvironmentVariable("CI");
            if (!string.IsNullOrEmpty(runningInCI))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal const string PnPCoreSDKTestPrefix = "PNP_SDK_TEST_";
        internal static string GetPnPSdkTestAssetName(string name)
        {
            return name.StartsWith(PnPCoreSDKTestPrefix) ? name : $"{PnPCoreSDKTestPrefix}{name}";
        }
    }
}
