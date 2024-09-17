using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Test.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Test.Common.Utilities
{
    public class TestCommonBase
    {
        internal const string PnPCoreSDKTestPrefix = "PNP_SDK_TEST_";

        protected const string AsyncSuffix = "_Async";
        protected const string PnPCoreSDKTestSite = "pnpcoresdktestsite";
        protected const string PnPCoreSDKTestUser = "pnpcoresdktestuser";
        protected const string PnPCoreSDKTestUserPassword = "pnpcoresdktestuserpassword";
        protected const string PnPCoreSDKTestClientId = "pnpcoresdktestclientid";
        protected IPnPTestContextFactory pnpContextFactoryCache;
        protected static readonly SemaphoreSlim semaphoreSlimFactory = new SemaphoreSlim(1);

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
        /// Name of the default no group test site configuration
        /// </summary>
        internal static string ClassicSTS0TestSite { get { return "ClassicSTS0TestSite"; } }

        /// <summary>
        /// Name of the tenant admin center site configuration
        /// </summary>
        internal static string TenantAdminCenterSite { get { return "TenantAdminCenterSite"; } }

        /// <summary>
        /// Name of the default Syntex Content Center test site confguration
        /// </summary>
        internal static string SyntexContentCenterTestSite { get { return "SyntexContentCenterTestSite"; } }

        /// <summary>
        /// Name of the default Syntex Content Center test site confguration
        /// </summary>
        internal static string VivaTopicCenterTestSite { get { return "VivaTopicCenterTestSite"; } }

        /// <summary>
        /// Name of the default test site confguration when using an access token to authenticate
        /// </summary>
        internal static string TestSiteAccessToken { get { return "TestSiteAccessToken"; } }

        /// <summary>
        /// Name of the site collection app catalog site
        /// </summary>
        internal static string SiteCollectionAppCatalogSite { get { return "SiteCollectionAppCatalogSite"; } }

        /// <summary>
        /// Name of the site collection app catalog site
        /// </summary>
        internal static string HomeTestSite { get { return "HomeTestSite"; } }

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
        /// Run the tests using application permissions instead of delegated permissions, this can be setup per test, test class or globally
        /// </summary>
        public bool UseApplicationPermissions { get; set; }

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
            configurationName = ConfigurePnPContextFactory(configurationName, id, testName, sourceFilePath, factory);

            return await factory.CreateAsync(configurationName).ConfigureAwait(false);
        }

        private string RewriteConfigurationNameForOptionalOfflineTestConfigurations(string configurationName)
        {
            if (Mocking &&
                configurationName == ClassicSTS0TestSite || configurationName == SyntexContentCenterTestSite || configurationName == VivaTopicCenterTestSite || configurationName == HomeTestSite)
            {
                var configuration = GetConfigurationSettings();
                if (configurationName == SyntexContentCenterTestSite && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:SyntexContentCenterTestSite:SiteUrl")))
                {
                    configurationName = TestSite;
                }
                else if (configurationName == VivaTopicCenterTestSite && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:VivaTopicCenterTestSite:SiteUrl")))
                {
                    configurationName = TestSite;
                }
                else if (configurationName == ClassicSTS0TestSite && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:ClassicSTS0TestSite:SiteUrl")))
                {
                    configurationName = TestSite;
                }
                else if (configurationName == HomeTestSite && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:HomeTestSite:SiteUrl")))
                {
                    configurationName = TestSite;
                }
            }

            return configurationName;
        }

        internal async Task<PnPContext> GetContextWithTelemetryManagerAsync(string configurationName, TelemetryManager telemetryManager, int id = 0,
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
            configurationName = ConfigurePnPContextFactory(configurationName, id, testName, sourceFilePath, factory);

            return await (factory as TestPnPContextFactory).CreateWithTelemetryManagerAsync(configurationName, telemetryManager).ConfigureAwait(false);
        }

        public async Task<PnPContext> GetContextWithoutInitializationAsync(string configurationName, int id = 0,
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
            configurationName = ConfigurePnPContextFactory(configurationName, id, testName, sourceFilePath, factory);

            return await factory.CreateWithoutInitializationAsync(configurationName).ConfigureAwait(false);
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
            (factory as TestPnPContextFactory).SourceFilePath = UseApplicationPermissions ? $"{sourceFilePath}{Constants.ApplicationPermissionsSuffix}" : sourceFilePath;
            (factory as TestPnPContextFactory).GenerateTestMockingDebugFiles = GenerateMockingDebugFiles;
            (factory as TestPnPContextFactory).TestUris = TestUris;
            (factory as TestPnPContextFactory).UseApplicationPermissions = UseApplicationPermissions;

            return await factory.CreateAsync(groupId).ConfigureAwait(false);
        }

        internal async Task<PnPContext> GetLiveContextAsync()
        {
            // Obtain factory (cached)
            var factory = BuildContextFactory();

            var pnpCoreSDKTestUserPassword = Environment.GetEnvironmentVariable(PnPCoreSDKTestUserPassword);
            var pnpCoreSDKTestUser = Environment.GetEnvironmentVariable(PnPCoreSDKTestUser);
            var pnpCoreSDKTestSite = Environment.GetEnvironmentVariable(PnPCoreSDKTestSite);
            var pnpCoreSDKTestClientId = Environment.GetEnvironmentVariable(PnPCoreSDKTestClientId);

            var pwd = new NetworkCredential(null, pnpCoreSDKTestUserPassword).SecurePassword;

            var context = await factory.CreateLiveAsync(new Uri(pnpCoreSDKTestSite), new UsernamePasswordAuthenticationProvider(pnpCoreSDKTestClientId, null, pnpCoreSDKTestUser, pwd)).ConfigureAwait(false);

            return context;
        }

        public async Task<PnPContext> GetContextWithOptionsAsync(string configurationName, PnPContextOptions options, int id = 0,
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
            configurationName = ConfigurePnPContextFactory(configurationName, id, testName, sourceFilePath, factory);

            return await factory.CreateAsync(configurationName, options).ConfigureAwait(false);
        }

        public async Task<PnPContext> CloneAsync(PnPContext source, int id)
        {
            return await source.CloneForTestingAsync(source, null, null, id).ConfigureAwait(false);
        }

        public async Task<PnPContext> CloneAsync(PnPContext source, Uri uri, int id)
        {
            return await source.CloneForTestingAsync(source, uri, null, id).ConfigureAwait(false);
        }

        public async Task<PnPContext> CloneAsync(PnPContext source, string configuration, int id)
        {
            return await source.CloneForTestingAsync(source, null, configuration, id).ConfigureAwait(false);
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

        public IPnPTestContextFactory BuildContextFactory()
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
                string classicSTS0SiteUrl = configuration.GetValue<string>("PnPCore:Sites:ClassicSTS0TestSite:SiteUrl");
                string tenantAdminCenterSiteUrl = configuration.GetValue<string>("PnPCore:Sites:TenantAdminCenterSite:SiteUrl");
                string syntexContentCenterSiteUrl = configuration.GetValue<string>("PnPCore:Sites:SyntexContentCenterTestSite:SiteUrl");
                string vivaTopicCenterSiteUrl = configuration.GetValue<string>("PnPCore:Sites:VivaTopicCenterTestSite:SiteUrl");
                string homeSiteUrl = configuration.GetValue<string>("PnPCore:Sites:HomeTestSite:SiteUrl");

                if (RunningInGitHubWorkflow())
                {
                    targetSiteUrl = "https://bertonline.sharepoint.com/sites/prov-1";
                    targetSubSiteUrl = "https://bertonline.sharepoint.com/sites/prov-1/testsub1";
                    noGroupSiteUrl = "https://bertonline.sharepoint.com/sites/modern";
                    classicSTS0SiteUrl = "https://bertonline.sharepoint.com/sites/sts0";
                    tenantAdminCenterSiteUrl = "https://bertonline-admin.sharepoint.com";
                    syntexContentCenterSiteUrl = "https://bertonline.sharepoint.com/sites/syntextcc";
                    homeSiteUrl = "https://bertonline.sharepoint.com";
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

                if (!string.IsNullOrEmpty(classicSTS0SiteUrl))
                {
                    TestUris.Add("ClassicSTS0TestSite", new Uri(classicSTS0SiteUrl));
                }

                if (!string.IsNullOrEmpty(tenantAdminCenterSiteUrl))
                {
                    TestUris.Add("TenantAdminCenterSite", new Uri(tenantAdminCenterSiteUrl));
                }

                if (!string.IsNullOrEmpty(syntexContentCenterSiteUrl))
                {
                    TestUris.Add("SyntexContentCenterTestSite", new Uri(syntexContentCenterSiteUrl));
                }

                if (!string.IsNullOrEmpty(vivaTopicCenterSiteUrl))
                {
                    TestUris.Add("VivaTopicCenterTestSite", new Uri(vivaTopicCenterSiteUrl));
                }

                if (!string.IsNullOrEmpty(homeSiteUrl))
                {
                    TestUris.Add("HomeTestSite", new Uri(homeSiteUrl));
                }

                var pnpContextFactory = serviceProvider.GetRequiredService<IPnPTestContextFactory>();

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

        private string ConfigurePnPContextFactory(string configurationName, int id, string testName, string sourceFilePath, IPnPTestContextFactory factory)
        {
            sourceFilePath = Path.GetFullPath(sourceFilePath);

            var testPnPContextFactory = factory as TestPnPContextFactory;
            testPnPContextFactory.Mocking = Mocking;
            testPnPContextFactory.Id = id;
            testPnPContextFactory.TestName = testName;
            testPnPContextFactory.SourceFilePath = UseApplicationPermissions ? $"{sourceFilePath}{Constants.ApplicationPermissionsSuffix}" : sourceFilePath;
            testPnPContextFactory.GenerateTestMockingDebugFiles = GenerateMockingDebugFiles;
            testPnPContextFactory.TestUris = TestUris;
            testPnPContextFactory.UseApplicationPermissions = UseApplicationPermissions;

            // rewrite configuration for special cases
            configurationName = RewriteConfigurationNameForOptionalOfflineTestConfigurations(configurationName);
            return configurationName;
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
                var testEnvironmentFile = Path.Combine("..", "..", "..", "env.txt");
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

        internal static string GetPnPSdkTestAssetName(string name)
        {
            return name.StartsWith(PnPCoreSDKTestPrefix) ? name : $"{PnPCoreSDKTestPrefix}{name}";
        }

        internal static string GetTempFileName(string extension = "tmp")
        {
            return $"{Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Split(".")[0])}.{extension}";
        }
    }
}
