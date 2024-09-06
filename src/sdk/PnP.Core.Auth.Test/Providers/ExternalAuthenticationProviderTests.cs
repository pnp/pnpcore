using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the ExternalAuthenticationProvider
    /// </summary>
    [TestClass]
    public class ExternalAuthenticationProviderTests
    {
        private const string fakeToken = "ThisIsATestToken";
        private static readonly string externalRealProviderConfigurationPath = "externalRealProvider";

        private static readonly Lazy<CredentialManagerAuthenticationProvider> realProvider =
            new Lazy<CredentialManagerAuthenticationProvider>(() =>
            {
                var configuration = TestCommon.GetConfigurationSettings();
                var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{externalRealProviderConfigurationPath}:ClientId");
                var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{externalRealProviderConfigurationPath}:TenantId");
                var credentialManagerName = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{externalRealProviderConfigurationPath}:CredentialManager:CredentialManagerName");

                var provider = new CredentialManagerAuthenticationProvider(
                    clientId,
                    tenantId,
                    credentialManagerName);
                return provider;
            }, true);

        private static CredentialManagerAuthenticationProvider RealProvider
        {
            get
            {
                return (realProvider.Value);
            }
        }

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestExternalWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await GetTestSiteExternalContext())
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestExternalWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await GetTestSiteExternalContext())
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestExternalWithGraphNoUserTokenProvider()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await GetTestSiteExternalContext())
            {
                // Remove any already existing AccessTokenProvider
                var externalTokenProvider = (ExternalAuthenticationProvider)context.AuthenticationProvider;
                if (externalTokenProvider.AccessTokenProvider != null)
                {
                    externalTokenProvider.AccessTokenProvider = null;
                }

                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestExternalWithSPONoUserTokenProvider()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await GetTestSiteExternalContext())
            {
                // Remove any already existing AccessTokenProvider
                var externalTokenProvider = (ExternalAuthenticationProvider)context.AuthenticationProvider;
                if (externalTokenProvider.AccessTokenProvider != null)
                {
                    externalTokenProvider.AccessTokenProvider = null;
                }

                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestExternalConstructorNoDI()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = PrepareExternalAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.AccessTokenProvider);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestExternalAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareExternalAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestExternalAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareExternalAuthenticationProvider();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestExternalAuthenticateRequestAsyncCorrect()
        {
            var provider = PrepareExternalAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestExternalGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareExternalAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestExternalGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareExternalAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestExternalGetAccessTokenAsyncFullNullScopes()
        {
            var provider = PrepareExternalAuthenticationProvider();

            await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestExternalGetAccessTokenAsyncCorrect()
        {
            var provider = PrepareExternalAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        [TestMethod]
        public async Task TestExternalGetAccessTokenAsyncAsyncCorrect()
        {
            var provider = PrepareExternalAuthenticationProvider(useAsyncConstructor: true);

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }


        private static ExternalAuthenticationProvider PrepareExternalAuthenticationProvider(bool useFakeToken = false, bool useAsyncConstructor = false)
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            Func<Uri, string[], Task<string>> getAccessToken = useFakeToken
                ? (Func<Uri, string[], Task<string>>)GetFakeAccessToken
                // We get the access token using a CredentialManagerAuthenticationProvider
                : GetRealAccessToken;

            var provider = useAsyncConstructor
            ? new ExternalAuthenticationProvider(getAccessToken)
            : new ExternalAuthenticationProvider((uri, scopes) => getAccessToken(uri, scopes).GetAwaiter().GetResult());

            return provider;
        }

        private static Task<string> GetFakeAccessToken(Uri resource, string[] scopes)
        {
            return Task.FromResult(fakeToken);
        }

        private static async Task<string> GetRealAccessToken(Uri resource, string[] scopes)
        {
            return await RealProvider.GetAccessTokenAsync(resource, scopes);
        }

        private static async Task<PnPContext> GetTestSiteExternalContext()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var siteUrl = configuration.GetValue<Uri>($"{TestGlobals.SitesConfigurationBasePath}:{TestCommon.TestSiteExternal}:SiteUrl");

            return await TestCommon.Instance.GetContextAsync(siteUrl, PrepareExternalAuthenticationProvider());
        }
    }
}
