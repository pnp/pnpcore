using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;

namespace PnP.Core.Modernization.Tests.Transform.OnPremises
{
    [TestClass]
    [TestCategory("Integration")]
    public class OnPremisesWebPartPageTests
    {
        [TestMethod]
        public void OnPremises_BasicWikiPageTest()
        {
            PageToTransform("WKP-2010-BasicTest");
        }

        [TestMethod]
        public void OnPremises_WebPartInWikiPageTest()
        {
            PageToTransform("WKP-2010-WebPartTest");
        }

        [TestMethod]
        public void OnPremises_FullArticleWikiPageTest()
        {
            PageToTransform("WKP-2010-Quantum");
        }

        [TestMethod]
        public void OnPremises_FullArticleWebPartPageTest()
        {
            PageToTransform("WPP-2010-Quantum");
        }

        [TestMethod]
        public void OnPremises_WebExtensions_GetSitePages()
        {
            using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
            {

                var result = sourceClientContext.Web.GetSitePagesLibrary();

                Assert.IsNotNull(result);
                Assert.AreNotEqual(default(List), result);

            }
        }

        [TestMethod]
        public void BasePage_ExtractWebPartPropertiesViaWebServicesFromWikiPageTest()
        {
            string url = "/sites/teamsite/SitePages/WKP-2010-Quantum.aspx";
            //string url = "/pages/article-2010-custom.aspx";

            using (var context = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
            {

                var pages = context.Web.GetPages("WKP-2010-Quantum");

                pages.FailTestIfZero();

                foreach (var page in pages)
                {
                    page.EnsureProperty(p => p.File);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.ExtractWebPartPropertiesViaWebServicesFromPage(url);

                    Assert.IsTrue(result.Length > 0);

                    break;

                }
            }

        }

        [TestMethod]
        public void BasePage_ExtractWebPartPropertiesViaWebServicesFromPageTest()
        {
            string url = "/sites/teamsite/SitePages/WPP-2010-Quantum.aspx";
            //string url = "/pages/article-2010-custom.aspx";

            using (var context = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
            {

                var pages = context.Web.GetPages("WPP-2010-Quantum");

                pages.FailTestIfZero();

                foreach (var page in pages)
                {
                    page.EnsureProperty(p => p.File);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.ExtractWebPartPropertiesViaWebServicesFromPage(url);

                 Assert.IsTrue(result.Length > 0);

                    break;

                }
            }

        }

        [TestMethod]
        public void BasePage_ExtractWebPartDocumentViaWebServicesFromWebPartPageTest()
        {
            string url = "/sites/teamsite/SitePages/WPP-2010-Quantum.aspx";
            //string url = "/pages/article-2010-custom.aspx";

            using (var context = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
            {

                var pages = context.Web.GetPages("WPP-2010-Quantum");

                pages.FailTestIfZero();

                foreach (var page in pages)
                {
                    page.EnsureProperty(p => p.File);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.ExtractWebPartDocumentViaWebServicesFromPage(url);

                    Assert.IsTrue(result.Item1.Length > 0);
                    Assert.IsTrue(result.Item2.Length > 0);

                    break;

                }
            }

        }


        [TestMethod]
        public void BasePage_LoadWebPartPropertiesViaWebServicesTest()
        {
            string url = "/sites/teamsite/SitePages/WPP-2010-Quantum.aspx";
            
            using (var context = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
            {

                var pages = context.Web.GetPages("WPP-2010-Quantum");

                pages.FailTestIfZero();

                foreach (var page in pages)
                {
                    page.EnsureProperty(p => p.File);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.LoadWebPartPropertiesFromWebServices(url);

                    break;
                    //TODO: Finish Test

                }
            }

        }
        

        /// <summary>
        /// Different page same test conditions
        /// </summary>
        /// <param name="pageName"></param>
        private void PageToTransform(string pageName)
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateOnPremisesClientContext(TestCommon.AppSetting("SPOnPremTeamSiteUrl")))
                {
                    var pageTransformator = new PageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPages(pageName);

                    pages.FailTestIfZero();

                    foreach (var page in pages)
                    {
                        PageTransformationInformation pti = new PageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                        };

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();

                    //TODO: Add Target Site Page Creation Checking
                }
            }
        }

    }

    
}
