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
    /// Tests that focus on validating the DeviceCodeAuthenticationProvider
    /// </summary>
    [TestClass]
    public class DeviceCodeAuthenticationProviderTests
    {
        private static readonly string deviceCodeConfigurationPath = "deviceCode";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestDeviceCodeWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(
                TestCommon.TestSiteDeviceCode,
                (authProvider) => {
                    ((DeviceCodeAuthenticationProvider)authProvider)
                        .DeviceCodeVerification = DeviceCodeVerificationCallback;
                }))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestDeviceCodeWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(
                TestCommon.TestSiteDeviceCode,
                (authProvider) => {
                    ((DeviceCodeAuthenticationProvider)authProvider)
                        .DeviceCodeVerification = DeviceCodeVerificationCallback;
                }))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestDeviceCodeConstructorNoDI()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
            Assert.IsNotNull(provider.DeviceCodeVerification);
        }

        [TestMethod]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestDeviceCodeConstructorNoDI_NullClientId_NullTenantId()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:TenantId");
            var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:DeviceCode:RedirectUri");

            var provider = new DeviceCodeAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri,
                n => { }); // Fake notification, we don't care of it in this test

            Assert.IsNotNull(provider);
            Assert.IsNotNull(provider.ClientId);
            Assert.IsNotNull(provider.TenantId);
            Assert.IsNotNull(provider.RedirectUri);
            Assert.IsNotNull(provider.DeviceCodeVerification);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestDeviceCodeConstructorNoDI_NullRedirectUri()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:TenantId");

            var provider = new DeviceCodeAuthenticationProvider(
                clientId,
                tenantId,
                null,
                n => { }); // Fake notification, we don't care of it in this test
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task TestDeviceCodeConstructorNoDI_NullNotification()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:TenantId");
            var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:Interactive:RedirectUri");

            var provider = new DeviceCodeAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestDeviceCodeAuthenticateRequestAsyncNoResource()
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            await provider.AuthenticateRequestAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestDeviceCodeAuthenticateRequestAsyncNoHttpRequest()
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestDeviceCodeAuthenticateRequestAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareDeviceCodeAuthenticationProvider();

            var request = new HttpRequestMessage(HttpMethod.Get, TestGlobals.GraphMeRequest);
            await provider.AuthenticateRequestAsync(TestGlobals.GraphResource, request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual(request.Headers.Authorization.Scheme.ToLower(), "bearer");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestDeviceCodeGetAccessTokenAsyncNullResource()
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            await provider.GetAccessTokenAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestDeviceCodeGetAccessTokenAsyncFullNullResource()
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            await provider.GetAccessTokenAsync(null, new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task TestDeviceCodeGetAccessTokenAsyncFullNullScopes()
        {
            var provider = PrepareDeviceCodeAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource, null);
        }

        [TestMethod]
        public async Task TestDeviceCodeGetAccessTokenAsyncCorrect()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            var provider = PrepareDeviceCodeAuthenticationProvider();

            var accessToken = await provider.GetAccessTokenAsync(TestGlobals.GraphResource);

            Assert.IsNotNull(accessToken);
            Assert.IsTrue(accessToken.Length > 0);
        }

        private static DeviceCodeAuthenticationProvider PrepareDeviceCodeAuthenticationProvider()
        {
            var configuration = TestCommon.GetConfigurationSettings();
            var clientId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:ClientId");
            var tenantId = configuration.GetValue<string>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:TenantId");
            var redirectUri = configuration.GetValue<Uri>($"{TestGlobals.CredentialsConfigurationBasePath}:{deviceCodeConfigurationPath}:DeviceCode:RedirectUri");

            var provider = new DeviceCodeAuthenticationProvider(
                clientId,
                tenantId,
                redirectUri,
                DeviceCodeVerificationCallback);
            return provider;
        }

        private static void DeviceCodeVerificationCallback(DeviceCodeNotification notification)
        {
            // Write the Device Code in the Output window
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = $"PowerShell.exe",
                    Arguments = $"-Command \"'{ notification.UserCode }' | clip\"",
                    UseShellExecute = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                });

            // Start the browser to input the Device Code
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = notification.VerificationUrl.ToString(),
                    UseShellExecute = true
                });
        }
    }
}
