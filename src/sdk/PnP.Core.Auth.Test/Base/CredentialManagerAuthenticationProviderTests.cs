using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the CredentialManagerAuthenticationProvider
    /// </summary>
    [TestClass]
    public class CredentialManagerAuthenticationProviderTests
    {
        private static Uri graphResource = new Uri("https://graph.microsoft.com");
        private static string graphMeRequest = "https://graph.microsoft.com/v1.0/me";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestCredentialManagerWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerConstructorNoDI()
        {
            var provider = PrepareCredentialManagerAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.CredentialManagerName);
        }

        [TestMethod]
        public async Task TestCredentialManagerConstructorNoDI_NullClientId_NullTenantId()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var credentialManagerName = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:CredentialManager:CredentialManagerName");

            var provider = new CredentialManagerAuthenticationProvider(
                null,
                null,
                credentialManagerName);

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.CredentialManagerName);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public async Task TestCredentialManagerConstructorNoDI_NullCredentialManagerName()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:ClientId");
            var tenantId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:TenantId");

            var provider = new CredentialManagerAuthenticationProvider(
                clientId,
                tenantId,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestCredentialManagerAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareCredentialManagerAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestCredentialManagerAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareCredentialManagerAuthenticationProvider();

            await provider.AuthenticateRequestAsync(graphResource, null);
        }

        [TestMethod]
        public async Task TestCredentialManagerAuthenticateRequestAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareCredentialManagerAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, graphMeRequest);
            await provider.AuthenticateRequestAsync(graphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestCredentialManagerGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareCredentialManagerAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestCredentialManagerGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareCredentialManagerAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        public async Task TestCredentialManagerGetAccessTokenAsyncFullNullScopes()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareCredentialManagerAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(graphResource, null);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        [TestMethod]
        public async Task TestCredentialManagerGetAccessTokenAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareCredentialManagerAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(graphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static CredentialManagerAuthenticationProvider PrepareCredentialManagerAuthenticationProvider()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:ClientId");
            var tenantId = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:TenantId");
            var credentialManagerName = configuration.GetValue<string>("PnPCore:Credentials:Configurations:TestSiteCredentialManager:CredentialManager:CredentialManagerName");

            var provider = new CredentialManagerAuthenticationProvider(
                clientId,
                tenantId,
                credentialManagerName);
            return provider;
        }
    }
}
