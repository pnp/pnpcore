using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Common.Utilities;
using System;

namespace PnP.Core.Test.Utilities
{
    public sealed class TestCommon : TestCommonBase
    {
        private static readonly Lazy<TestCommon> _lazyInstance = new Lazy<TestCommon>(() => new TestCommon(), true);

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
        /// Private constructor since this is a singleton
        /// </summary>
        private TestCommon()
        {

        }

        internal static void PnPCoreSDKTestUserSetup()
        {
            var pnpCoreSDKTestUserPassword = Environment.GetEnvironmentVariable(PnPCoreSDKTestUserPassword);
            var pnpCoreSDKTestUser = Environment.GetEnvironmentVariable(PnPCoreSDKTestUser);
            var pnpCoreSDKTestSite = Environment.GetEnvironmentVariable(PnPCoreSDKTestSite);
            var pnpCoreSDKTestClientId = Environment.GetEnvironmentVariable(PnPCoreSDKTestClientId);
            if (string.IsNullOrEmpty(pnpCoreSDKTestUser) || string.IsNullOrEmpty(pnpCoreSDKTestUserPassword) || string.IsNullOrEmpty(pnpCoreSDKTestSite) || string.IsNullOrEmpty(pnpCoreSDKTestClientId))
            {
                Assert.Inconclusive("Skipping test because 'live' tests are not configured. Add pnpcoresdktestsite, pnpcoresdktestuser, pnpcoresdktestuserpassword and pnpCoreSDKTestClientId environment variables");
            }
        }

        internal static void SharePointSyntexTestSetup()
        {
            var configuration = GetConfigurationSettings();
            if (!Instance.Mocking && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:SyntexContentCenterTestSite:SiteUrl")))
            {
                Assert.Inconclusive("No Syntex Content Center setup for live testing");
            }
        }

        internal static void SharePointVivaTopicsTestSetup()
        {
            var configuration = GetConfigurationSettings();
            if (!Instance.Mocking && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:VivaTopicCenterTestSite:SiteUrl")))
            {
                Assert.Inconclusive("No Viva Topic Center setup for live testing");
            }
        }

        internal static void ClassicSTS0TestSetup()
        {
            var configuration = GetConfigurationSettings();
            if (!Instance.Mocking && string.IsNullOrEmpty(configuration.GetValue<string>("PnPCore:Sites:ClassicSTS0TestSite:SiteUrl")))
            {
                Assert.Inconclusive("No classic STS#0 site setup for live testing");
            }
        }

    }
}
