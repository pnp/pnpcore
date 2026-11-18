using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;

namespace PnP.Core.Modernization.Tests.Transform.Wp
{
    [TestClass]
    [TestCategory("Integration")]
    public class WpTests
    {
        #region Test initialization
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //using (var cc = TestCommon.CreateClientContext())
            //{
            //    // Clean all migrated pages before next run
            //    var pages = cc.Web.GetPages("Migrated_");

            //    foreach (var page in pages.ToList())
            //    {
            //        page.DeleteObject();
            //    }

            //    cc.ExecuteQueryRetry();
            //}
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {

        }
        #endregion

        [TestMethod]
        public void RunWPTest()
        {
            using (var cc = TestCommon.CreateClientContext())
            {
                var pageTransformator = new PageTransformator(cc);
                pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose:true, includeDebugEntries:true));
                pageTransformator.RegisterObserver(new ConsoleObserver());

                var pages = WebExtensions.GetPages(cc.Web,"wp_");
                //var pages = cc.Web.GetPages("pagein", "folder1/sub1");
                //var pages = cc.Web.GetPagesFromList("SiteAssets", "loc_", "Folder1");
                foreach (IListItem page in pages)
                {
                    PageTransformationInformation pti = new PageTransformationInformation(page)
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,

                        // Don't log test runs
                        SkipTelemetry = true,

                        //RemoveEmptySectionsAndColumns = false,

                        // ModernizationCenter options
                        //ModernizationCenterInformation = new ModernizationCenterInformation()
                        //{
                        //    AddPageAcceptBanner = true
                        //},

                        // Migrated page gets the name of the original page
                        //TargetPageTakesSourcePageName = true,

                        // Give the migrated page a specific prefix, default is Migrated_
                        //TargetPagePrefix = "Yes_",

                        // Configure the page header, empty value means ClientSidePageHeaderType.None
                        //PageHeader = new ClientSidePageHeader(cc, ClientSidePageHeaderType.None, null),

                        // If the page is a home page then replace with stock home page
                        //ReplaceHomePageWithDefaultHomePage = true,

                        // Replace embedded images and iframes with a placeholder and add respective images and video web parts at the bottom of the page
                        //HandleWikiImagesAndVideos = false,

                        // Callout to your custom code to allow for title overriding
                        //PageTitleOverride = titleOverride,

                        // Callout to your custom layout handler
                        //LayoutTransformatorOverride = layoutOverride,

                        // Callout to your custom content transformator...in case you fully want replace the model
                        //ContentTransformatorOverride = contentOverride,
                    };

                    pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                    pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                    pageTransformator.Transform(pti);
                }

                pageTransformator.FlushObservers();
            }
        }

    }
}
