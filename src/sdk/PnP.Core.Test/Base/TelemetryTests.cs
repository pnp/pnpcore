using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Test.Common.Services;
using PnP.Core.Test.Services.Core.CSOM.Requests;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class TelemetryTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void InitPropertiesTest()
        {
            TelemetryManager tm = new TelemetryManager((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions());
            var initProps = tm.PopulateInitProperties();

            Assert.IsTrue(initProps.ContainsKey("PnPCoreSDKVersion"));
            Assert.IsTrue(!string.IsNullOrEmpty(initProps["PnPCoreSDKVersion"]));
            Assert.IsTrue(initProps.ContainsKey("AADTenantId"));
            Assert.IsTrue(!string.IsNullOrEmpty(initProps["AADTenantId"]));
            Assert.IsTrue(initProps.ContainsKey("OS"));
            Assert.IsTrue(!string.IsNullOrEmpty(initProps["OS"]));
        }

        [TestMethod]
        public async Task GetViaRest()
        {
            //TestCommon.Instance.Mocking = false;

            bool requestPassed = false;
            int numberOfRequestsPassed = 0;

            // Configure fake telemetry manager
            var telemetryManager = new TestTelemetryManager((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions())
            {
                TelemetryEvent = (properties, eventName) =>
                {
                    requestPassed = true;
                    numberOfRequestsPassed++;

                    if (numberOfRequestsPassed == 3)
                    {
                        // The first 2 requests are initializing data, ignore them
                        Assert.IsTrue(properties["Model"] == "PnP.Core.Model.SharePoint.Web");
                        Assert.IsTrue(properties["ApiType"] == "SPORest");
                        Assert.IsTrue(properties["ApiMethod"] == "GET");
                        Assert.IsTrue(properties["Operation"] == "Get");
                    }
                }
            };

            // GetContextWithTelemetryManagerAsync will only result on telemetry Request events, the init event will not fire
            using (var context = await TestCommon.Instance.GetContextWithTelemetryManagerAsync(TestCommon.TestSite, telemetryManager))
            {
                // Perform a get that will be resolved via rest
                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Verify the telemetry events did fire
                Assert.IsTrue(requestPassed);
                Assert.IsTrue(numberOfRequestsPassed == 3);
            }
        }

        [TestMethod]
        public async Task GetViaInteractiveRest()
        {
            //TestCommon.Instance.Mocking = false;

            bool requestPassed = false;
            int numberOfRequestsPassed = 0;

            // Configure fake telemetry manager
            var telemetryManager = new TestTelemetryManager((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions())
            {
                TelemetryEvent = (properties, eventName) =>
                {
                    requestPassed = true;
                    numberOfRequestsPassed++;

                    if (numberOfRequestsPassed == 3)
                    {
                        // The first 2 requests are initializing data, ignore them
                        Assert.IsTrue(properties["Model"] == "PnP.Core.Model.SharePoint.Web");
                        Assert.IsTrue(properties["ApiType"] == "SPORest");
                        Assert.IsTrue(properties["ApiMethod"] == "GET");
                        Assert.IsTrue(properties["Operation"] == "Get");
                    }
                }
            };

            // GetContextWithTelemetryManagerAsync will only result on telemetry Request events, the init event will not fire
            using (var context = await TestCommon.Instance.GetContextWithTelemetryManagerAsync(TestCommon.TestSite, telemetryManager))
            {
                // Perform a get that will be resolved via an interactive rest request
                var api = new ApiCall($"_api/web?$select=Id%2cWelcomePage", ApiType.SPORest)
                {
                    Interactive = true
                };

                await (context.Web as Web).RequestAsync(api, HttpMethod.Get, "GetAsync");

                // Verify the telemetry events did fire
                Assert.IsTrue(requestPassed);
                Assert.IsTrue(numberOfRequestsPassed == 3);
            }
        }

        [TestMethod]
        public async Task GetViaGraph()
        {
            //TestCommon.Instance.Mocking = false;

            bool requestPassed = false;
            int numberOfRequestsPassed = 0;

            // Configure fake telemetry manager
            var telemetryManager = new TestTelemetryManager((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions())
            {
                TelemetryEvent = (properties, eventName) =>
                {
                    requestPassed = true;
                    numberOfRequestsPassed++;

                    if (numberOfRequestsPassed == 3)
                    {
                        // The first 2 requests are initializing data, ignore them
                        Assert.IsTrue(properties["Model"] == "PnP.Core.Model.SharePoint.Web");
                        Assert.IsTrue(properties["ApiType"] == "Graph");
                        Assert.IsTrue(properties["ApiMethod"] == "GET");
                        Assert.IsTrue(properties["Operation"] == "Get");
                    }
                }
            };

            // GetContextWithTelemetryManagerAsync will only result on telemetry Request events, the init event will not fire
            using (var context = await TestCommon.Instance.GetContextWithTelemetryManagerAsync(TestCommon.TestSite, telemetryManager))
            {
                // Perform a get that will be resolved via Graph
                var web = await context.Web.GetAsync(p => p.Title);

                // Verify the telemetry events did fire
                Assert.IsTrue(requestPassed);
                Assert.IsTrue(numberOfRequestsPassed == 3);
            }
        }

        [TestMethod]
        public async Task GetViaCSOM()
        {
            //TestCommon.Instance.Mocking = false;

            bool requestPassed = false;
            int numberOfRequestsPassed = 0;

            // Configure fake telemetry manager
            var telemetryManager = new TestTelemetryManager((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions())
            {
                TelemetryEvent = (properties, eventName) =>
                {
                    requestPassed = true;
                    numberOfRequestsPassed++;

                    if (numberOfRequestsPassed == 3)
                    {
                        // The first 2 requests are initializing data, ignore them
                        Assert.IsTrue(properties["Model"] == "PnP.Core.Model.SharePoint.Web");
                        Assert.IsTrue(properties["ApiType"] == "CSOM");
                        Assert.IsTrue(properties["ApiMethod"] == "POST");
                        Assert.IsTrue(properties["Operation"] == "Get");
                    }
                }
            };

            // GetContextWithTelemetryManagerAsync will only result on telemetry Request events, the init event will not fire
            using (var context = await TestCommon.Instance.GetContextWithTelemetryManagerAsync(TestCommon.TestSite, telemetryManager))
            {
                // Perform a get that will be resolved via CSOM
                var apiCall = new ApiCall(new List<IRequest<object>>() { new GetTitleRequest() });
                await (context.Web as Web).RawRequestAsync(apiCall, HttpMethod.Post, "Get");

                // Verify the telemetry events did fire
                Assert.IsTrue(requestPassed);
                Assert.IsTrue(numberOfRequestsPassed == 3);
            }
        }

        [TestMethod]
        public async Task TelemetryRoundtrip()
        {
            //TestCommon.Instance.Mocking = false;

            // As there's a live API call here (to get the tenant id) let's skip this when running via a GitHub action
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            bool requestPassed = false;
            int numberOfRequestsPassed = 0;
            
            try
            {
                // Hookup a fake TestManager class that simply returns the created telemetry properties versus sending them to AppInsights.
                // Doing this will allow to test the full roundtrip, including the one time init
                (TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetGlobalSettingsOptions().DisableTelemetry = false;
                (TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).HookupTelemetryManager();
                ((TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).GetTelemetryManager() as TestTelemetryManager).TelemetryEvent = (properties, eventName) =>
                {
                    if (eventName == "Init")
                    {
                        Assert.IsTrue(properties.ContainsKey("PnPCoreSDKVersion"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["PnPCoreSDKVersion"]));
                        // Don't test on this as this might fail if the test config has turned off telemetry in combination with running multiple tests at once
                        //Assert.IsTrue(properties.ContainsKey("AADTenantId"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["AADTenantId"]));
                        Assert.IsFalse(Guid.Parse(properties["AADTenantId"]).Equals(Guid.Empty));
                        Assert.IsTrue(properties.ContainsKey("OS"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["OS"]));
                    }
                    else if (eventName == "Request")
                    {
                        requestPassed = true;
                        numberOfRequestsPassed++;
                        Assert.IsTrue(properties.ContainsKey("PnPCoreSDKVersion"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["PnPCoreSDKVersion"]));
                        Assert.IsTrue(properties.ContainsKey("AADTenantId"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["AADTenantId"]));
                        // Don't test on this as this might fail if the test config has turned off telemetry in combination with running multiple tests at once
                        //Assert.IsFalse(Guid.Parse(properties["AADTenantId"]).Equals(Guid.Empty));
                        Assert.IsTrue(properties.ContainsKey("OS"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["OS"]));
                        Assert.IsTrue(properties.ContainsKey("Model"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["Model"]));
                        Assert.IsTrue(properties.ContainsKey("ApiType"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["ApiType"]));
                        Assert.IsTrue(properties.ContainsKey("ApiMethod"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["ApiMethod"]));
                        Assert.IsTrue(properties.ContainsKey("GraphFirst"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["GraphFirst"]));
                        Assert.IsTrue(properties.ContainsKey("GraphCanUseBeta"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["GraphCanUseBeta"]));
                        Assert.IsTrue(properties.ContainsKey("GraphAlwaysUseBeta"));
                        Assert.IsTrue(!string.IsNullOrEmpty(properties["GraphAlwaysUseBeta"]));
                    }

                };

                // Init of the site collection will trigger the 
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    var web = await context.Web.GetAsync(p => p.WelcomePage);

                    // Don't verify if the init passed as when multiple tests are run other tests might have triggered this and 
                    // since it only fires once...this test might fail
                    Assert.IsTrue(requestPassed);
                    Assert.IsTrue(numberOfRequestsPassed == 3);
                }
            }
            finally
            {
                (TestCommon.Instance.BuildContextFactory() as TestPnPContextFactory).RemoveTelemetryManager();
            }
        }

    }
}
