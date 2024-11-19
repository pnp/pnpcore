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
    /// Tests that focus on validating the X509CertificateAuthenticationProvider
    /// </summary>
    [TestClass]
    public class X509CertificateAuthenticationProviderTests
    {
        private static readonly string x509CertificateConfigurationPath = "x509Certificate";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Install the debug cert in the certstore ~ this works on Linux as well
            string path = $"TestAssets{Path.DirectorySeparatorChar}pnp.pfx";
#pragma warning disable SYSLIB0057 // Type or member is obsolete
            using (X509Certificate2 certificate = new X509Certificate2(path, "PnPRocks!"))
            {
                X509Store xstore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                xstore.Open(OpenFlags.ReadWrite);
                xstore.Add(certificate);
                xstore.Close();
            }
#pragma warning restore SYSLIB0057 // Type or member is obsolete
        }

        [TestMethod]
        public async Task TestX509CertificateWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestX509CertificateConstructorNoDI()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var provider = PrepareX509CertificateAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestX509CertificateConstructorNoDI_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:Thumbprint");
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:ClientId");

            var provider = new X509CertificateAuthenticationProvider(
                clientId,
                null,
                storeName,
                storeLocation,
                thumbprint);

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.Certificate);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestX509CertificateConstructorNoDI_NullThumbprint()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreLocation");

            var provider = new X509CertificateAuthenticationProvider(
                TestGlobals.FakeClientId,
                AuthGlobals.OrganizationsTenantId,
                storeName,
                storeLocation,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestX509CertificateAuthenticateRequestAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var provider = PrepareX509CertificateAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestX509CertificateGetAccessTokenAsyncFullNullScopes()
        {
            var provider = PrepareX509CertificateAuthenticationProvider();

            await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestX509CertificateGetAccessTokenAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var provider = PrepareX509CertificateAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static X509CertificateAuthenticationProvider PrepareX509CertificateAuthenticationProvider()
        {
            //if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping test because we're running inside a GitHub action and we don't have access to the certificate store");

            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:TenantId");
            var storeName = configuration.GetValue<StoreName>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreName");
            var storeLocation = configuration.GetValue<StoreLocation>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:StoreLocation");
            var thumbprint = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{x509CertificateConfigurationPath}:X509Certificate:Thumbprint");

            var provider = new X509CertificateAuthenticationProvider(
                clientId,
                tenantId,
                storeName,
                storeLocation,
                thumbprint);
            return provider;
        }
    }
}
