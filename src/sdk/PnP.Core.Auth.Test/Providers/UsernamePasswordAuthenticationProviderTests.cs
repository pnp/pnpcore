using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the UsernamePasswordAuthenticationProvider
    /// </summary>
    [TestClass]
    public class UsernamePasswordAuthenticationProviderTests
    {
        private static readonly string usernamePasswordConfigurationPath = "usernamePassword";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestUsernamePasswordConstructorNoDI()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Username);
            Assert.IsNotNull(provider.Password);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestUsernamePasswordConstructorNoDI_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                null,
                "FakeUsername",
                "FakePassword".ToSecureString());

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Username);
            Assert.IsNotNull(provider.Password);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestUsernamePasswordConstructorNoDI_NullUsername()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                null,
                "FakePassword".ToSecureString());
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestUsernamePasswordConstructorNoDI_NullPassword()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestUsernamePasswordAuthenticateRequestAsyncNoResource()
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestUsernamePasswordAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestUsernamePasswordAuthenticateRequestAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var configuration = TestCommon.GetConfigurationSettings();
            var username = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:UsernamePassword:Username");
            var password = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:UsernamePassword:Password");
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:ClientId");

            var provider = new UsernamePasswordAuthenticationProvider(
                clientId,
                AuthGlobals.OrganizationsTenantId,
                username,
                password.ToSecureString());

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestUsernamePasswordGetAccessTokenAsyncNullResource()
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestUsernamePasswordGetAccessTokenAsyncFullNullResource()
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestUsernamePasswordGetAccessTokenAsyncFullNullScopes()
        {
            var provider = new UsernamePasswordAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                "FakeUsername",
                "FakePassword".ToSecureString());

            await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestUsernamePasswordGetAccessTokenAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var configuration = TestCommon.GetConfigurationSettings();
            var username = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:UsernamePassword:Username");
            var password = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:UsernamePassword:Password");
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{usernamePasswordConfigurationPath}:ClientId");

            var provider = new UsernamePasswordAuthenticationProvider(
                clientId,
                AuthGlobals.OrganizationsTenantId,
                username,
                password.ToSecureString());

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }
    }
}
