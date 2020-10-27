using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Utilities
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
        /// Name of the default test site configuration with Username and Password delegated authentication
        /// </summary>
        internal static string TestSiteUsernamePassword { get { return "TestSiteUsernamePassword"; } }

        /// <summary>
        /// Name of the default test site configuration with Credential Manager delegated authentication
        /// </summary>
        internal static string TestSiteCredentialManager { get { return "TestSiteCredentialManager"; } }

        /// <summary>
        /// Name of the default test site configuration with OnBehalfOf app-omly authentication
        /// </summary>
        internal static string TestSiteOnBehalfOf { get { return "TestSiteOnBehalfOf"; } }

        /// <summary>
        /// Name of the default test site configuration with AspNetCore delegated authentication
        /// </summary>
        internal static string TestSiteExternal { get { return "TestSiteExternal"; } }

        /// <summary>
        /// Name of the default test site configuration with X.509 Certificate app-only authentication
        /// </summary>
        internal static string TestSiteX509Certificate { get { return "TestSiteX509Certificate"; } }

        /// <summary>
        /// Name of the default test site configuration with Interactive authentication
        /// </summary>
        internal static string TestSiteInteractive { get { return "TestSiteInteractive"; } }

        /// <summary>
        /// Name of the default test site configuration with Device Code authentication
        /// </summary>
        internal static string TestSiteDeviceCode { get { return "TestSiteDeviceCode"; } }

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

            return await factory.CreateAsync(configurationName).ConfigureAwait(false);
        }

        public async Task<PnPContext> GetContextAsync(Uri url, IAuthenticationProvider authenticationProvider,
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

            return await factory.CreateAsync(url, authenticationProvider).ConfigureAwait(false);
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

            return await factory.CreateAsync(groupId).ConfigureAwait(false);
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

                if (RunningInGitHubWorkflow())
                {
                    // NOOP so far
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
                    // Add the PnP Core SDK library services
                    .AddPnPCore().Services
                    // Add the PnP Core SDK library services configuration from the appsettings.json file
                    .Configure<PnPCoreOptions>(configuration.GetSection("PnPCore"))
                    // Add the PnP Core SDK Authentication Providers
                    .AddPnPCoreAuthentication()
                    .Configure<PnPCoreAuthenticationOptions>(configuration.GetSection("PnPCore"))
                .BuildServiceProvider();

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

        internal static async Task CheckAccessToTargetResource(PnPContext context, bool graphFirst = true)
        {
            context.GraphFirst = graphFirst;
            var web = await context.Web.GetAsync(w => w.Title);

            Assert.IsNotNull(web.Title);
            Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
        }

        internal static string GetX509CertificateThumbprint()
        {
            var configuration = GetConfigurationSettings();
            return configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:x509Certificate:X509Certificate:Thumbprint");
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
                string testEnvironmentFile = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}env.txt";
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
    }
}
