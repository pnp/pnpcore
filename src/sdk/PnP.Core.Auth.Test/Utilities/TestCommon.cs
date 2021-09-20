using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Test.Common.Utilities;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Utilities
{
    public sealed class TestCommon : TestCommonBase
    {
        private static readonly Lazy<TestCommon> _lazyInstance = new Lazy<TestCommon>(() => new TestCommon(), true);
        private new IPnPContextFactory pnpContextFactoryCache;

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

        public async Task<PnPContext> GetContextAsync(string configurationName,
            Action<IAuthenticationProvider> initializeAuthenticationProvider = null,
            int id = 0,
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

            return await factory.CreateAsync(configurationName, initializeAuthenticationProvider).ConfigureAwait(false);
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

        public new IPnPContextFactory BuildContextFactory()
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
            await context.Web.EnsurePropertiesAsync(w => w.Title);

            Assert.IsNotNull(context.Web.Title);
            Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Title));
        }

        internal static string GetX509CertificateThumbprint()
        {
            return "DF5450F6FB23838465128BBFC95C86091504B16B";

            //var configuration = GetConfigurationSettings();
            //return configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:x509Certificate:X509Certificate:Thumbprint");
        }

    }
}
