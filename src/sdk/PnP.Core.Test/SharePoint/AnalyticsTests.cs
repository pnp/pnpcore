using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class AnalyticsTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;            
        }

        [TestMethod]
        public async Task GetSiteAnalytics()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var analytics = context.Site.GetAnalytics();

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 1);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount > 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount > 0);
                Assert.IsTrue(analyticsForSite.StartDateTime == DateTime.MinValue);
                Assert.IsTrue(analyticsForSite.EndDateTime == DateTime.MinValue);
            }
        }

        [TestMethod]
        public async Task GetSiteAnalyticsLastWeek()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var analytics = context.Site.GetAnalytics(new AnalyticsOptions { Interval = AnalyticsInterval.LastSevenDays });

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 1);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount > 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount > 0);

                Assert.IsTrue(analyticsForSite.EndDateTime.Subtract(analyticsForSite.StartDateTime).Days + 1 == 7);
            }
        }

        [TestMethod]
        public async Task GetSiteAnalyticsCustomByDay()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                DateTime startDate;
                DateTime endDate;
                if (!TestCommon.Instance.Mocking)
                {
                    startDate = DateTime.Now - new TimeSpan(20, 0, 0, 0);
                    endDate = DateTime.Now - new TimeSpan(10, 0, 0, 0);
                    Dictionary<string, string> properties = new Dictionary<string, string>
                    {
                        { "StartDate", startDate.Ticks.ToString() },
                        { "EndDate", endDate.Ticks.ToString() },
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    startDate = new DateTime(long.Parse(properties["StartDate"]));
                    endDate = new DateTime(long.Parse(properties["EndDate"]));
                }


                var analytics = context.Site.GetAnalytics(new AnalyticsOptions
                {
                    Interval = AnalyticsInterval.Custom,
                    CustomStartDate = startDate,
                    CustomEndDate = endDate,
                    CustomAggregationInterval = AnalyticsAggregationInterval.Day
                }); 

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 11);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount >= 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount >= 0);

                // Since we've a daily aggregation
                Assert.IsTrue(analyticsForSite.EndDateTime.Subtract(analyticsForSite.StartDateTime).Days + 1 == 1);
            }
        }

        [TestMethod]
        public async Task GetFileAnalytics()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = context.Web.GetFileByServerRelativeUrl($"{context.Uri.AbsolutePath}/sitepages/home.aspx", p => p.VroomItemID, p => p.VroomDriveID);

                var analytics = file.GetAnalytics();

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 1);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount >= 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount >= 0);
                Assert.IsTrue(analyticsForSite.StartDateTime == DateTime.MinValue);
                Assert.IsTrue(analyticsForSite.EndDateTime == DateTime.MinValue);
            }
        }

        [TestMethod]
        public async Task GetFileAnalyticsLastWeek()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var file = context.Web.GetFileByServerRelativeUrl($"{context.Uri.AbsolutePath}/sitepages/home.aspx", p => p.VroomItemID, p => p.VroomDriveID);

                var analytics = file.GetAnalytics(new AnalyticsOptions { Interval = AnalyticsInterval.LastSevenDays });

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 1);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount > 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount > 0);

                Assert.IsTrue(analyticsForSite.EndDateTime.Subtract(analyticsForSite.StartDateTime).Days + 1 == 7);
            }
        }

        [TestMethod]
        public async Task GetFileAnalyticsCustomByDay()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                DateTime startDate;
                DateTime endDate;
                if (!TestCommon.Instance.Mocking)
                {
                    startDate = DateTime.Now - new TimeSpan(20, 0, 0, 0);
                    endDate = DateTime.Now - new TimeSpan(10, 0, 0, 0);
                    Dictionary<string, string> properties = new Dictionary<string, string>
                    {
                        { "StartDate", startDate.Ticks.ToString() },
                        { "EndDate", endDate.Ticks.ToString() },
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    startDate = new DateTime(long.Parse(properties["StartDate"]));
                    endDate = new DateTime(long.Parse(properties["EndDate"]));
                }

                var file = context.Web.GetFileByServerRelativeUrl($"{context.Uri.AbsolutePath}/sitepages/home.aspx", p => p.VroomItemID, p => p.VroomDriveID);

                var analytics = file.GetAnalytics(new AnalyticsOptions
                {
                    Interval = AnalyticsInterval.Custom,
                    CustomStartDate = startDate,
                    CustomEndDate = endDate,
                    CustomAggregationInterval = AnalyticsAggregationInterval.Day
                });

                Assert.IsNotNull(analytics);
                Assert.IsTrue(analytics.Count == 11);

                var analyticsForSite = analytics.First();

                Assert.IsTrue(analyticsForSite.Access.ActionCount >= 0);
                Assert.IsTrue(analyticsForSite.Access.ActorCount >= 0);

                // Since we've a daily aggregation
                Assert.IsTrue(analyticsForSite.EndDateTime.Subtract(analyticsForSite.StartDateTime).Days + 1 == 1);
            }
        }
    }
}
