using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry.Observers;
using Microsoft.SharePoint.Client;
using static PnP.Core.Modernization.Tests.TestCommon;
using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Telemetry;

namespace PnP.Core.Modernization.Tests.Transform.CommonTests
{
    /// <summary>
    /// Summary description for CommonSP_PublishingPages
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class CommonSPPublishingPages
    {
        #region Test Config

        public CommonSPPublishingPages()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #endregion

        #region SharePoint 2010 Tests

        [TestCategory(TestCategories.SP2010)]
        [TestMethod]
        public void AllCommonPublishingPages_SP2010()
        {
            TransformPage(SPPlatformVersion.SP2010);
        }

        [TestCategory(TestCategories.SP2010)]
        [TestMethod]
        public void ProcessedWebPartDocumentWebServices_SP2010()
        {
            LoadWebPartDocumentViaWebServicesTest(SPPlatformVersion.SP2010);
        }

        [TestCategory(TestCategories.SP2010)]
        [TestMethod]
        public void RawExtractWebPartDocumentViaWebServices_SP2010()
        {
            ExtractWebPartDocumentViaWebServicesFromPageTest(SPPlatformVersion.SP2010);
        }

        #endregion

        #region SharePoint 2013 Tests

        [TestCategory(TestCategories.SP2013)]
        [TestMethod]
        public void AllCommonPublishingPages_SP2013()
        {
            TransformPage(SPPlatformVersion.SP2013);
        }

        [TestCategory(TestCategories.SP2013)]
        [TestMethod]
        public void ProcessedWebPartDocumentWebServicess_SP2013()
        {
            LoadWebPartDocumentViaWebServicesTest(SPPlatformVersion.SP2013);
        }

        [TestCategory(TestCategories.SP2013)]
        [TestMethod]
        public void RawExtractWebPartDocumentViaWebServices_SP2013()
        {
            ExtractWebPartDocumentViaWebServicesFromPageTest(SPPlatformVersion.SP2013);
        }

        #endregion

        #region SharePoint 2016 Tests

        [TestCategory(TestCategories.SP2016)]
        [TestMethod]
        public void AllCommonPublishingPages_SP2016()
        {
            TransformPage(SPPlatformVersion.SP2016);
        }

        [TestCategory(TestCategories.SP2016)]
        [TestMethod]
        public void ProcessedWebPartDocumentWebServices_SP2016()
        {
            LoadWebPartDocumentViaWebServicesTest(SPPlatformVersion.SP2016);
        }

        [TestCategory(TestCategories.SP2016)]
        [TestMethod]
        public void RawExtractWebPartDocumentViaWebServices_SP2016()
        {
            ExtractWebPartDocumentViaWebServicesFromPageTest(SPPlatformVersion.SP2016);
        }

        #endregion

        #region SharePoint 2019 Tests

        [TestCategory(TestCategories.SP2019)]
        [TestMethod]
        public void AllCommonPublishingPages_SP2019()
        {
            TransformPage(SPPlatformVersion.SP2019);
        }

        [TestCategory(TestCategories.SP2019)]
        [TestMethod]
        public void ProcessedWebPartDocumentWebServices_SP2019()
        {
            LoadWebPartDocumentViaWebServicesTest(SPPlatformVersion.SP2019);
        }

        [TestCategory(TestCategories.SP2019)]
        [TestMethod]
        public void RawExtractWebPartDocumentViaWebServices_SP2019()
        {
            ExtractWebPartDocumentViaWebServicesFromPageTest(SPPlatformVersion.SP2019);
        }

        #endregion

        #region SharePoint Online Tests

        [TestCategory(TestCategories.SPO)]
        [TestMethod]
        public void AllCommonPublishingPages_SPO()
        {
            TransformPage(SPPlatformVersion.SPO);
        }

        #endregion

        #region Test Code

        // Common Tests

        /// <summary>
        /// Standard Transform Test
        /// </summary>
        /// <param name="version"></param>
        /// <param name="fullPageLayoutMapping"></param>
        /// <param name="pageNameStartsWith"></param>
        private void TransformPage(SPPlatformVersion version, string fullPageLayoutMapping = "", string pageNameStartsWith = "Common")
        {

            using (var targetClientContext = TestCommon.CreateClientContext(TestCommon.AppSetting("SPOTargetSiteUrl")))
            {
                using (var sourceClientContext = TestCommon.CreateSPPlatformClientContext(version, TransformType.PublishingPage))
                {

                    var  pageTransformator = new PublishingPageTransformator(sourceClientContext, targetClientContext, fullPageLayoutMapping);
                    pageTransformator.RegisterObserver(new MarkdownObserver(folder: TestContext.ResultsDirectory, includeVerbose: true));
                    pageTransformator.RegisterObserver(new UnitTestLogObserver());

                    var pages = sourceClientContext.Web.GetPagesFromList("Pages", pageNameStartsWith);
                    pages.FailTestIfZero();

                    foreach (var page in pages)
                    {
                        var pageName = page.FieldValues["FileLeafRef"].ToString();
                            
                        PublishingPageTransformationInformation pti = new PublishingPageTransformationInformation(page)
                        {
                            // If target page exists, then overwrite it
                            Overwrite = true,

                            // Don't log test runs
                            SkipTelemetry = true,

                            //Permissions are unlikely to work given cross domain
                            KeepPageSpecificPermissions = false,

                            //Update target to include SP version
                            TargetPageName = TestCommon.UpdatePageToIncludeVersion(version, pageName)
                        };

                        Console.WriteLine("SharePoint Version: {0}", pti.SourceVersion);

                        pti.MappingProperties["SummaryLinksToQuickLinks"] = "true";
                        pti.MappingProperties["UseCommunityScriptEditor"] = "true";

                        var result = pageTransformator.Transform(pti);

                        Assert.IsTrue(!string.IsNullOrEmpty(result));
                    }

                    pageTransformator.FlushObservers();

                }
            }
        }

        /// <summary>
        /// Standard Load Web Part Document with Web Services Test
        /// </summary>
        /// <param name="version"></param>
        /// <param name="pageNameStartsWith"></param>
        private void LoadWebPartDocumentViaWebServicesTest(SPPlatformVersion version, string pageNameStartsWith = "Common")
        {
            using (var context = TestCommon.CreateSPPlatformClientContext(version, TransformType.PublishingPage))
            {

                var pages = context.Web.GetPagesFromList("Pages", pageNameStartsWith);

                foreach (var page in pages)
                {
                    page.EnsureProperties(p => p.File, p => p.File.ServerRelativeUrl);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.LoadPublishingPageFromWebServices(page.File.ServerRelativeUrl);

                    Assert.IsTrue(result.Count > 0);

                }
            }

        }

        /// <summary>
        /// Export workaround tests
        /// </summary>
        /// <param name="version"></param>
        /// <param name="pageNameStartsWith"></param>
        private void ExportWebPartByWorkaround(SPPlatformVersion version, string pageNameStartsWith = "Common")
        {
            using (var context = TestCommon.CreateSPPlatformClientContext(version, TransformType.PublishingPage))
            {

                var pages = context.Web.GetPagesFromList("Pages", pageNameStartsWith);

                foreach (var page in pages)
                {
                    page.EnsureProperties(p => p.File, p => p.File.ServerRelativeUrl);

                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var webPartEntities = testBase.LoadPublishingPageFromWebServices(page.File.ServerRelativeUrl);

                    foreach (var webPart in webPartEntities)
                    {
                        var result = testBase.ExportWebPartXmlWorkaround(page.File.ServerRelativeUrl, webPart.Id.ToString());

                        Assert.IsTrue(!string.IsNullOrEmpty(result));

                    }

                }
            }

        }

        /// <summary>
        /// Call SharePoint Web Servics for Web Part Document
        /// </summary>
        /// <param name="version"></param>
        /// <param name="pageNameStartsWith"></param>
        public void ExtractWebPartDocumentViaWebServicesFromPageTest(SPPlatformVersion version, string pageNameStartsWith = "Common")
        {
            using (var context = TestCommon.CreateSPPlatformClientContext(version, TransformType.PublishingPage))
            {

                var pages = context.Web.GetPagesFromList("Pages", pageNameStartsWith);

                foreach (var page in pages)
                {
                    page.EnsureProperties(p => p.File, p => p.File.ServerRelativeUrl);

                    List<string> search = new List<string>()
                    {
                        "WebPartZone"
                    };

                    //Should be one
                    TestBasePage testBase = new TestBasePage(page, page.File, null, null);
                    var result = testBase.ExtractWebPartDocumentViaWebServicesFromPage(page.File.ServerRelativeUrl);

                    Assert.IsTrue(result.Item1.Length > 0);
                    Assert.IsTrue(result.Item2.Length > 0);
                }
            }
        }

        #endregion

    }

    /// <summary>
    /// Test class to access base page methods
    /// </summary>
    public class TestBasePage : BasePage
    {
        public TestBasePage(ListItem item, File file, PageTransformation pt, IList<ILogObserver> logObservers) : base(item, file, pt, logObservers)
        {

        }
    }


}
