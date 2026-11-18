using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry.Observers;
using PnP.Core.Modernization.Transform;
using Microsoft.SqlServer.Server;

namespace PnP.Core.Modernization.Tests.Transform.Mapping
{
    [TestClass]
    [TestCategory("Integration")]
    public class UrlMappingTests
    {
        [TestMethod]
        public void UrlMappingFileLoadTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUrlMappingFile(@"..\..\Transform\Mapping\urlmapping_sample.csv");

            Assert.IsTrue(mapping.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UrlMappingFileNotFoundTest()
        {
            FileManager fm = new FileManager();
            var mapping = fm.LoadUrlMappingFile(@"..\..\Transform\Mapping\idontexist_sample.csv");
        }

        [TestMethod]
        public void PublishingPageUrlTest()
        {
            using (var targetClientContext = TestCommon.CreateClientContext("https://capadevtest.sharepoint.com/sites/PnPSauceModern"))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    var pageTransformator = new PublishingPageTransformator(sourceClientContext, targetClientContext);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: "c:\\temp", includeVerbose: true));
                    //using var reportContext = PnP.Framework.PnPCoreSdk.Instance.GetPnPContext(targetClientContext);
                    //pageTransformator.RegisterObserver(new MarkdownToSharePointObserver(reportContext.Web, includeVerbose: true));

                    var pages = sourceClientContext.Web.GetPagesFromList("Pages", folder: "News", pageNameStartsWith: "Hot-Off-The-Press");

                    foreach (IListItem page in pages)
                    {
                        PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,
                            // Don't log test runs
                            SkipTelemetry = true,

                            KeepPageCreationModificationInformation = false,
                        };

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);
                    }

                    pageTransformator.FlushObservers();
                }
            }
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_SrcSubSite()
        {

            var input = "/sites/PnPSauce/en/Pages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce/en";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "/sites/PnPSauceModern/sitepages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_SrcRoot()
        {

            var input = "/sites/PnPSauce/Pages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "/sites/PnPSauceModern/sitepages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_SrcAbs()
        {

            var input = "https://capadevtest.sharepoint.com/sites/PnPSauce/Pages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "https://capadevtest.sharepoint.com/sites/PnPSauceModern/sitepages/The-Cherry-on-the-Cake,-Transforming-to-Modern.aspx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_SiteOnly()
        {

            var input = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_SiteOnlyWithSlash()
        {

            var input = "https://capadevtest.sharepoint.com/sites/PnPSauce/";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "https://capadevtest.sharepoint.com/sites/PnPSauceModern/";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_Doc()
        {

            var input = "https://capadevtest.sharepoint.com/sites/PnPSauce/Documents/Employee-Handbook.docx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "https://capadevtest.sharepoint.com/sites/PnPSauceModern/Documents/Employee-Handbook.docx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_DocSubSite()
        {

            var input = "/sites/PnPSauce/en/Documents/Employee-Handbook.docx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce/en";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "/sites/PnPSauceModern/Documents/Employee-Handbook.docx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        [TestMethod]
        public void UrlTransformatorRewriteTest_DocFolder()
        {

            var input = "/sites/PnPSauce/Documents/Folder/Employee-Handbook.docx";
            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = "/sites/PnPSauceModern/Documents/Folder/Employee-Handbook.docx";

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }


        [TestMethod]
        public void UrlTransformatorRewriteTest_HtmlBody()
        {

            var input = @"<div data-automation-id=""textBox"" class=""cke_editable rte--read isFluentRTE cke_editableBulletCounterReset cke_editable cke_editableBulletCounterReset rteEmphasis root-305""><p><span></span></p><p style=""margin - left:0; text - align:left; "">Capsaicin is produced by the plant as a defense against mammalian predators and <a title=""Microbe"" class=""mw - redirect"" href=""https://en.wikipedia.org/wiki/Microbe"">microbes</a>, in particular a <a title=""Fusarium"" href=""https://en.wikipedia.org/wiki/Fusarium"">fusarium</a> fungus carried by <a title=""Hemipteran"" class=""mw-redirect"" 
                            href=""https://en.wikipedia.org/wiki/Hemipteran"">hemipteran</a><span> insects that attack certain species of chili peppers, according to one study. Peppers increased the quantity of capsaicin in proportion to the damage caused by fungal predation on the plant's seeds. This is <a href=""/sites/PnPSauce/Documents/Employee%20Handbook.docx"">another link</a> to the document.</span></p><span><p style=""margin-left:0;text-align:left;"">Source:&nbsp;https://en.wikipedia.org/wiki/Chili_pepper&nbsp;</p></span><p></p></div>";

            var sourceSite = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var sourceWeb = "https://capadevtest.sharepoint.com/sites/PnPSauce";
            var targetWeb = "https://capadevtest.sharepoint.com/sites/PnPSauceModern";
            var pagesLibrary = "pages";

            // Must be relative result
            var expected = @"<div data-automation-id=""textBox"" class=""cke_editable rte--read isFluentRTE cke_editableBulletCounterReset cke_editable cke_editableBulletCounterReset rteEmphasis root-305""><p><span></span></p><p style=""margin - left:0; text - align:left; "">Capsaicin is produced by the plant as a defense against mammalian predators and <a title=""Microbe"" class=""mw - redirect"" href=""https://en.wikipedia.org/wiki/Microbe"">microbes</a>, in particular a <a title=""Fusarium"" href=""https://en.wikipedia.org/wiki/Fusarium"">fusarium</a> fungus carried by <a title=""Hemipteran"" class=""mw-redirect"" 
                            href=""https://en.wikipedia.org/wiki/Hemipteran"">hemipteran</a><span> insects that attack certain species of chili peppers, according to one study. Peppers increased the quantity of capsaicin in proportion to the damage caused by fungal predation on the plant's seeds. This is <a href=""/sites/PnPSauceModern/Documents/Employee%20Handbook.docx"">another link</a> to the document.</span></p><span><p style=""margin-left:0;text-align:left;"">Source:&nbsp;https://en.wikipedia.org/wiki/Chili_pepper&nbsp;</p></span><p></p></div>"; ;

            CommonUrlReWriteTest(input, sourceSite, sourceWeb, targetWeb, pagesLibrary, expected);
        }

        /// <summary>
        /// Common Test for URL Rewriting
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sourceSite"></param>
        /// <param name="sourceWeb"></param>
        /// <param name="targetWeb"></param>
        /// <param name="pagesLibrary"></param>
        /// <param name="expected"></param>
        public void CommonUrlReWriteTest(string input, string sourceSite, string sourceWeb, string targetWeb, string pagesLibrary, string expected)
        {
            //Pre-requisite objects
            using (var targetClientContext = TestCommon.CreateClientContext("https://capadevtest.sharepoint.com/sites/PnPSauceModern"))
            {
                using (var sourceClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPODevSiteUrl")))
                {
                    PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation()
                    {
                        // If target page exists, then overwrite it
                        Overwrite = true,
                        // Don't log test runs
                        SkipTelemetry = true,

                        KeepPageCreationModificationInformation = false,
                    };

                    UrlTransformator urlTransform = new UrlTransformator(pti, sourceClientContext, targetClientContext);
                                       
                    var result = urlTransform.ReWriteUrls(input, sourceSite, sourceWeb, targetWeb, pagesLibrary);

                    Assert.AreEqual(expected, result);
                }
            }


        }
    }
}
