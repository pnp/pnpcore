using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Test.Services;
using PnP.Core.Services;
using System;
using System.Collections.Generic;

namespace PnP.Core.Test.Utilities
{
    public sealed class TestCommon
    {
        private static readonly Lazy<TestCommon> _lazyInstance = new Lazy<TestCommon>(() => new TestCommon(), true);
        private IPnPContextFactory pnpContextFactoryCache;

        /// <summary>
        /// Get's the single TestCommon instance, singleton pattern
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
            // Obtain factory (cached)
            var factory = BuildContextFactory();

            // Configure the factory for our testing mode
            (factory as TestPnPContextFactory).Mocking = Mocking;
            (factory as TestPnPContextFactory).Id = id;
            (factory as TestPnPContextFactory).TestName = testName;
            (factory as TestPnPContextFactory).SourceFilePath = sourceFilePath;
            (factory as TestPnPContextFactory).GenerateTestMockingDebugFiles = GenerateMockingDebugFiles;
            (factory as TestPnPContextFactory).TestUris = TestUris;

            return BuildContextFactory().Create(configurationName);
        }

        public IPnPContextFactory BuildContextFactory()
        {
            if (pnpContextFactoryCache != null)
            {
                return pnpContextFactoryCache;
            }

            // Define Hosting:EnvironmentName as environment variable in Debug tab of the test project settings or as environemnt variable in your host
            var environmentName = Environment.GetEnvironmentVariable("Hosting:EnvironmentName");

            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var serviceProvider = new ServiceCollection()
                // Configuration
                .AddScoped<IConfiguration>(_ => configuration)
                // Logging service, get config from appsettings + add debug output handler
                .AddLogging(configure =>
                {
                    configure.AddConfiguration(configuration.GetSection("Logging"));
                    configure.AddDebug();
                })
                // Authentication provider factory
                .AddAuthenticationProviderFactory(options =>
                {
                    options.Configurations.Add(new OAuthCredentialManagerConfiguration
                    {
                        Name = "CredentialManagerAuthentication",
                        CredentialManagerName = configuration.GetValue<string>("CustomSettings:CredentialManager"),
                    });

                    options.DefaultConfiguration = "CredentialManagerAuthentication";
                })
                // PnP Context factory
                .AddTestPnPContextFactory(options =>
                {
                    options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                    {
                        Name = TestSite,
                        SiteUrl = new Uri(configuration.GetValue<string>("CustomSettings:TargetSiteUrl")),
                        AuthenticationProviderName = "CredentialManagerAuthentication",
                    });
                    options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                    {
                        Name = TestSubSite,
                        SiteUrl = new Uri(configuration.GetValue<string>("CustomSettings:TargetSubSiteUrl")),
                        AuthenticationProviderName = "CredentialManagerAuthentication",
                    });
                    options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                    {
                        Name = NoGroupTestSite,
                        SiteUrl = new Uri(configuration.GetValue<string>("CustomSettings:NoGroupSiteUrl")),
                        AuthenticationProviderName = "CredentialManagerAuthentication",
                    });
                })
                .BuildServiceProvider();

            TestUris = new Dictionary<string, Uri>
            {
                { TestSite, new Uri(configuration.GetValue<string>("CustomSettings:TargetSiteUrl")) },
                { TestSubSite, new Uri(configuration.GetValue<string>("CustomSettings:TargetSubSiteUrl")) },
                { NoGroupTestSite, new Uri(configuration.GetValue<string>("CustomSettings:NoGroupSiteUrl")) }
            };

            var pnpContextFactory = serviceProvider.GetRequiredService<IPnPContextFactory>();

            if (pnpContextFactoryCache == null)
            {
                pnpContextFactoryCache = pnpContextFactory;
            }

            return pnpContextFactory;
        }
    }
}
