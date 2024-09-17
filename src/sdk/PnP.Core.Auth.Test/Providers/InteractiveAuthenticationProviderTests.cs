using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the InteractiveAuthenticationProvider
    /// </summary>
    [TestClass]
    public class InteractiveAuthenticationProviderTests
    {
        private static readonly string interactiveConfigurationPath = "interactive";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestInteractiveWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteInteractive))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestInteractiveWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteInteractive))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestInteractiveConstructorNoDI()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestInteractiveConstructorNoDI_NullClientId_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:TenantId");
            var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:Interactive:RedirectUri");

            var provider = new InteractiveAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri);

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestInteractiveConstructorNoDI_NullRedirectUri()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:TenantId");

            var provider = new InteractiveAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri: null);

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestInteractiveConstructorNoDI_DefaultValues()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:ClientId");
            Uri test = null;
            var provider = new InteractiveAuthenticationProvider(clientId, null, test);

            Assert.IsNotNull(provider);
            // ClientId is required now
            //Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestInteractiveAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestInteractiveAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestInteractiveAuthenticateRequestAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareInteractiveAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestInteractiveGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestInteractiveGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestInteractiveGetAccessTokenAsyncFullNullScopes()
        {
            var provider = PrepareInteractiveAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestInteractiveGetAccessTokenAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareInteractiveAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static InteractiveAuthenticationProvider PrepareInteractiveAuthenticationProvider()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:TenantId");
            var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{interactiveConfigurationPath}:Interactive:RedirectUri");

            var provider = new InteractiveAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri);
            return provider;
        }
    }
}
