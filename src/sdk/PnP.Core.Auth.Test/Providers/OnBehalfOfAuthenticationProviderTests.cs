using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the OnBehalfOfAuthenticationProvider
    /// </summary>
    [TestClass]
    public class OnBehalfOfAuthenticationProviderTests
    {
        private static readonly string onBehalfOfConfigurationPath = "onBehalfOf";
        private static readonly string onBehalfOfFrontEndConfigurationPath = "onBehalfFrontEnd";

        private static readonly Lazy<InteractiveAuthenticationProvider> frontendProvider =
            new Lazy<InteractiveAuthenticationProvider>(() =>
            {
                var configuration = TestCommon.GetConfigurationSettings();
                var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfFrontEndConfigurationPath}:ClientId");
                var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfFrontEndConfigurationPath}:TenantId");
                var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfFrontEndConfigurationPath}:Interactive:RedirectUri");

                var provider = new InteractiveAuthenticationProvider(
                    clientId,
                    tenantId,
                    redirectUri);
                return provider;
            }, true);

        private static InteractiveAuthenticationProvider FrontEndProvider
        {
            get
            {
                return (frontendProvider.Value);
            }
        }

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Install the debug cert in the certstore ~ this works on Linux as well
            string path = $"TestAssets{Path.DirectorySeparatorChar}pnp.pfx";
            using (X509Certificate2 certificate = new X509Certificate2(path, "PnPRocks!"))
            {
                X509Store xstore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                xstore.Open(OpenFlags.ReadWrite);
                xstore.Add(certificate);
                xstore.Close();
            }
        }
        
        [TestMethod]
        public async Task TestOnBehalfOfWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(
                TestCommon.TestSiteOnBehalfOf,
                (authProvider) =>
                {
                    ((OnBehalfOfAuthenticationProvider)authProvider)
                        .UserTokenProvider = () => GetUserAccessToken().GetAwaiter().GetResult();
                }))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestOnBehalfOfWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf,
                (authProvider) =>
                {
                    ((OnBehalfOfAuthenticationProvider)authProvider)
                        .UserTokenProvider = () => GetUserAccessToken().GetAwaiter().GetResult();
                }))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestOnBehalfOfWithGraphNoUserTokenProvider()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
            {
                // Remove any already existing UserTokenProvider
                var onBehalfTokenProvider = (OnBehalfOfAuthenticationProvider)context.AuthenticationProvider;
                if (onBehalfTokenProvider.UserTokenProvider != null)
                {
                    onBehalfTokenProvider.UserTokenProvider = null;
                }

                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestOnBehalfOfWithSPONoUserTokenProvider()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
            {
                // Remove any already existing UserTokenProvider
                var onBehalfTokenProvider = (OnBehalfOfAuthenticationProvider)context.AuthenticationProvider;
                if (onBehalfTokenProvider.UserTokenProvider != null)
                {
                    onBehalfTokenProvider.UserTokenProvider = null;
                }

                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithCertificate()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithClientSecret()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.ClientSecret);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithCertificateFile_NullClientId_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:Thumbprint");

            var certificateFromFile = X509CertificateUtility.LoadCertificate(storeName, storeLocation, thumbprint);

            //string path = $"TestAssets{Path.DirectorySeparatorChar}pnp.pfx";
            var provider = new OnBehalfOfAuthenticationProvider(
                null,
                null,
                certificate: certificateFromFile,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());
            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithCertificate_NullClientId_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:Thumbprint");

            var provider = new OnBehalfOfAuthenticationProvider(
                null,
                null,
                storeName,
                storeLocation,
                thumbprint,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithClientSecret_NullClientId_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientSecret = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:ClientSecret");

            var provider = new OnBehalfOfAuthenticationProvider(
                null,
                null,
                clientSecret.ToSecureString(),
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.ClientSecret);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithCertificate_NullThumbprint()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");

            var provider = new OnBehalfOfAuthenticationProvider(
                AuthGlobals.DefaultClientId,
                AuthGlobals.OrganizationsTenantId,
                storeName,
                storeLocation,
                null,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestOnBehalfOfConstructorNoDIWithClientSecret_NullClientSecret()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");

            var provider = new OnBehalfOfAuthenticationProvider(
                AuthGlobals.DefaultClientId,
                AuthGlobals.OrganizationsTenantId,
                clientSecret: null,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncNoResourceWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncNoResourceWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncNoHttpRequestWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncNoHttpRequestWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncCorrectWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        public async Task TestOnBehalfOfAuthenticateRequestAsyncCorrectWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncNullResourceWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncNullResourceWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncFullNullResourceWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncFullNullResourceWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncFullNullScopesWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestOnBehalfOfGetAccessTokenAsyncFullNullScopesWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestOnBehalfOfGetAccessTokenAsyncCorrectWithCertificate()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificate();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        [TestMethod]
        public async Task TestOnBehalfOfGetAccessTokenAsyncCorrectWithCertificateFile()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithCertificateFile();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }
        
        [TestMethod]
        public async Task TestOnBehalfOfGetAccessTokenAsyncCorrectWithClientSecret()
        {
            var provider = PrepareOnBehalfOfAuthenticationProviderWithClientSecret();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static OnBehalfOfAuthenticationProvider PrepareOnBehalfOfAuthenticationProviderWithCertificate()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:TenantId");
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:Thumbprint");

            var provider = new OnBehalfOfAuthenticationProvider(
                clientId,
                tenantId,
                storeName,
                storeLocation,
                thumbprint,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());

            return provider;
        }

        private static OnBehalfOfAuthenticationProvider PrepareOnBehalfOfAuthenticationProviderWithCertificateFile()
        {
            if (TestCommon.RunningInGitHubWorkflow())
                Assert.Inconclusive(
                    "Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:TenantId");
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:Thumbprint");

            var certificateFromFile = X509CertificateUtility.LoadCertificate(storeName, storeLocation, thumbprint);
            
            var provider = new OnBehalfOfAuthenticationProvider(
                clientId,
                tenantId,
                certificateFromFile,
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());
            
            return provider;
        }

        private static OnBehalfOfAuthenticationProvider PrepareOnBehalfOfAuthenticationProviderWithClientSecret()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:TenantId");
            var clientSecret = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{onBehalfOfConfigurationPath}:OnBehalfOf:ClientSecret");

            var provider = new OnBehalfOfAuthenticationProvider(
                clientId,
                tenantId,
                clientSecret.ToSecureString(),
                // We get the consumer access token using an InteractiveAuthenticationProvider
                () => GetUserAccessToken().GetAwaiter().GetResult());

            return provider;
        }

        private static async Task<string> GetUserAccessToken()
        {
            return await FrontEndProvider.GetAccessTokenAsync(TestGlobals.OnBehalfOfBackendResource);
            //, new string[] { "api://pnp.core.test.onbehalfof.backend/Backend.Consume" });
        }
    }
}
